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
    if file is None:
        print("Failed to open proj2.out file")
        return -1
    tmp = []
    riders = []
    riders_boarded = []
    error = 0
    error_action = 0
    error_boarding = 0
    riders_actual = 0
    boarding = False
    arrival = 0
    plus_error = 0
    riders_cur_boarded = []
    for line in file:
        line = "".join(line.split())
        tmp.append(line.split(":"))
        if line is "":
            print("Error: There should be no empty lines in output file")
            return -1
    if tmp is []:
        print("Warninig: file is empty")
        plus_error += 1

    for x in range(len(tmp)):
        if eval(tmp[x][0]) != x+1:
            print("Error: line:",x+1)
            error_action += 1
        if ''.join(i for i in tmp[x][1] if not i.isdigit()) == "RID":
            riderID = eval(re.findall('\d+', tmp[x][1])[0])
            if tmp[x][2] == "start":
                riders_actual += 1
                riders.insert(riderID-1, True)
            if boarding is True and tmp[x][2] == "boarding":
                riders_boarded.insert(riderID-1,True);
                riders_cur_boarded.append(riderID);
            if boarding is True and tmp[x][2] == "enter":
                error_boarding += 1
                print("Error: riders cant enter stop if bus arrived, line:",x+1)
            if tmp[x][2] == "finish":
                if boarding is True and riderID in riders_cur_boarded:
                    print("Error: riders should not finish in their ride")
                    riders_cur_boarded = []
                riders[riderID-1] = False

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
        if riders_boarded[x] is not True:
            print("Error: RID:",x+1,"did not board");
            error += 1

    if arrival is not 0:
        print("proj2.out_test",testn,": Error: bus arrived and didnt depart somewhere")
    if r != riders_actual:
        print("proj2.out_test",testn,": Error:", riders_actual,"riders", r, "expected")
        plus_error += 1
    else:
        print("proj2.out_test",testn,": all riders generated")
    if error is 0:
        print("proj2.out_test",testn,": all riders finished")
    if error_action is 0:
        print("proj2.out_test",testn,": actions correct")
    if error_boarding is 0:
        print("proj2.out_test",testn,": boarding correct")
    print("-------------------------------------")
    file.close()

    comp2 = []
    file = open("proj2.out","r")
    for line in file:
        line = "".join(line.split())
        comp2.append(line.split(":"))
    if len(comp2) > len(tmp):
        print("++ Something was added to file after execution")
        plus_error += 1
    file.close();
    return arrival + error + error_action + error_boarding + plus_error
if __name__ == '__main__':
    error_total = 0
    if len(sys.argv) is not 2:
        r, c, art, abt = (int(x) for x in sys.argv[1:])
        error_total += test(r, c, art, abt,1)
    else:
        for i in range(int(sys.argv[1])):
            r = random.randint(1,500)
            c = random.randint(1,100)
            art = random.randint(0,200)
            abt = random.randint(0,200)
            error_total += test(r,c,art,abt,i+1)
    print("proj2.out_test: summary: total errors:",error_total)
