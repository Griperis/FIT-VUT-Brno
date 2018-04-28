#!/usr/bin/env python3
"""
Author: Zdenek Dolezal (xdolez82)
FIT VUT ISJ proj07 2017/18
"""
import math

class TooManyCallsError(Exception):
    """Exception for limit_calls"""
    def __init__(self, message):
        """Returns message to the super class"""
        super().__init__(message)


def limit_calls(max_calls=2, error_message_tail="called too often"):
    """Limit calls decorator"""
    def real_decorator(function):
        """Decorator"""
        def wrapper(*args, **kwargs):
            """Real decorator wrapper"""
            wrapper.called += 1
            if wrapper.called > max_calls:
                message = "function \"" + str(function.__name__) + "\" - " + error_message_tail
                raise(TooManyCallsError(message))
            return function(*args, **kwargs)
        wrapper.called = 0
        return wrapper
    return real_decorator


def ordered_merge(*args, **kwargs):
    """Ordered merge - selects items from iterable types by selector"""
    if "selector" not in kwargs:
        return []
    indexes = kwargs["selector"]
    res_list = []
    indexes_use = [0] * (len(indexes))
    i = 0
    for x in indexes:
        res_list.insert(i, args[x][indexes_use[x]])
        indexes_use[x] += 1
        i += 1
    return res_list


class Log:
    """Logging class"""
    def __init__(self, logfile):
        """Initialization method - opens logfile"""
        self.__file = open(logfile, "w")

    def logging(self, str):
        """Method logging used to write to the file"""
        self.__file.write(str + "\n")

    def __enter__(self):
        """Enter method - called at the start of block"""
        self.__file.write("Begin\n")
        return self

    def __exit__(self, type, value, traceback):
        """Exit method - called when at the end of block"""
        self.__file.write("End\n")
        self.__file.close()
