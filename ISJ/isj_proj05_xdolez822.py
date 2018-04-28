
from collections import OrderedDict

class Polynomial:
    """Polynomial class methods and definitions"""
    def __init__(self, *args, **kwargs):
        """Initialize method (on creation)"""
        #polynom coefficients list
        self.coeffs=list()
        #checking the arguments and converting them to list in correct form
        #whether is list and args is used(checking by len)
        if len(args) != 0 and isinstance(args[0],list):
            self.coeffs.extend(*args)
        #if kwards are not used and its not a list, then normal args are used
        elif not kwargs:
            #append all arguments
            for i in args:
                self.coeffs.append(i)
        #keywordargs
        else:
            #replacing x in keys, so we can convert them to int
            aux_d = {i.replace('x', ''): kwargs[i] for i in kwargs.keys()}
            #converting keys and values to ints
            aux_d = {int(key):int(value) for key, value in aux_d.items()}
            #adding 0s to unfilled spots
            for i in range(max(aux_d.keys())):
                if not i in aux_d.keys():
                    aux_d[i] = 0
            #sorting the dictionary, so it keys are by order
            aux_d = OrderedDict(sorted(aux_d.items()))
            #converting to list
            self.coeffs=list(aux_d[k] for k in aux_d)

    def __str__(self):
        """Printing method"""
        #returns polynom converted to string using private function __toString
        return self.__toString(self.coeffs)
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
                #bypass 0 values from start (f)
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
        return self.__toString(self.coeffs) == str(other)

    def __add__(self, other):
        """Adding polynomials"""
        #test to ensure that always we add to the bigger list
        if len(self.coeffs) >= len(other.coeffs):
            res_coeffs=self.coeffs[:]
            #iterating over all values
            for i in range(len(self.coeffs)):
                #check if we are not trying to add value that is not in the list
                if i < len(other.coeffs):
                    #adding values at indexes together
                    res_coeffs[i] += other.coeffs[i]
            return Polynomial(*res_coeffs)
        else:
            res_coeffs=other.coeffs[:]
            #same as before, only saving results into the other polynom
            for i in range(len(other.coeffs)):
                if i < len(self.coeffs):
                    res_coeffs[i] += self.coeffs[i]
            return Polynomial(*res_coeffs)


    def __pow__(self,n):
        """Exponentiating polynomials"""
        res_coeffs=Polynomial(1)
        #n times multiply self by self (initialized to one, because **1  needs to return itself)
        for i in range(n):
            res_coeffs *= self
        return res_coeffs

    def __mul__(self,other):
        """Multiplication of polynoms"""
        #creating a result list with size of polynoms max indexes
        #because for example 4 degree * 4 degree must give 8 degree polynom
        res_coeffs=[0]*(len(self.coeffs)+len(other.coeffs)-1)
        #multiplying every value by every other value
        for i in range(len(self.coeffs)):
            for j in range(len(other.coeffs)):
                res_coeffs[i+j] += self.coeffs[i] * other.coeffs[j]
        return Polynomial(*res_coeffs)

    def derivative(self):
        """Derivative of polynomial"""
        res_coeffs=[]
        #append to a list derivatives of members of polynomial
        for i,c in enumerate(self.coeffs):
            #exponent -1 and coeff=exponent*prev_coeff
            res_coeffs.insert(i-1,c*i)
        return Polynomial(*res_coeffs)

    def at_value(self,x1,x2=0):
        """Value of polynom for x, when 2 parameters are given, function returns differnece between them"""
        result=0
        #calculates the value of polynom
        for i,c in enumerate(self.coeffs):
            result+=c*(x1)**i
        if x2 == 0:
            return result
        #if optional second parameter given, calculate polynom again with other value and then return difference between their results
        else:
            result2=0
            for i,c in enumerate(self.coeffs):
                result2+=c*(x2)**i
            return result2-result

def test():
    pol1=Polynomial(1,1)
    pol1=pol1**100
    pol1.at_value(2.54546545646545645646501)
if __name__ == '__main__':
    test()
