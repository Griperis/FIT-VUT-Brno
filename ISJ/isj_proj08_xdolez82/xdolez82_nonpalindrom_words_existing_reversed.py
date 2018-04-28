#!/usr/bin/env python3

import fileinput

words = set()
for line in fileinput.input():
    words.add(line.rstrip())
#if checking whether something is present or not, its better to use set
palindroms = [w for w in words if w == w[::-1]]
palindroms = set(palindroms)
#palindroms = set(palindroms)

result = [w for w in words if w not in palindroms and w[::-1] in words]
print(sorted(result))
