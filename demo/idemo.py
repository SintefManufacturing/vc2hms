#!/usr/bin/python2

import icehms

def moveup(rob, vel=0.01):
    l = rob.getl()
    l[2] += 0.2
    rob.movel(l, acc=1000, vel=vel)

def movedown(rob, vel=0.01):
    l = rob.getl()
    l[2] -= 0.2
    rob.movel(l, acc=1000, vel=vel)

if __name__ == "__main__":
    mgr = icehms.IceManager()
    mgr.init()
    alive = []
    try:
        print("Holons are: ", mgr.findHolons())
        try:
            ida = mgr.getHolon("Ida")
            ida = ida.ice_timeout(20000)
            alive.append(ida)
        except Exception, ex:
            print("Could not create ur", ex)
        try:
            ur = mgr.getHolon("UR5")
            ur = ur.ice_timeout(20000)
            alive.append(ur)
        except Exception, ex:
            print("Could not create ur", ex)
        try:
            ur10 = mgr.getHolon("UR10")
            ur10 = ur10.ice_timeout(20000)
            alive.append(ur10)
        except Exception, ex:
            print("Could not create ur10", ex)

        from IPython.frontend.terminal.embed import InteractiveShellEmbed
        ipshell = InteractiveShellEmbed( banner1="\nStarting IPython shell, available objects are:\n " + str(alive) + "\n")
        ipshell(local_ns=locals())
    finally:
        mgr.shutdown()

