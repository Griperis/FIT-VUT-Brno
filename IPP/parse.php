<?php
/**
 *  IPP 1st part of project - file parse.php
 *
 *  This program loads IPPcode19 source code and checks its
 *  lexical and syntactic correctness and writes xml representation
 *  of this code to output
 *
 *  Author: Zdenek Dolezal
 *  Login: xdolez82
 *  Faculty of information technology BUT
*/

/* Constants */
const PARAM_ERROR = 10;
const FOPENR_ERROR = 11;
const FOPENW_ERROR = 12;
const HEADER_ERROR = 21;
const INVALID_OPCODE_ERROR = 22;
const SYNLEX_ERROR = 23;
const EXIT_OK = 0;

// Posible instructions in IPPcode19 with their allowed arguments
$instructionSet = array(
    "MOVE"          => array("var", "symb", ),
    "CREATEFRAME"   => array(),
    "PUSHFRAME"     => array(),
    "POPFRAME"      => array(),
    "DEFVAR"        => array("var"),
    "CALL"          => array("label"),
    "RETURN"        => array(),
    "PUSHS"         => array("symb"),
    "POPS"          => array("var"),
    "ADD"           => array("var", "symb", "symb"),
    "SUB"           => array("var", "symb", "symb"),
    "MUL"           => array("var", "symb", "symb"),
    "IDIV"          => array("var", "symb", "symb"),
    "LT"            => array("var", "symb", "symb"),
    "GT"            => array("var", "symb", "symb"),
    "EQ"            => array("var", "symb", "symb"),
    "AND"           => array("var", "symb", "symb"),
    "OR"            => array("var", "symb", "symb"),
    "NOT"           => array("var", "symb"),
    "INT2CHAR"      => array("var", "symb"),
    "STRI2INT"       => array("var", "symb", "symb"),
    "READ"          => array("var", "type"),
    "WRITE"         => array("symb"),
    "CONCAT"        => array("var", "symb", "symb"),
    "STRLEN"        => array("var", "symb"),
    "GETCHAR"       => array("var", "symb", "symb"),
    "SETCHAR"       => array("var", "symb", "symb"),
    "TYPE"          => array("var", "symb"),
    "LABEL"         => array("label"),
    "JUMP"          => array("label"),
    "JUMPIFEQ"      => array("label", "symb", "symb"),
    "JUMPIFNEQ"     => array("label", "symb", "symb"),
    "EXIT"          => array("symb"),
    "DPRINT"        => array("symb"),
    "BREAK"         => array()
);

// Internal representation of XML file for storing instructions
class XMLInstructionFile{
    static private $instructionArray = array();  // array of created Instructions

    public static function addInstruction(Instruction $instruction){
        array_push(self::$instructionArray, $instruction);
    }
    // Prints internal interpretation of Instructions and Arguments to standart output
    public static function print(){
        echo "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
        echo "<program language=\"IPPcode19\">\n";

        foreach(self::$instructionArray as $instruction){
            echo $instruction->toXML();
        }

        echo "</program>\n";
    }
}

