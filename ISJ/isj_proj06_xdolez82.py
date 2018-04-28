#!/usr/bin/env python3
"""
Author: Zdenek Dolezal (xdolez82)
FIT VUT ISJ proj06 2017/18
"""
import itertools
import timeit


def first_nonrepeating(s):
    """Finds first nonrepeating letter in string"""

    if not isinstance(s, str):
        return None
    c_count = {}
    for c in s:
        if c not in c_count:
            c_count[c] = 1
        else:
            c_count[c] += 1
    for c in c_count:
        if c_count[c] == 1:
            if c.isprintable() and not c.isspace():
                return c
    return None


def combine4(four, result):
    """Returns possible numeric combinations of four numbers to get result"""

    if not (isinstance(four, list) and len(four) is 4) or not isinstance(result, int):
        return None

    symbols = ['+', '-', '*', '/']
    symbols_comb = []
    result_list = []
    number_perm_list = []

    for x in symbols_comb:
        for c in x:
            c = str(c)

    for number_list in itertools.permutations(four):
        number_list = [str(x) for x in number_list]
        for sc in itertools.product(symbols, repeat=3):
            sc = [str(x) for x in sc]
            p_comb = []

            p_comb.append(number_list[0] + sc[0] + number_list[1] + sc[1] + number_list[2] + sc[2] + number_list[3])
            p_comb.append(number_list[0] + sc[0] + '(' + number_list[1] + sc[1] + number_list[2] + sc[2] + number_list[3] + ')')
            p_comb.append('(' + number_list[0] + sc[0] + number_list[1] + sc[1] + number_list[2] + ')' + sc[2] + number_list[3])
            p_comb.append('(' + number_list[0] + sc[0] + number_list[1] + ')' + sc[1] + '(' + number_list[2] + sc[2] + number_list[3] + ')')
            p_comb.append('(' + number_list[0] + sc[0] + number_list[1] + ')' + sc[1] + number_list[2] + sc[2] + number_list[3])
            p_comb.append(number_list[0] + sc[0] + number_list[1] + sc[1] + '(' + number_list[2] + sc[2] + number_list[3] + ')')
            p_comb.append(number_list[0] + sc[0] + '(' + '(' + number_list[1] + sc[1] + number_list[2] + ')' + sc[2] + number_list[3] + ')')
            p_comb.append(number_list[0] + sc[0] + '(' + number_list[1] + sc[1] + '(' + number_list[2] + sc[2] + number_list[3] + ')' + ')')
            p_comb.append(number_list[0] + sc[0] + '(' + number_list[1] + sc[1] + number_list[2] + ')' + sc[2] + number_list[3])
            p_comb.append('(' + '(' + number_list[0] + sc[0] + number_list[1] + ')' + sc[1] + number_list[2] + ')' + sc[2] + number_list[3])
            p_comb.append('(' + number_list[0] + sc[0] + '(' +  number_list[1] + sc[1] + number_list[2] + ')' + ')' + sc[2] + number_list[3])
            #zero division error
            for x in p_comb:
                try:
                    if eval(x) == result:
                        if x not in result_list:
                            result_list.append(x)
                except (ZeroDivisionError):
                    pass

    return result_list
