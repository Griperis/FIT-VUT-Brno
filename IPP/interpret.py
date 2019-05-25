# !/usr/bin/env python3

# IPP Project part 2
# Author: Zdenek Dolezal
# Login: xdolez82
# Faculty of information technology BUT

from enum import Enum
from getopt import getopt
from getopt import GetoptError
import re
import sys
import xml.etree.ElementTree as ET


class CONST:
    """
    Constants used all over the program includes returncode and instruction set
    """
    OK = 0
    ERROR_PARAM = 10
    ERROR_FOPENR = 11
    ERROR_FOPENW = 12

    ERROR_XML_FORMAT = 31
    ERROR_XML_STRUCT = 32

    ERROR_SEMANTIC = 52
    RUN_ERROR_OPERAND_TYPE = 53
    RUN_ERROR_NON_EX_VARIABLE = 54
    RUN_ERROR_NON_EX_FRAME = 55
    RUN_ERROR_NO_VALUE = 56
    RUN_ERROR_OPERAND = 57
    RUN_ERROR_STRING = 58
    RUN_ERROR_REDEFINITION = 59
    ERROR_INTERNAL = 99

    # Other constants
    MAX_ARGUMENTS = 3
    MIN_RETVAL = 0
    MAX_RETVAL = 49

    # Instruction set
    INSTRUCTION_SET = {
        "MOVE": ["var", "symb"],
        "CREATEFRAME": [],
        "PUSHFRAME": [],
        "POPFRAME": [],
        "DEFVAR": ["var"],
        "CALL": ["label"],
        "RETURN": [],
        "PUSHS": ["symb"],
        "POPS": ["var"],
        "ADD": ["var", "symb", "symb"],
        "SUB": ["var", "symb", "symb"],
        "MUL": ["var", "symb", "symb"],
        "IDIV": ["var", "symb", "symb"],
        "LT": ["var", "symb", "symb"],
        "GT": ["var", "symb", "symb"],
        "EQ": ["var", "symb", "symb"],
        "AND": ["var", "symb", "symb"],
        "OR": ["var", "symb", "symb"],
        "NOT": ["var", "symb"],
        "INT2CHAR": ["var", "symb"],
        "STRI2INT": ["var", "symb", "symb"],
        "READ": ["var", "type"],
        "WRITE": ["symb"],
        "CONCAT": ["var", "symb", "symb"],
        "STRLEN": ["var", "symb"],
        "GETCHAR": ["var", "symb", "symb"],
        "SETCHAR": ["var", "symb", "symb"],
        "TYPE": ["var", "symb"],
        "LABEL": ["label"],
        "JUMP": ["label"],
        "JUMPIFEQ": ["label", "symb", "symb"],
        "JUMPIFNEQ": ["label", "symb", "symb"],
        "EXIT": ["symb"],
        "DPRINT": ["symb"],
        "BREAK": []
    }


class Statistics:
    """
    Implements statistics extension, contains information about number of executed
    instructions and maximal number of intialized variable
    """
    options = []
    filePath = None
    executedInstructions = 0
    maxInitializedVars = 0
    
    @staticmethod
    def incExecutedInstructions():
        Statistics.executedInstructions += 1

    @staticmethod
    def incMaxInitializedVars(memoryModel):
        def countVarsInFrame(frame):
            if frame is None:
                return 0
            count = 0
            for var in frame.variables:
                if var.type is not Variable.Type.NONETYPE:
                    count += 1
            return count

        def countVarsInMemory():
            result = 0
            if len(memoryModel.stack) != 0:
                result += countVarsInFrame(memoryModel.stack[len(memoryModel.stack) - 1])
            result += countVarsInFrame(memoryModel.tmpFrame)
            result += countVarsInFrame(memoryModel.globalFrame)
            return result


        varCount = countVarsInMemory()
        if varCount > Statistics.maxInitializedVars:
            Statistics.maxInitializedVars = varCount
        
    @staticmethod
    def writeToFile():
        try:
            file = open(Statistics.filePath, 'w')
            for opt in Statistics.options:
                if opt == "insts":
                    file.write(str(Statistics.executedInstructions) + "\n")
                elif opt == "vars":
                    file.write(str(Statistics.maxInitializedVars) + "\n")

        except Exception as e:
            Print.error("Couldn't open file for statistics: " + str(e))
            sys.exit(CONST.ERROR_FOPENW)
        finally:
            if file is not None:
                file.close()