/*
Instruction Factory
-   makes more abstraction on creating instructions and to check
    instruction validness before creating actual object of it
*/
class InstructionFactory{
    /* */
    public static function create($line){
        global $lineNumber;
        global $instructionSet;
        // Partition of instruction to opcode and arguments
        if (!preg_match("/^[\t ]*(?'OPCODE'\w+?)([\t ]+(?'ARG1'\S+))?([\t ]+(?'ARG2'\S+))?([\t ]+(?'ARG3'\S+))?[\t ]*$/u", $line, $matches)){
            fwrite(STDERR, "Error: Wrong format of instruction on line $lineNumber\n");
            exit(SYNLEX_ERROR);
        }
        $opcode = strtoupper($matches['OPCODE']);
        if (array_key_exists($opcode, $instructionSet)){
            if (self::isJumpOpcode($opcode)) Statistics::incJumps();
            $argumentCount = sizeof($instructionSet[$opcode]);
            if (self::getNumberOfMatchedArgs($matches) !== $argumentCount){
                fwrite(STDERR, "Error: $opcode Invalid number of arguments. $argumentCount expected on line: $lineNumber\n");
                exit(SYNLEX_ERROR);
            }
            switch($argumentCount){
                case 0:
                    return new Instruction($opcode);
                case 1:
                    return new Instruction($opcode,
                        Argument::create($opcode, $matches['ARG1'], 1));
                case 2:
                    return new Instruction($opcode,
                        Argument::create($opcode, $matches['ARG1'], 1),
                        Argument::create($opcode, $matches['ARG2'], 2));
                case 3:
                    return new Instruction($opcode,
                        Argument::create($opcode, $matches['ARG1'], 1),
                        Argument::create($opcode, $matches['ARG2'], 2),
                        Argument::create($opcode, $matches['ARG3'], 3));
                default:
                    fwrite(STDERR, "Error: Unsupported number of instruction arguments\n");
                    exit(SYNLEX_ERROR);
            }
        }else{
            fwrite(STDERR, "Error: Unknown instruction $opcode on line: $lineNumber\n");
            exit(INVALID_OPCODE_ERROR);
        }
    }
    private static function getNumberOfMatchedArgs($matches){
        $args = 0;
        for ($i = 1; $i <= 3; $i++){
            if (array_key_exists("ARG$i", $matches))
                $args++;
        }
        return $args;

    }
    private static function isJumpOpcode($opcode){
        return $opcode === "JUMPIFEQ" || $opcode === "JUMPIFNEQ" || $opcode === "JUMP";
    }

}

/*
Instruction
-   always created by instruction factory, can have 0 to 3 arguments of type argument
*/
class Instruction{
    private static $total = 1;  // total count of instructions
    private $order;     // order of instruction in code
    private $opcode;    // operational code
    private $arg1;      // arguments
    private $arg2;
    private $arg3;

    function __construct($opcode, Argument $arg1 = null, Argument $arg2 = null, Argument $arg3 = null){
        $this->order = self::$total++;
        $this->opcode = $opcode;
        $this->arg1 = $arg1;
        $this->arg2 = $arg2;
        $this->arg3 = $arg3;
    }
    // Conversion to XML format of instruction
    public function toXML(){
        $returnString = "\t<instruction order=\"$this->order\" opcode=\"".strtoupper($this->opcode)."\">\n";
        if ($this->arg1 !== null){
            $returnString .= $this->arg1->toXML();
        }
        if ($this->arg2 !== null){
            $returnString .= $this->arg2->toXML();
        }
        if ($this->arg3 !== null){
            $returnString .= $this->arg3->toXML();
        }
        return $returnString."\t</instruction>\n";
    }
    public static function getInstructionCount(){
        return self::$total - 1;
    }
}

