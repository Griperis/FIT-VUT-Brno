#!/usr/bin/env python3
def can_be_a_set_member_or_frozenset(item):
    """
    checking whether the item can be a member of set
    only a list or dict can't be members of a set
    """
    if isinstance(item, list) or isinstance(item, dict):
        return frozenset(item)
    else:
        return item

def all_subsets(lst):
    """"
    all subsets of a set
    @param lst: input set
    @return lst_out: potent set of lst
    """
    lst_out=[[]]
    #cycling through items list and adding different subsets
    for item in lst:
        prev_lst = []
        for each in lst_out:
            tmp_lst=list(each)
            tmp_lst.append(item)
            prev_lst.append(list(tmp_lst))
        lst_out.extend(prev_lst)
    return lst_out

def all_subsets_excl_empty(*input, exclude_empty=True):
    """
    analogy to all_subsets, differentiates only with types of arguments
    """
    lst = list(input)
    #calling all_subset function
    lst_out=all_subsets(lst)
    #removing empty set (which is at first position) if opt arg is set
    if (exclude_empty == True):
        lst_out.pop(0)
    return lst_out

assert can_be_a_set_member_or_frozenset(1) == 1
assert can_be_a_set_member_or_frozenset((1,2)) == (1,2)
assert can_be_a_set_member_or_frozenset([1,2]) == frozenset([1,2])

assert all_subsets(['a', 'b', 'c']) == [[], ['a'], ['b'], ['a', 'b'], ['c'], ['a', 'c'], ['b', 'c'], ['a', 'b', 'c']]

assert all_subsets_excl_empty('a', 'b', 'c') == [['a'], ['b'], ['a', 'b'], ['c'], ['a', 'c'], ['b', 'c'], ['a', 'b', 'c']]
assert all_subsets_excl_empty('a', 'b', 'c', exclude_empty = True) == [['a'], ['b'], ['a', 'b'], ['c'], ['a', 'c'], ['b', 'c'], ['a', 'b', 'c']]
assert all_subsets_excl_empty('a', 'b', 'c', exclude_empty = False) == [[], ['a'], ['b'], ['a', 'b'], ['c'], ['a', 'c'], ['b', 'c'], ['a', 'b', 'c']]
