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
        print("Holons are: ", mgr.find_holons())
        try:
            ida = mgr.get_holon("Ida")
            ida = ida.ice_timeout(20000)
            alive.append(ida)
        except Exception as ex:
            pass
        else:
            print("found ida")
        try:
            ur = mgr.get_holon("UR5")
            ur = ur.ice_timeout(20000)
            alive.append(ur)
        except Exception as ex:
            pass
        else:
            print("found ur")
        try:
            ur10 = mgr.get_holon("UR10")
            ur10 = ur10.ice_timeout(20000)
            alive.append(ur10)
        except Exception as ex:
            pass
        else:
            print("found ur10")
        try:
            conv = mgr.get_holon("Sensor Conveyor")
            alive.append(conv)
        except Exception as ex:
            pass
        else:
            print("found conv")

        msg = icehms.Message()
        msg.arguments["SignalType"] = "BooleanSignal"
        msg.arguments["SignalValue"] = "False"
        msg.arguments["SignalName"] = "StartStop"

        from IPython.frontend.terminal.embed import InteractiveShellEmbed
        ipshell = InteractiveShellEmbed( banner1="\nStarting IPython shell, available objects are:\n " + str(alive) + "\n")
        ipshell(local_ns=locals())
    finally:
        mgr.shutdown()

