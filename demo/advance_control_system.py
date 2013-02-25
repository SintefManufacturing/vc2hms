#!/usr/bin/python2
import sys
import time
import icehms

if __name__ == "__main__":
    name = sys.argv[1]
    mgr = icehms.IceManager()
    mgr.init()
    try:
        print("Holons are: ", mgr.findHolons())
        rob = mgr.getHolon(name)
        print("My robot is: ", rob)
        rob = rob.ice_timeout(10000)
        p = rob.getl()
        print("Current pose is: ", rob.getl())
        p[2] += 0.10
        print("Sending robot to: ", p)
        rob.movel(p, 0.1, 0.01)
        print("Current pose is: ", rob.getl())
        p[1] += 0.10
        rob.movel(p, 0.1, 0.01)
        print("Current pose is: ", rob.getl())
        p[2] -= 0.10
        rob.movel(p, 0.1, 0.01)
        p[1] -= 0.10
        rob.movel(p, 0.1, 0.01)

    finally:
        mgr.shutdown()