class Program:
    """
    Internal representation of code in IPPCode19 alreadz parsed by XMLParser,
    implements all methods needed to interpret the code and manages its MemoryModel
    """
    def __init__(self, input, stats=None):
        self.instructions = []
        self.instListIndex = 0
        self.labels = {}
        self.memoryModel = MemoryModel()
        self.inputStream = InputStream(input)
        self.callStack = []

    def setInstructions(self, instructionList):
        self.instructions = instructionList

    def __repr__(self):
        retval = ""
        for inst in self.instructions:
            retval += str(inst) + "\n"
        return retval

    def interpret(self):
        """
        Calls inner execution method for every instruction in self.instructions and
        manages internal program counters
        """
        def move(leftOperand, rightOperand):
            leftVar = self.memoryModel.getVariable(*leftOperand.splitVariable())
            if leftVar is None:
                Print.error("Cannot assign to undefined variable: " + leftOperand.getVariableName())
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if rightOperand.type == Argument.Type.VAR:
                rightVar = self.memoryModel.getVariable(*rightOperand.splitVariable())
                self.isVariableReadable(rightVar, rightOperand.getVariableName())
                leftVar.copy(rightVar)
            elif rightOperand.isConst():
                leftVar.insert(self.mapArgTypeToVarType(rightOperand.type), rightOperand.value)

        def defvar(var):
            frameType = self.memoryModel.convertStringToFrameType(inst.args[0].getVariableFrame())
            var = Variable(inst.args[0].getVariableName())
            self.memoryModel.addVariable(var, frameType)

        def call(label):
            self.callStack.append(self.instListIndex)
            jumpTo(label)

        def ret():
            if len(self.callStack) > 0:
                self.instListIndex = self.callStack.pop()
            else:
                Print.error("Callstack is empty")
                sys.exit(CONST.RUN_ERROR_NO_VALUE)

        def push(symb):
            if symb.type == Argument.Type.VAR:
                var = self.memoryModel.getVariable(*symb.splitVariable())
                self.isVariableReadable(var)
                type = var.type
                value = var.value
            elif symb.isConst():
                type = self.mapArgTypeToVarType(symb.type)
                value = symb.value
            else:
                Print.error("Invalid symbol to push")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            varToPush = Variable("stack")
            varToPush.insert(type, value)
            self.memoryModel.dataStack.push(varToPush)

        def pop(var):
            stackValue = self.memoryModel.dataStack.pop()
            var = self.memoryModel.getVariable(*var.splitVariable())
            if var is None:
                Print.error("Cannot pop into undefined variable")
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)
            var.insert(stackValue.type, stackValue.value)

        def chekALOps(destination, leftOperand, rightOperand, type):
            destinationVar, leftOperand, rightOperand = self.parse3Operands(destination, leftOperand, rightOperand)
            if self.checkTypeEquality(leftOperand, rightOperand, expectedType=type) is False:
                Print.error("Incompatible type for arithmetic-logic operation")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            return destinationVar, leftOperand, rightOperand

        def add(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.INT)
            destVar.insert(Variable.Type.INT, int(leftOp.value + rightOp.value))

        def sub(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.INT)
            destVar.insert(Variable.Type.INT, int(leftOp.value - rightOp.value))

        def mul(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.INT)
            destVar.insert(Variable.Type.INT, int(leftOp.value * rightOp.value))

        def idiv(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.INT)
            if rightOp.value == 0:
                Print.error("Division by zero")
                sys.exit(CONST.RUN_ERROR_OPERAND)
            destVar.insert(Variable.Type.INT, int(leftOp.value // rightOp.value))

        def relationComparison(opcode, destination, leftOperand, rightOperand):
            destinationVar, leftOperand, rightOperand = self.parse3Operands(destination, leftOperand, rightOperand)
            if (opcode == "EQ" and (self.mapArgTypeToVarType(leftOperand.type) == Variable.Type.NIL or
                                    self.mapArgTypeToVarType(rightOperand.type) == Variable.Type.NIL)):
                if leftOperand.type == rightOperand.type:
                    destinationVar.insert(Variable.Type.BOOL, True)
                else:
                    destinationVar.insert(Variable.Type.BOOL, False)
                return
            if self.checkTypeEquality(leftOperand, rightOperand) is False:
                Print.error("Types for relation comparison have to be equal")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if self.mapArgTypeToVarType(leftOperand.type) == Variable.Type.NIL and opcode != "EQ":
                Print.error("Operands of type nil only usable with EQ")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if opcode == "LT":
                if leftOperand.value < rightOperand.value:
                    destinationVar.insert(Variable.Type.BOOL, True)
                else:
                    destinationVar.insert(Variable.Type.BOOL, False)
            elif opcode == "GT":
                if leftOperand.value > rightOperand.value:
                    destinationVar.insert(Variable.Type.BOOL, True)
                else:
                    destinationVar.insert(Variable.Type.BOOL, False)
            elif opcode == "EQ":
                if leftOperand.value == rightOperand.value:
                    destinationVar.insert(Variable.Type.BOOL, True)
                else:
                    destinationVar.insert(Variable.Type.BOOL, False)
            else:
                Print.error("Invalid opcode in relation comparison")
                sys.exit(CONST.ERROR_INTERNAL)

        def andInst(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.BOOL)
            destVar.insert(Variable.Type.BOOL, leftOp.value and rightOp.value)

        def orInst(destination, leftOperand, rightOperand):
            destVar, leftOp, rightOp = chekALOps(destination, leftOperand, rightOperand, Variable.Type.BOOL)
            destVar.insert(Variable.Type.BOOL, leftOp.value or rightOp.value)

        def notInst(destination, leftOperand):
            destVar = self.memoryModel.getVariable(*destination.splitVariable())
            if destVar is None:
                Print.error("Cannot save not result")
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if leftOperand.type == Argument.Type.VAR:
                leftOperand = self.memoryModel.getVariable(*leftOperand.splitVariable())
                self.isVariableReadable(leftOperand)

            if self.mapArgTypeToVarType(leftOperand.type) != Variable.Type.BOOL:
                Print.error("Incompatible type")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)
            destVar.insert(Variable.Type.BOOL, not leftOperand.value)

        def int2char(destination, leftOperand):
            destinationVar = self.memoryModel.getVariable(*destination.splitVariable())
            if destinationVar is None:
                Print.error("Cannot save int2char result into undefined variable")
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if leftOperand.type == Argument.Type.VAR:
                leftOperand = self.memoryModel.getVariable(*leftOperand.splitVariable())
                self.isVariableReadable(leftOperand)

            if self.mapArgTypeToVarType(leftOperand.type) != Variable.Type.INT:
                Print.error("Incompatible type for int2char")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)
            try:
                destinationVar.insert(Variable.Type.STRING, chr(leftOperand.value))
            except Exception as e:
                Print.error("Invalid range for instruction int2chr ended with exception: " + str(e))
                sys.exit(CONST.RUN_ERROR_STRING)

        def stri2int(destination, leftOperand, rightOperand):
            destinationVar, stringOperand, intOperand = self.parse3Operands(destination, leftOperand, rightOperand)

            if stringOperand.type != Variable.Type.STRING:
                Print.error("String expected")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if intOperand.type != Variable.Type.INT:
                Print.error("Integer expected as index value")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            try:
                destinationVar.insert(Variable.Type.INT, ord(stringOperand.value[intOperand.value]))
            except Exception as e:
                Print.error("Invalid value passed to stri2int ended with exception: " + str(e))
                sys.exit(CONST.RUN_ERROR_STRING)

        def read(destination, specifiedType):
            userInput = self.inputStream.getNextLine()
            leftVar = self.memoryModel.getVariable(*destination.splitVariable())
            if leftVar is None:
                Print.error("Cannot read into undefined variable " + destination.getVariableName())
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if specifiedType.value == "int":
                if userInput is not None and re.match(r"^(\+|-)?[0-9]+$", userInput):
                    leftVar.insert(Variable.Type.INT, int(userInput))
                else:
                    leftVar.insert(Variable.Type.INT, 0)
            elif specifiedType.value == "string":
                if userInput is not None and re.match(r"^([^#\\\s]|(\\[0-9]{3}))*?$", userInput):
                    leftVar.insert(Variable.Type.STRING, str(userInput))
                else:
                    leftVar.insert(Variable.Type.STRING, "")
            elif specifiedType.value == "bool":
                if userInput is not None and re.match(r"^true$", userInput, re.IGNORECASE):
                    leftVar.insert(Variable.Type.BOOL, True)
                else:
                    leftVar.insert(Variable.Type.BOOL, False)

        def write(symbol):
            if symbol.type == Argument.Type.VAR:
                varToPrint = self.memoryModel.getVariable(*symbol.splitVariable())
                self.isVariableReadable(varToPrint, symbol.getVariableName())
                if varToPrint.type == Variable.Type.BOOL:
                    print(str(varToPrint.value).lower(), end='')
                else:
                    print(str(varToPrint.value), end='')

            elif symbol.isConst():
                if symbol.type == Argument.Type.BOOL:
                    print(str(symbol.value).lower(), end='')
                else:
                    print(str(symbol.value), end='')
            else:
                Print.error("[INTERNAL] Unexpected symbol")
                sys.exit(CONST.ERROR_INTERNAL)

        def concat(destination, leftOperand, rightOperand):
            destinationVar, leftString, rightString = self.parse3Operands(destination, leftOperand, rightOperand)
            if self.checkTypeEquality(leftString, rightString, expectedType=Variable.Type.STRING) is False:
                Print.error("Cannot concatenate types other than string")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            destinationVar.insert(Variable.Type.STRING, leftString.value + rightString.value)

        def strlen(destination, leftOperand):
            destinationVar = self.memoryModel.getVariable(*destination.splitVariable())
            if destinationVar is None:
                Print.error("Cannot save string length into undefined variable")
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if leftOperand.type == Argument.Type.VAR:
                leftOperand = self.memoryModel.getVariable(*leftOperand.splitVariable())
                self.isVariableReadable(leftOperand)

            if self.mapArgTypeToVarType(leftOperand.type) != Variable.Type.STRING:
                Print.error("Operand of type string expected")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            destinationVar.insert(Variable.Type.INT, len(leftOperand.value))

        def getchar(destination, leftOperand, rightOperand):
            destinationVar, char, index = self.parse3Operands(destination, leftOperand, rightOperand)
            if self.mapArgTypeToVarType(char.type) != Variable.Type.STRING:
                Print.error("Cannot get character from other type than string")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if self.mapArgTypeToVarType(index.type) != Variable.Type.INT:
                Print.error("Cannot index character in string with type other than int")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if index.value < len(char.value) and index.value >= 0:
                destinationVar.insert(Variable.Type.STRING, char.value[index.value])
            else:
                Print.error("Invalid index")
                sys.exit(CONST.RUN_ERROR_STRING)

        def setchar(toModify, index, char):
            toModifyVar, index, char = self.parse3Operands(toModify, index, char)

            if self.mapArgTypeToVarType(toModifyVar.type) != Variable.Type.STRING:
                Print.error("String expected")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if self.mapArgTypeToVarType(index.type) != Variable.Type.INT:
                Print.error("Invalid type of index")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if self.mapArgTypeToVarType(char.type) != Variable.Type.STRING:
                Print.error("Modifying character has to be string")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if len(char.value) == 0:
                Print.error("No character to insert")
                sys.exit(CONST.RUN_ERROR_STRING)

            if index.value >= len(toModifyVar.value):
                Print.error("Cannot modify character that is out of index")
                sys.exit(CONST.RUN_ERROR_STRING)

            originalString = toModifyVar.value
            toModifyVar.insert(Variable.Type.STRING, originalString[:index.value] + char.value[0] + originalString[index.value + 1:])

        def type(var, symbol):
            destinationVar = self.memoryModel.getVariable(*var.splitVariable())
            if destinationVar is None:
                Print.error("Cannot save type into uninitialized variable")
                sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

            if symbol.type == Argument.Type.VAR:
                rightVar = self.memoryModel.getVariable(*symbol.splitVariable())
                if rightVar is None:
                    Print.error("Cannot resolve type of uninitialized variable")
                    sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)
                type = rightVar.type
            elif symbol.isConst():
                type = self.mapArgTypeToVarType(symbol.type)
            else:
                Print.error("[INTERNAL] Unexpected symbol")
                sys.exit(CONST.ERROR_INTERNAL)

            if type == Variable.Type.INT:
                destinationVar.insert(Variable.Type.STRING, "int")
            elif type == Variable.Type.STRING:
                destinationVar.insert(Variable.Type.STRING, "string")
            elif type == Variable.Type.BOOL:
                destinationVar.insert(Variable.Type.STRING, "bool")
            elif type == Variable.Type.NIL:
                destinationVar.insert(Variable.Type.STRING, "nil")
            elif type == Variable.Type.NONETYPE:
                destinationVar.insert(Variable.Type.STRING, "")
            else:
                Print.error("[INTERNAL] Unexpected variable")

        def jumpTo(destinationLabel):
            if destinationLabel.value not in self.labels:
                Print.error("Jump to undefined label: " + str(destinationLabel.value))
                sys.exit(CONST.ERROR_SEMANTIC)
            else:
                self.instListIndex = int(self.labels[destinationLabel.value]) - 1

        def jumpIfEq(destinationLabel, leftSymbol, rightSymbol):
            if leftSymbol.type == Argument.Type.VAR:
                leftSymbol = self.memoryModel.getVariable(*leftSymbol.splitVariable())
                self.isVariableReadable(leftSymbol)
            if rightSymbol.type == Argument.Type.VAR:
                rightSymbol = self.memoryModel.getVariable(*rightSymbol.splitVariable())
                self.isVariableReadable(rightSymbol)

            if self.checkTypeEquality(leftSymbol, rightSymbol) is False:
                Print.error("Types are not equal")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if leftSymbol.value == rightSymbol.value:
                jumpTo(destinationLabel)

        def jumpIfNotEq(destinationLabel, leftSymbol, rightSymbol):
            if leftSymbol.type == Argument.Type.VAR:
                leftSymbol = self.memoryModel.getVariable(*leftSymbol.splitVariable())
                self.isVariableReadable(leftSymbol)
            if rightSymbol.type == Argument.Type.VAR:
                rightSymbol = self.memoryModel.getVariable(*rightSymbol.splitVariable())
                self.isVariableReadable(rightSymbol)

            if self.checkTypeEquality(leftSymbol, rightSymbol) is False:
                Print.error("Types are not equal")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)

            if leftSymbol.value != rightSymbol.value:
                jumpTo(destinationLabel)

        def exit(returnCode):
            if returnCode.type == Argument.Type.VAR:
                returnCode = self.memoryModel.getVariable(*returnCode.splitVariable())
                self.isVariableReadable(returnCode)

            if self.mapArgTypeToVarType(returnCode.type) != Variable.Type.INT:
                Print.error("Invalid variable type")
                sys.exit(CONST.RUN_ERROR_OPERAND_TYPE)
            if returnCode.value >= CONST.MIN_RETVAL and returnCode.value <= CONST.MAX_RETVAL:
                sys.exit(returnCode.value)
            else:
                Print.error("Invalid return code")
                sys.exit(CONST.RUN_ERROR_OPERAND)

        def dprint(symbol):
            if symbol.type == Argument.Type.VAR:
                var = self.memoryModel.getVariable(*symbol.splitVariable())
                Print.debug(str(var))
            else:
                Print.debug(str(symbol))

        self.findLabels()
        # instruction interpretation cycle
        while self.instListIndex < len(self.instructions):
            inst = self.instructions[self.instListIndex]
            Print.setInfo(inst.order, inst.opcode)
            if inst.opcode == "MOVE":
                move(inst.args[0], inst.args[1])
            elif inst.opcode == "CREATEFRAME":
                self.memoryModel.createFrame()
            elif inst.opcode == "PUSHFRAME":
                self.memoryModel.pushFrame()
            elif inst.opcode == "POPFRAME":
                self.memoryModel.popFrame()
            elif inst.opcode == "DEFVAR":
                defvar(inst.args[0])
            elif inst.opcode == "CALL":
                call(inst.args[0])
            elif inst.opcode == "RETURN":
                ret()
            elif inst.opcode == "PUSHS":
                push(inst.args[0])
            elif inst.opcode == "POPS":
                pop(inst.args[0])
            elif inst.opcode == "ADD":
                add(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "SUB":
                sub(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "MUL":
                mul(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "IDIV":
                idiv(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "LT" or inst.opcode == "GT" or inst.opcode == "EQ":
                relationComparison(inst.opcode, inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "AND":
                andInst(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "OR":
                orInst(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "NOT":
                notInst(inst.args[0], inst.args[1])
            elif inst.opcode == "INT2CHAR":
                int2char(inst.args[0], inst.args[1])
            elif inst.opcode == "STRI2INT":
                stri2int(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "READ":
                read(inst.args[0], inst.args[1])
            elif inst.opcode == "WRITE":
                write(inst.args[0])
            elif inst.opcode == "CONCAT":
                concat(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "STRLEN":
                strlen(inst.args[0], inst.args[1])
            elif inst.opcode == "GETCHAR":
                getchar(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "SETCHAR":
                setchar(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "TYPE":
                type(inst.args[0], inst.args[1])
            elif inst.opcode == "JUMP":
                jumpTo(inst.args[0])
            elif inst.opcode == "JUMPIFEQ":
                jumpIfEq(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "JUMPIFNEQ":
                jumpIfNotEq(inst.args[0], inst.args[1], inst.args[2])
            elif inst.opcode == "EXIT":
                exit(inst.args[0])
            elif inst.opcode == "DPRINT":
                dprint(inst.args[0])
            elif inst.opcode == "BREAK":
                Print.debug("Inst:" + str(inst.order) + "\n" + str(self.memoryModel))
            elif inst.opcode != "LABEL":
                Print.error("Internal: Unknown opcode")
                sys.exit(CONST.ERROR_XML_STRUCT)
            Statistics.incExecutedInstructions()
            Statistics.incMaxInitializedVars(self.memoryModel)
            self.instListIndex += 1

    def findLabels(self):
        for inst in self.instructions:
            if inst.opcode == "LABEL":
                if inst.args[0].value in self.labels:
                    Print.error("Duplicate label found: " + str(inst.args[0].value))
                    sys.exit(CONST.ERROR_SEMANTIC)
                self.labels[inst.args[0].value] = inst.order

    def parse3Operands(self, destination, leftOperand, rightOperand):
        # generalization of parsing 3 operands (destination, lo, ro)
        destinationVar = self.memoryModel.getVariable(*destination.splitVariable())
        if destinationVar is None:
            Print.error("Undefined variable")
            sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)

        if leftOperand.type == Argument.Type.VAR:
            leftOperand = self.memoryModel.getVariable(*leftOperand.splitVariable())
            self.isVariableReadable(leftOperand)

        if rightOperand.type == Argument.Type.VAR:
            rightOperand = self.memoryModel.getVariable(*rightOperand.splitVariable())
            self.isVariableReadable(rightOperand)

        leftOperand.type = self.mapArgTypeToVarType(leftOperand.type)
        rightOperand.type = self.mapArgTypeToVarType(rightOperand.type)
        return destinationVar, leftOperand, rightOperand

    def isVariableReadable(self, var, varName=""):
        if var is None:
            Print.error("Undefined variable to read " + str(varName))
            sys.exit(CONST.RUN_ERROR_NON_EX_VARIABLE)
        if var.value is None:
            Print.error("Uninitialized value " + str(varName))
            sys.exit(CONST.RUN_ERROR_NO_VALUE)

    def mapArgTypeToVarType(self, type):
        # mapping of variable type to argument type enums
        if type == Argument.Type.BOOL:
            return Variable.Type.BOOL
        elif type == Argument.Type.INT:
            return Variable.Type.INT
        elif type == Argument.Type.STRING:
            return Variable.Type.STRING
        elif type == Argument.Type.NIL:
            return Variable.Type.NIL
        elif type == Argument.Type.VAR:
            return Argument.Type.VAR
        else:
            if isinstance(type, Variable.Type):
                return type
            else:
                Print.error("[Internal] Cannot map type " + str(type))
                sys.exit(CONST.ERROR_INTERNAL)

    def checkTypeEquality(self, leftSymbol, rightSymbol, expectedType=None):
        # checks if types are equal (independantly of enum type)
        if expectedType is not None and isinstance(expectedType, Argument.Type):
            expectedType = self.mapArgTypeToVarType(expectedType)
        if isinstance(leftSymbol, Variable):
            if isinstance(rightSymbol, Variable):
                if expectedType is None:
                    return True if leftSymbol.type == rightSymbol.type else False
                else:
                    return True if leftSymbol.type == rightSymbol.type and leftSymbol.type == expectedType else False
            elif isinstance(rightSymbol, Argument):
                convertedRightType = self.mapArgTypeToVarType(rightSymbol.type)
                if expectedType is None:
                    return True if leftSymbol.type == convertedRightType else False
                else:
                    return True if leftSymbol.type == convertedRightType and leftSymbol.type == expectedType else False
        elif isinstance(leftSymbol, Argument):
            convertedLeftType = self.mapArgTypeToVarType(leftSymbol.type)
            if isinstance(rightSymbol, Variable):
                if expectedType is None:
                    return True if convertedLeftType == rightSymbol.type else False
                else:
                    return True if convertedLeftType == rightSymbol.type and rightSymbol.type == expectedType else False
            elif isinstance(rightSymbol, Argument):
                convertedRightType = self.mapArgTypeToVarType(rightSymbol.type)
                if expectedType is None:
                    return True if convertedLeftType == convertedRightType else False
                else:
                    return True if convertedLeftType == convertedRightType and convertedLeftType == expectedType else False


class InputStream:
    """
    Handles user input via method getNextLine, input from stdin or file is possible
    """
    def __init__(self, path):
        self.inputIndex = 0
        if path == sys.stdin:
            self.isStdin = True
        else:
            self.isStdin = False
            inputFile = open(path, "r")
            if inputFile is None:
                Print.error("Could not open file to read input from")
                sys.exit(CONST.ERROR_FOPENR)
            else:
                self.inputList = inputFile.readlines()

    def getNextLine(self):
        if self.isStdin:
            try:
                return input()
            except EOFError:
                return None
        else:
            self.inputIndex += 1
            if self.inputIndex - 1 < len(self.inputList):
                return self.inputList[self.inputIndex - 1].rstrip()
            else:
                return None


class MemoryModel:
    """
    Abstraction of memory model (frames, datastack) in ippcode19. that implements
    methods needed to work with memory model easily and correctly
    """
    class DataStack:
        def __init__(self):
            self._list = []

        def push(self, symb):
            self._list.append(symb)

        def pop(self):
            if len(self._list) > 0:
                return self._list.pop()
            else:
                Print.error("Data stack is empty")
                sys.exit(CONST.RUN_ERROR_NO_VALUE)

        def __repr__(self):
            return "DataStack: " + str([str(var) for var in self._list]) + "\n"

    class Frame:
        def __init__(self, scope):
            self.variables = []
            self.scope = scope

        def addVariable(self, var):
            if self.getVariable(var.name) is None:
                self.variables.append(var)
            else:
                Print.error("Redefinition of variable: " + str(var.name))
                sys.exit(CONST.RUN_ERROR_REDEFINITION)

        def getVariable(self, name):
            for var in self.variables:
                if var.name == name:
                    return var
            else:
                return None

        def __repr__(self):
            return "Scope: " + str(self.scope) + " Vars: " + str([str(var) for var in self.variables]) + "\n"

    class FrameType(Enum):
        GF = 0
        LF = 1
        TF = 2

    def __init__(self):
        self.stack = []
        self.tmpFrame = None
        self.globalFrame = self.Frame(self.FrameType.GF)
        self.dataStack = self.DataStack()

    def createFrame(self):
        self.tmpFrame = self.Frame(self.FrameType.TF)

    def pushFrame(self):
        if self.tmpFrame is None:
            Print.error("No frame available to push")
            sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
        self.tmpFrame.scope = self.FrameType.LF
        self.stack.append(self.tmpFrame)
        self.tmpFrame = None

    def popFrame(self):
        if len(self.stack) == 0:
            Print.error("No frame available to pop")
            sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
        else:
            self.tmpFrame = self.stack.pop()

    def addVariable(self, var, frameType):
        if frameType == self.FrameType.GF:
            self.globalFrame.addVariable(var)
        elif frameType == self.FrameType.TF:
            if self.tmpFrame is not None:
                self.tmpFrame.addVariable(var)
            else:
                Print.error("Adding variable to nonexisting TF")
                sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
        elif frameType == self.FrameType.LF:
            if len(self.stack) != 0:
                self.stack[-1].addVariable(var)
            else:
                Print.error("No local frame available")
                sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
        else:
            Print.error("[Internal] Unknown frametype")
            sys.exit(CONST.ERROR_INTERNAL)

    def getVariable(self, frameType, name):
        if not isinstance(frameType, self.FrameType):
            frameType = self.convertStringToFrameType(frameType)
        if frameType == self.FrameType.LF:
            if len(self.stack) == 0:
                Print.error("Local frame does not exist")
                sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
            else:
                return self.stack[-1].getVariable(name)
        elif frameType == self.FrameType.TF:
            if self.tmpFrame is None:
                Print.error("Temporary frame does not exist")
                sys.exit(CONST.RUN_ERROR_NON_EX_FRAME)
            else:
                return self.tmpFrame.getVariable(name)
        elif frameType == self.FrameType.GF:
            return self.globalFrame.getVariable(name)
        else:
            Print.error("[Internal] Unknown frametype")
            sys.exit(CONST.ERROR_INTERNAL)

    def convertStringToFrameType(self, string):
        if string == "LF":
            return self.FrameType.LF
        elif string == "TF":
            return self.FrameType.TF
        elif string == "GF":
            return self.FrameType.GF
        else:
            Print.error("[Internal] Unknown string in frametype")
            sys.exit(CONST.ERROR_INTERNAL)

    def __repr__(self):
        return "MemoryModel:\n" + str(self.dataStack) + "GF: " + str(self.globalFrame) + "TF: " + str(self.tmpFrame) + "\nLF: " + str([str(frame) for frame in self.stack])


class Variable:
    """
    Representation of variable used to be stored in memory model, variable knows
    its name value and type and implements basic methods to work with variable
    """
    class Type(Enum):
        NONETYPE = 0
        NIL = 1
        INT = 2
        STRING = 3
        BOOL = 4

    def __init__(self, name):
        self.name = name
        self.type = Variable.Type.NONETYPE
        self.value = None

    def insert(self, type, value):
        self.type = type
        if type == Variable.Type.BOOL:
            self.value = bool(value)
        elif type == Variable.Type.INT:
            self.value = int(value)
        elif type == Variable.Type.STRING:
            self.value = str(value)
        elif type == Variable.Type.NIL:
            self.value = ""
        else:
            Print.error("[Internal] insert: unsupported type")
            sys.exit(CONST.ERROR_INTERNAL)

    def copy(self, var):
        self.type = var.type
        self.value = var.value

    def __repr__(self):
        return "VAR: Name: " + self.name + ";type: " + str(self.type) + ";pytype:" + str(type(self.value)) + ";val: " + str(self.value) + "|"


class InstructionFactory:
    """
    Factory that returns instruction object, if opcode is okay
    """
    @staticmethod
    def create(opcode, order):
        if opcode not in CONST.INSTRUCTION_SET:
            Print.error("Unsuported opcode: " + opcode)
            sys.exit(CONST.ERROR_XML_STRUCT)
        else:
            return Instruction(opcode, order)


class Instruction:
    """
    Representation of instruction loaded from xml file
    """
    def __init__(self, opcode, order):
        self.opcode = opcode
        self.order = order
        self.args = []

    def addArgument(self, argument):
        self.args.append(argument)

    def __repr__(self):
        return self.order + ": " + self.opcode + str([str(arg) for arg in self.args])


class ArgumentFactory:
    """
    Factory that returns Argument object, if all checkcs of its lexical and syntactic
    correctness vere okay
    """
    @staticmethod
    def create(opcode, type, position, xmlText):
        if position > CONST.MAX_ARGUMENTS:
            Print.error("Too many arguments in instruction: " + opcode)
            sys.exit(CONST.ERROR_XML_STRUCT)

        if position > len(CONST.INSTRUCTION_SET[opcode]):
            Print.error("Too many arguments in instruction: " + opcode)
            sys.exit(CONST.ERROR_XML_STRUCT)

        if xmlText is None:
            xmlText = ""

        expectedType = CONST.INSTRUCTION_SET[opcode][position - 1]
        if ArgumentFactory.isTypeCorrect(expectedType, type):
            ArgumentFactory.checkTypeFormat(opcode, type, xmlText)
            return Argument(opcode, ArgumentFactory.convertStringToType(type), position, xmlText)
        else:
            Print.error("Error: Expected type: " + expectedType + " instead of " + type)
            sys.exit(CONST.ERROR_XML_STRUCT)
    
    @staticmethod
    def isTypeCorrect(expectedType, type):
        if expectedType == "var" and type == "var":
            return True
        elif expectedType == "symb":
            if type == "var" or type == "int" or type == "bool" or type == "string" or type == "nil":
                return True
            else:
                return False
        elif expectedType == "type" and type == "type":
            return True
        elif expectedType == "label" and type == "label":
            return True
        else:
            return False

    @staticmethod
    def checkTypeFormat(opcode, type, xmlText):
        if type == "var":
            if not re.match(r"^(LF|TF|GF)@([^0-9]|[\-\_\$\&\%\*\!\?])[\w\-\$\&\%\*\!\?]*$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "type":
            if not re.match(r"^(bool|string|int)$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "label":
            if not re.match(r"^([^0-9]|[\-\_\$\&\%\*\!\?])[\w\-\$\&\%\*\!\?]*$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "int":
            if not re.match(r"^(\+|-)?[0-9]+$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "bool":
            if not re.match(r"^(true|false)$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "string":
            if not re.match(r"^([^#\\\s]|(\\[0-9]{3}))*?$", xmlText):
                ArgumentFactory.typeError(type, opcode)
        elif type == "nil":
            if not re.match(r"^nil$", xmlText):
                ArgumentFactory.typeError(type, opcode)

    @staticmethod
    def typeError(type, opcode):
        Print.error("Invalid format of argument of type " + type + " in instruction: " + opcode)
        sys.exit(CONST.ERROR_XML_STRUCT)

    @staticmethod
    def convertStringToType(string):
        if string == "var":
            return Argument.Type.VAR
        elif string == "type":
            return Argument.Type.TYPE
        elif string == "label":
            return Argument.Type.LABEL
        elif string == "int":
            return Argument.Type.INT
        elif string == "bool":
            return Argument.Type.BOOL
        elif string == "string":
            return Argument.Type.STRING
        elif string == "nil":
            return Argument.Type.NIL
        else:
            Print.error("[Internal] Unsuported type")
            sys.exit(CONST.ERROR_INTERNAL)


class Argument:
    """
    Internal description of ippcode19 instruction argument
    """
    class Type(Enum):
        VAR = 0
        LABEL = 1
        TYPE = 2
        INT = 3
        BOOL = 4
        STRING = 5
        NIL = 6

    def __init__(self, opcode, type, position, value):
        self.opcode = opcode
        self.type = type
        self.position = position
        if type == Argument.Type.INT:
            self.value = int(value)
        elif type == Argument.Type.STRING:
            self.value = Argument.convertEscapeSequences(str(value))
        elif type == Argument.Type.BOOL:
            if value == "true":
                self.value = True
            else:
                self.value = False
        elif type == Argument.Type.NIL:
            self.value = ""
        else:
            self.value = value
    
    @staticmethod
    def convertEscapeSequences(string):
        def replaceCallback(match):
            return chr(int(match.group(1)))

        escSeqRegex = re.compile(r'\\([0-9]{3})')
        return str(escSeqRegex.sub(replaceCallback, string))

    def __repr__(self):
        return str(self.position) + ":(" + str(self.type) + ")" + str(self.value) + ";"

    def splitVariable(self):
        return self.value.split("@")

    def getVariableFrame(self):
        return self.value[0:2]

    def getVariableName(self):
        return self.value[3:]

    def isConst(self):
        return (self.type == Argument.Type.INT or self.type == Argument.Type.BOOL or
                self.type == Argument.Type.STRING or self.type == Argument.Type.NIL)


class XMLFileParser:
    """
    Parser for input xml file with instructions, handles lexical and syntactic analysis
    in cooperation with Instruction and Argument Factories
    """
    def __init__(self, file):
        self.file = file

    def parseFile(self):
        if (self.file == sys.stdin):
            try:
                self.tree = ET.parse(sys.stdin)
            except Exception as e:
                Print.error("Couldn't parse XML file. Ended with exception: \n" + str(e))
                sys.exit(CONST.ERROR_XML_FORMAT)
        else:
            xmlFile = None
            try:
                xmlFile = open(self.file, "r")
                try:
                    self.tree = ET.parse(xmlFile)
                except Exception as e:
                    Print.error("Couldn't parse XML file. Ended with exception: \n" + str(e))
                    sys.exit(CONST.ERROR_XML_FORMAT)

            except IOError:
                Print.error("Couldn't open specified source file")
                sys.exit(CONST.ERROR_FOPENR)

            finally:
                if xmlFile is not None:
                    xmlFile.close()

    def checkRootHeader(self):
        try:
            self.root = self.tree.getroot()
        except Exception as e:
            Print.error("Couldn't get XML file root. Ended with exception: \n" + str(e))
            sys.exit(CONST.ERROR_XML_FORMAT)

        if (self.root.tag == "program"):
            if("language" in self.root.attrib and self.root.attrib["language"] == "IPPcode19"):
                return
            else:
                Print.error("Attribute language expected on first line with IPPCode19 value")
                sys.exit(CONST.ERROR_XML_STRUCT)
        else:
            Print.error("Tag program expected on first line of XML")
            sys.exit(CONST.ERROR_XML_STRUCT)

    def saveInstructions(self, program):
        instructionList = []
        for instElem in self.root:
            if instElem.tag != "instruction":
                Print.error("Invalid tag in XML file")
                sys.exit(CONST.ERROR_XML_STRUCT)
            else:
                if "opcode" in instElem.attrib and "order" in instElem.attrib:
                    opcode = instElem.attrib["opcode"]
                    order = instElem.attrib["order"]
                    Print.setInfo(order, opcode)
                    instruction = InstructionFactory.create(opcode, order)
                    argCounter = 1
                    instElem = sorted(instElem, key=lambda argElem: argElem.tag)
                    for argElem in instElem:
                        if argElem.tag != "arg" + str(argCounter):
                            Print.error("Expected argument[1-3] tag")
                            sys.exit(CONST.ERROR_XML_STRUCT)
                        if "type" not in argElem.attrib:
                            Print.error("Argument has to have attribute type")
                            sys.exit(CONST.ERROR_XML_STRUCT)
                        else:
                            instruction.addArgument(ArgumentFactory.create(opcode, argElem.attrib["type"], argCounter, argElem.text))
                            argCounter += 1
                    if opcode not in CONST.INSTRUCTION_SET:
                        Print.error("Unknown opcode")
                        sys.exit(CONST.ERROR_XML_STRUCT)

                    if argCounter != len(CONST.INSTRUCTION_SET[opcode]) + 1:
                        Print.error("Not enough operands")
                        sys.exit(CONST.ERROR_XML_STRUCT)
                    instructionList.append(instruction)

        instructionList.sort(key=lambda i: int(i.order))
        self.checkInstructionsOrder(instructionList)
        program.setInstructions(instructionList)


    def checkInstructionsOrder(self, sortedInstructionList):
        counter = 1
        for inst in sortedInstructionList:
            if int(inst.order) == counter:
                counter += 1
            else:
                Print.error("Instruction order is not a sequence...")
                sys.exit(CONST.ERROR_XML_STRUCT)


class Print:
    """
    Used for specifying more user friendly output of interpretation result
    """
    order = None
    opcode = None

    @staticmethod
    def setInfo(order, opcode):
        Print.order = order
        Print.opcode = opcode
    
    @staticmethod
    def help():
        print("HELP for script interpret.py")
        print("-------------------------------------------------------------")
        print("Loads ippCode19 in XML format frin given source and")
        print("interprets it to stdout. Returns non zero value if interpretation")
        print("was unsucessful")
        print()
        print("Script parameters:")
        print(" --help              displays help (this)")
        print(" --source=file       uses file as source of ippCode19 in XML")
        print(" --input=file        uses file as input for interpret")
        print("At least one of --source or --input parameter has to be given")
        print("if one of them is missing, its corresponding data are loaded")
        print("from stdin")
        print("-------------------------------------------------------------")

    @staticmethod
    def error(string):
        print("[Error][" + str(Print.order) + "][" + str(Print.opcode) + "] " + string, file=sys.stderr)


    @staticmethod
    def debug(string):
        print("[Debug] " + string, file=sys.stderr)


def parseArguments():
        if "--help" in sys.argv:
            Print.help()
            sys.exit(CONST.OK)
        try:
            opts, args = getopt(sys.argv[1:], "", ["insts", "vars", "help", "source=", "input=", "stats="])
        except GetoptError as err:
            Print.error("Invalid arguments:" + str(err))
            sys.exit(CONST.ERROR_PARAM)

        sourceStream = sys.stdin
        inputStream = sys.stdin
        statsFound = False
        statOpts = []
        for opt, arg in opts:
            if "--source" == opt:
                sourceStream = arg
            if "--input" == opt:
                inputStream = arg
            if "--stats" == opt:
                statsFound = True
                Statistics.filePath = arg
            if "--insts" == opt:
                statOpts.append("insts")
            if "--vars" == opt:
                statOpts.append("vars")

        if statsFound is False and len(statOpts) > 0:
            Print.error("Parameters insts or vars are not usable without stats")
            sys.exit(CONST.ERROR_PARAM)
        else:
            Statistics.options = statOpts
        if inputStream == sys.stdin and sourceStream == sys.stdin:
            Print.error("One of parameters --input or --source has to be used")
            sys.exit(CONST.ERROR_PARAM)
        else:
            return inputStream, sourceStream

if __name__ == '__main__':
    inputStream, source = parseArguments()
    xmlParser = XMLFileParser(source)
    xmlParser.parseFile()
    xmlParser.checkRootHeader()
    source = Program(inputStream)
    xmlParser.saveInstructions(source)
    source.interpret()
    if (Statistics.filePath is not None):
        Statistics.writeToFile()
