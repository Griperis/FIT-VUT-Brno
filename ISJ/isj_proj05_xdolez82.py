#!/usr/bin/env python3
class Polynomial:
    """Polynomial class methods and definitions"""
    def __init__(self, *args, **kwargs):
        """Initialize method (on creation)"""
        #private polynom coefficients list
        self.__coeffs=[]
        #checking the arguments and converting them to list in correct form
        #whether is list and args is used(checking by len)
        if len(args) != 0 and isinstance(args[0],list):
            self.__coeffs.extend(*args)
        #if kwards are not used and its not a list, then normal args are used
        elif not kwargs:
            #append all arguments
            for i in args:
                self.__coeffs.append(i)
        #keywordargs
        else:
            #replacing x in keys and converting them to integers
            aux_d = {int(k):v for k, v in {i.replace('x',''): kwargs[i] for i in kwargs.keys()}.items()}
            #adding 0s to unfilled spots
            for i in range(max(aux_d.keys())):
                if not i in aux_d.keys():
                    aux_d[i] = 0
            #sorting the dictionary, so it keys are by order
            #converting to list
            self.__coeffs=list(aux_d[k] for k in sorted(aux_d))

    def __str__(self):
        """Printing method"""
        #returns polynom converted to string using private function __toString
        return self.__toString(self.__coeffs)

    def __repr__(self):
        """Define how the class represents itself"""
        return self.__toString(self.__coeffs)

    def __toString(self,pol):
        """Private method to convert polynomial to correct output string"""
        ret_string=""
        fnz_index_found=False
        #if is only absolute part of polynom then immeadetly return value
        if len(pol) == 1 or all(v == 0 for v in pol[1:]):
            if pol[0] > 0:
                return str(pol[0])
            elif pol[0] < 0:
                return "- " + str(abs(pol[0]))
        #when pol = 0
        if all(v == 0 for v in pol):
            return "0"
        #looping reversed through index and value in list
        for i,c in reversed(list(enumerate(pol))):
            if c != 0:
                #bypass 0 values from start (first non ze)
                if not fnz_index_found:
                    fnz_index=i
                    fnz_index_found=True
                if i != 0:
                    #index string (if one then nothing)
                    if i == 1:
                        index=''
                    else:
                        index='^' + str(i)
                    #whether its the first value of the polynom
                    if i == fnz_index:
                        if c == 1:
                            ret_string = ret_string + 'x' + index
                        else:
                            ret_string = ret_string + str(c) + 'x' + index
                    #whether the other values are positive or negative
                    elif c > 0:
                        #if coeff is 1 its not needed to print it
                        if c == 1:
                            ret_string = ret_string + ' + x' + index
                        else:
                            ret_string = ret_string + ' + ' + str(c) + 'x' + index
                    elif c < 0:
                        #if coeff is -1 its not needed to print it
                        if c == -1:
                            ret_string = ret_string + ' - x' + index
                        else:
                            ret_string = ret_string + ' - ' + str(abs(c)) + 'x' + index
                #the last coeff
                else:
                    if c > 0:
                        ret_string = ret_string + ' + ' + str(c)
                    if c < 0:
                        ret_string = ret_string + ' - ' + str(abs(c))
        return ret_string

    def __eq__(self, other):
        """String comparison between polynomials"""
        return self.__toString(self.__coeffs) == str(other)

    def __add__(self, other):
        """Adding polynomials"""
        #test to ensure that always we add to the bigger list
        if len(self.__coeffs) >= len(other.__coeffs):
            res_coeffs=self.__coeffs[:]
            #iterating over all values
            for i in range(len(self.__coeffs)):
                #check if we are not trying to add value that is not in the list
                if i < len(other.__coeffs):
                    #adding values at indexes together
                    res_coeffs[i] += other.__coeffs[i]
            return Polynomial(*res_coeffs)
        else:
            res_coeffs=other.__coeffs[:]
            #same as before, only saving results into the other polynom
            for i in range(len(other.__coeffs)):
                if i < len(self.__coeffs):
                    res_coeffs[i] += self.__coeffs[i]
            return Polynomial(*res_coeffs)


    def __pow__(self,n):
        """Exponentiating polynomials"""
        res_poly=Polynomial(1)
        #n times multiply self by self (initialized to one, because **1  needs to return itself)
        for i in range(n):
            res_poly *= self
        return res_poly

    def __mul__(self,other):
        """Multiplication of polynoms"""
        #creating a result list with size of polynoms max indexes
        #because for example 4 degree * 4 degree must give 8 degree polynom
        res_coeffs=[0]*(len(self.__coeffs)+len(other.__coeffs)-1)
        #multiplying every value by every other value
        for i in range(len(self.__coeffs)):
            for j in range(len(other.__coeffs)):
                res_coeffs[i+j] += self.__coeffs[i] * other.__coeffs[j]
        return Polynomial(*res_coeffs)

    def derivative(self):
        """Derivative of polynomial"""
        res_coeffs=[]
        #append to a list derivatives of members of polynomial
        for i,c in enumerate(self.__coeffs):
            #exponent -1 and coeff=exponent*prev_coeff
            res_coeffs.insert(i-1,c*i)
        return Polynomial(*res_coeffs)

    def at_value(self,x1,x2=0):
        """Value of polynom for x, when 2 parameters are given, function returns differnece between them"""
        result=0
        result2=0
        #calculates the value of polynomial, when two arguments are given, calculates difference between them
        for i,c in enumerate(self.__coeffs):
            result+=c*x1**i
            if x2 != 0:
                result2+=c*x2**i
        if x2 == 0:
            return result
        else:
            return result2-result