/*
Argument class
- describes one argument of instruction
- used in creation of Instruction
*/
class Argument{
    private $type;  //type of argument to be displayed in type="" tag
    private $text;  //inner text value of the <arg> tag
    private $order; //order of the argument in instruction
    /*
    Factory method for creating arguments and checking their correctness
    -   checks whether opcode is valid with certain argument on certain position
        and also checks its lexical and syntactic validness with set of regular
        expressions
    */
    public static function create($opcode, $text, $order){
        global $instructionSet;
        global $lineNumber;
        $internalType = $instructionSet[$opcode][$order-1];
        switch($internalType){
            case "var":
                //matches LF, TF, GF followed by @ and letter with special symbols followed by any unicode character or special symbol
                if (preg_match("/^(LF|TF|GF)@[\p{L}\-\_\$\&\%\*\!\?][\w\-\$\&\%\*\!\?]*$/u", $text, $matches)){
                    $fixedMatch = htmlspecialchars($matches[0], ENT_XML1 | ENT_COMPAT, 'UTF-8');
                    return new Argument("var", $fixedMatch, $order);
                }else{
                    fwrite(STDERR, "Error: $opcode [$order] Expected argument of type 'var' instead of \"$text\" on line $lineNumber\n");
                    exit(SYNLEX_ERROR);
                }
                break;

            case "symb":
                    // matches ippcode specified constants with their format or variable
                    if (preg_match("/^(bool@(true|false)|string@(.*)|int@(.*)|nil@nil|(LF|TF|GF)@[\p{L}\_\-\$\&\%\*\!\?][\w\-\$\&\%\*\!\?]*)$/u", $text, $matches)){
                        preg_match("/^.*?(?=@)/u", $matches[0], $symbType);
                        $fixedMatch = htmlspecialchars($matches[0], ENT_XML1 | ENT_COMPAT, 'UTF-8');
                        // Matching what follows after @
                        preg_match("/@(?'TEXT'.*$)/u", $fixedMatch, $innerText);

                        if ($symbType[0] === "LF" || $symbType[0] === "TF" || $symbType[0] == "GF"){
                            return new Argument("var", $fixedMatch, $order);
                        }else if ($symbType[0] === "string"){
                            // String validness
                            if (preg_match("/^([^#\\\\\s]|(\\\\\p{N}{3}))*?$/u", $innerText['TEXT'])){
                                return new Argument($symbType[0], $innerText['TEXT'], $order);
                            }else{
                                fwrite(STDERR, "Error: $opcode [$order] Wrong format of string \"$text\" on line $lineNumber\n");
                                exit(SYNLEX_ERROR);
                            }
                        }else if ($symbType[0] === "int"){
                            // Integer validness regex
                            if (preg_match("/^[+-]?[0-9]+$/u", $innerText['TEXT'])){
                                return new Argument($symbType[0], $innerText['TEXT'], $order);
                            }else{
                                fwrite(STDERR, "Error: $opcode [$order] Wrong format of int \"$text\" on line $lineNumber\n");
                                exit(SYNLEX_ERROR);
                            }
                        }else{
                            return new Argument($symbType[0], $innerText['TEXT'], $order);
                        }
                    }else{
                        fwrite(STDERR, "Error: $opcode [$order] Expected argument of type 'symb' instead of \"$text\" on line $lineNumber\n");
                        exit(SYNLEX_ERROR);
                    }
                break;

            case "label":
                // matches the same string as variable
                if (preg_match("/^[\p{L}\_\-\$\&\%\*\!\?][\w\-\$\&\%\*\!\?]*$/u", $text, $matches)){
                    if ($opcode === "LABEL"){
                        Statistics::addLabel($matches[0]);
                    }
                    return new Argument("label", $matches[0], $order);
                }else{
                    fwrite(STDERR, "Error: $opcode [$order] Expected argument of type 'label' instead of \"$text\" on line $lineNumber\n");
                    exit(SYNLEX_ERROR);
                }
                break;

            case "type":
                if (preg_match("/^int$|^bool$|^string$/u", $text, $matches)){
                    return new Argument("type", $matches[0], $order);
                }else{
                    fwrite(STDERR, "Error: $opcode [$order] Expected argument of type 'type' instead of \"$text\" on line $lineNumber\n");
                    exit(SYNLEX_ERROR);
                }
                break;

            default:
                fwrite(STDERR, "Error: Unsupported argument: [$order] $argument $text on line $lineNumber\n");
                exit(SYNLEX_ERROR);
        }
    }
    function __construct($type, $text, $order){
        $this->type = $type;
        $this->text = $text;
        $this->order = $order;
    }
    // Argument conversion to its format in xml
    public function toXML(){
        return "\t\t<arg$this->order type=\"$this->type\">$this->text</arg$this->order>\n";
    }

}

/*
Statistics class (expansion)
- stores different metricies about converted code
*/
class Statistics{
    private static $comments = 0;
    private static $labels = array();
    private static $jumps = 0;
    private static $path = null;
    private static $options;

