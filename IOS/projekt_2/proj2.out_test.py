#!/usr/bin/env python3
import re
import sys
import os
import subprocess
import random

def test(r, c, art, abt,testn):
    print("proj2.out_test",testn,": ./proj2",r,c,art,abt)
    cwd = os.getcwd()
    if (subprocess.call(["./proj2", str(r), str(c), str(art), str(abt)]) is not 0):
        print("Error: Failed executing ./proj2")
        return


    file = open("proj2.out","r")
    tmp = []
    riders = []
    error = 0
    error_action = 0
    error_boarding = 0
    riders_actual = 0
    boarding = False
    arrival = 0
    plus_error = 0
    for line in file:
        line = "".join(line.split())
        tmp.append(line.split(":"))
    if tmp is []:
        print("Warninig: file is empty")
        plus_error += 1
    for x in range(len(tmp)):
        if eval(tmp[x][0]) != x+1:
            print("Error: line:",x+1)
            error_action += 1
        if ''.join(i for i in tmp[x][1] if not i.isdigit()) == "RID":
            if tmp[x][2] == "start":
                riders_actual += 1
                riders.insert(eval(re.findall('\d+', tmp[x][1])[0])-1, True)
            if tmp[x][2] == "finish":
                riders[eval(re.findall('\d+', tmp[x][1])[0])-1] = False
            if boarding is True and tmp[x][2] == "enter":
                error_boarding += 1
                print("Error: riders cant enter stop if bus arrived, line:",x+1)

        elif tmp[x][1] == "BUS":
            if tmp[x][2] == "arrival":
                arrival += 1
            if tmp[x][2] == "depart":
                arrival -= 1
            if boarding is True and (tmp[x][2] == "depart" or tmp[x][2] == "arrival"):
                print("Error: bus cant depart or arrive when boarding, line:",x+1)
                error_boarding += 1
            if tmp[x][2] == "startboarding":
                if arrival == 1:
                    boarding = True
                    waiting = int(tmp[x][3])
                elif arrival == 0:
                    print("Error: cant board without arriving, line:",x+1)
                    error_boarding += 1
                else:
                    print("Error: bus didnt depart and you board",x+1)
                    error_boarding += 1
            if tmp[x][2] == "endboarding":
                end_waiting = int(tmp[x][3])
                boarded = waiting - end_waiting
                if c < boarded:
                    print("Error:",boarded,"riders instead of",c,"boarded, line:",x+1)
                    error_boarding += 1
                boarding = False
        else:
            print("Error: RID or BUS expected,line:",x+1)
    for x in range(len(riders)):
        if riders[x] is not False:
            print("Error: RID:",x+1,"did not finish")
            error += 1


    if arrival is not 0:
        print("proj2.out_test",testn,": Error: bus arrived and didnt depart somewhere")
    if r != riders_actual:
        print("proj2.out_test",testn,": Error:", riders_actual,"riders", r, "expected")
        pluse_error += 1
    else:
        print("proj2.out_test",testn,": all riders generated")
    if error is 0:
        print("proj2.out_test",testn,": all riders finished")
    if error_action is 0:
        print("proj2.out_test",testn,": actions correct")
    if error_boarding is 0:
        print("proj2.out_test",testn,": boarding correct")
    print("-------------------------------------")
    return arrival + error + error_action + error_boarding + plus_error
    file.close()

if __name__ == '__main__':
    error_total = 0
    if len(sys.argv) is not 2:
        r, c, art, abt = (int(x) for x in sys.argv[1:])
        error_total += test(r, c, art, abt,1)
    else:
        for i in range(int(sys.argv[1])):
            r = random.randint(1,100)
            c = random.randint(1,100)
            art = random.randint(0,50)
            abt = random.randint(0,50)
            error_total += test(r,c,art,abt,i+1)
    print("proj2.out_test: summary: total errors:",error_total)