def test():
    assert str(Polynomial(0,1,0,-1,4,-2,0,1,3,0)) == "3x^8 + x^7 - 2x^5 + 4x^4 - x^3 + x"
    assert str(Polynomial([-5,1,0,-1,4,-2,0,1,3,0])) == "3x^8 + x^7 - 2x^5 + 4x^4 - x^3 + x - 5"
    assert str(Polynomial(x7=1, x4=4, x8=3, x9=0, x0=0, x5=-2, x3= -1, x1=1)) == "3x^8 + x^7 - 2x^5 + 4x^4 - x^3 + x"
    assert str(Polynomial(x2=0)) == "0"
    assert str(Polynomial(x0=0)) == "0"
    assert Polynomial(x0=2, x1=0, x3=0, x2=3) == Polynomial(2,0,3)
    assert Polynomial(x2=0) == Polynomial(x0=0)
    assert str(Polynomial(x0=1)+Polynomial(x1=1)) == "x + 1"
    assert str(Polynomial([-1,1,1,0])+Polynomial(1,-1,1)) == "2x^2"
    pol1 = Polynomial(x2=3, x0=1)
    pol2 = Polynomial(x1=1, x3=0)
    assert str(pol1+pol2) == "3x^2 + x + 1"
    assert str(pol1+pol2) == "3x^2 + x + 1"
    assert str(Polynomial(x0=-1,x1=1)**1) == "x - 1"
    assert str(Polynomial(x0=-1,x1=1)**2) == "x^2 - 2x + 1"
    pol3 = Polynomial(x0=-1,x1=1)
    assert str(pol3**4) == "x^4 - 4x^3 + 6x^2 - 4x + 1"
    assert str(pol3**4) == "x^4 - 4x^3 + 6x^2 - 4x + 1"
    assert str(Polynomial(x0=2).derivative()) == "0"
    assert str(Polynomial(x3=2,x1=3,x0=2).derivative()) == "6x^2 + 3"
    assert str(Polynomial(x3=2,x1=3,x0=2).derivative().derivative()) == "12x"
    pol4 = Polynomial(x3=2,x1=3,x0=2)
    assert str(pol4.derivative()) == "6x^2 + 3"
    assert str(pol4.derivative()) == "6x^2 + 3"
    assert Polynomial(-2,3,4,-5).at_value(0) == -2
    assert Polynomial(x2=3, x0=-1, x1=-2).at_value(3) == 20
    assert Polynomial(x2=3, x0=-1, x1=-2).at_value(3,5) == 44
    pol5 = Polynomial([1,0,-2])
    assert pol5.at_value(-2.4) == -10.52
    assert pol5.at_value(-2.4) == -10.52
    assert pol5.at_value(-1,3.6) == -23.92
    assert pol5.at_value(-1,3.6) == -23.92

if __name__ == '__main__':
    test()