    /*
    Writing collected metrices to file. metrices are chosen by $options, which is
    initialized when parameters are being parsed. File location is described by $path (if
    stats parameter is not used it stays null, otherwise its set to value of stats
    parameter)
    */
    public static function fileWrite(){
        if (self::$path !== null){
            $file = @fopen(self::$path, "w");
            if ($file === false){
                fwrite(STDERR, "Error: Cannot open output file for statistics\n");
                exit(FOPENW_ERROR);
            }else{
                foreach (self::$options as $opt => $val){
                    switch($opt){
                        case "loc":
                            fwrite($file, self::getLoc()."\n");
                            break;
                        case "comments":
                            fwrite($file, self::getComments()."\n");
                            break;
                        case "labels":
                            fwrite($file, self::getLabels()."\n");
                            break;
                        case "jumps":
                            fwrite($file, self::getJumps()."\n");
                            break;
                    }
                }
            }
            fclose($file);
        }
    }
    public static function setPath($path){
        self::$path = $path;
    }
    public static function setOptions(Array $options){
        self::$options = $options;
    }
    public static function getLoc(){
        return Instruction::getInstructionCount();
    }
    public static function getComments(){
        return self::$comments;
    }
    public static function getLabels(){
        return sizeof(array_unique(self::$labels));
    }
    public static function getJumps(){
        return self::$jumps;
    }
    public static function incComments(){
        self::$comments++;
    }
    public static function addLabel($label){
        array_push(self::$labels, $label);
    }
    public static function incJumps(){
        self::$jumps++;
    }
}

function printHelp(){
    echo "HELP for script parse.php\n";
    echo "---------------------------------------------------------\n";
    echo "Script loads source code from standard input in IPPcode19\n";
    echo "format, checks its lexical and syntactic correctness and\n";
    echo "writes its XML representation to standard output.\n";
    echo "Script also can track statistics enablable with --stats='f'\n";
    echo "parameter. Statistics are printed to file 'f' in order given\n";
    echo "by order of stats related parameters passed to program\n";
    echo "\n";
    echo "Script parameters:\n";
    echo "  --help          displays help (this)\n";
    echo "  --stats='file'  enables logging of statistics\n";
    echo "  Stats related parameters (not usable solely)\n";
    echo "      --loc       counts every line with instruction\n";
    echo "      --jumps     count jump instructions\n";
    echo "      --comments  counts lines with comments\n";
    echo "      --labels    counts unique labels\n";
    echo "\n";
    echo "Specific return values:\n";
    echo "  21  invalid or missing header in source code\n";
    echo "  22  unknown or invalid operational code in source code\n";
    echo "  23  other lexical or syntactic error in source code\n";
    echo "---------------------------------------------------------\n";
}

/*  Parsing parameters, when using stats parameter, path and options are set
   for Statistics class
*/
function handleParams(){
    global $argc;
    $params = getopt("", ["help", "stats:", "loc", "comments", "labels", "jumps"]);
    if ($params === false || (sizeof($params) === 0 && $argc > 1)){
        fwrite(STDERR, "Error: invalid parameters use --help\n");
        exit(PARAM_ERROR);
    }
    if (array_key_exists("help", $params)){
        if ($argc === 2){
            printHelp();
            exit(EXIT_OK);
        }else{
            fwrite(STDERR, "Error: Help is not usable with other parameters\n");
            exit(PARAM_ERROR);
        }
    }
    if (array_key_exists("stats", $params)){
        Statistics::setPath($params["stats"]);
        unset($params["stats"]);
        Statistics::setOptions($params);
    }else{
        if ($argc > 1){
            fwrite(STDERR, "Error: Can't use statistics related paramteres without --stats\n");
            exit(PARAM_ERROR);
        }
    }
}

/* Main */
handleParams();

$firstLine = fgets(STDIN);
if (preg_match("/#/u", $firstLine)) Statistics::incComments();

// Regex for matching header (.ippcode19 followed by possible comment)
if (!preg_match("/^[ \t]*.IPPCode19(\s*(\#.*?)?)$/ui", $firstLine)){
    fwrite(STDERR, "Error: Invalid first line of input, .IPPcode19 expected\n");
    exit(HEADER_ERROR);
}


$lineNumber = 1;

while($line = fgets(STDIN)){
    $lineNumber++;
    // checking for commentary (if there is #) for stats
    if (preg_match("/#/u", $line)) Statistics::incComments();
    // if there is a blank line or whitespaces followed by comment, skips the line
    if (preg_match("/(^[ \t]*?#.*$)|(^\s*$)/u", $line)){
        continue;
    }else{
        // match part of line without commentary
        if (preg_match("/(?'WC'.*?)#.*$/u", $line, $matches)){
            $line = $matches['WC'];
        }
        XMLInstructionFile::addInstruction(InstructionFactory::create($line));
    }
}
XMLInstructionFile::print();
Statistics::fileWrite();
?>
