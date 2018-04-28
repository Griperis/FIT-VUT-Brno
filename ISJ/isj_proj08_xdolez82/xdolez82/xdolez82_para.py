#!/usr/bin/env python3

import multiprocessing as mp

#processes are not affected by GIL
def count(n):
    while n > 0:
        n -= 1

p1 = mp.Process(target=count,args=(10**8,))
p1.start()
p2 = mp.Process(target=count,args=(10**8,))
p2.start()
p1.join(); p2.join()
