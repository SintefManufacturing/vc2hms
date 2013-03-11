#!/usr/bin/python
import sys
from struct import unpack
import logging
import time

from icehms import Holon, run_holon, hms, Message

#class Client(Holon, hms.GenericEventInterface):
class CellCtrl(Holon):
    def __init__(self):
        Holon.__init__(self, "CellCtrl", logLevel=logging.INFO )

    def run(self):
        #self._subscribeTopic(self._tn)
        self._subscribe_topic("Sensor Conveyor::SensorSignal")


        stopmsg = Message()
        stopmsg.arguments["SignalType"] = "BooleanSignal"
        stopmsg.arguments["SignalValue"] = "False"
        stopmsg.arguments["SignalName"] = "StartStop"


        startmsg = Message()
        startmsg.arguments["SignalType"] = "BooleanSignal"
        startmsg.arguments["SignalValue"] = "True"
        startmsg.arguments["SignalName"] = "StartStop"

        conv = self._icemgr.get_holon("Sensor Conveyor")
        ur = self._icemgr.get_holon("UR10")
        init = [0.0, 0.0, 1.5707936732062238, 0, -1.57, 0.0]
        if True:
            ur = self._icemgr.get_holon("KR 16-2")
            init = [0.0, -1.570795, 1.570795, 0.0, 1.57, 0.0]
        ur = ur.ice_timeout(100000)
        ur.set_csys(hms.CSYS.World)
        print("Moving to init pose")
        ur.movej(init, 100, 30)
        while not self._stop:
            time.sleep(0.1)
            if len(self.mailbox) > 0:
                msg = self.mailbox.pop()
                conv.put_message(stopmsg)
                pose  = msg.arguments["WorldPositionMatrix"]
                print("We have a part at", pose)
                print("We are at: ", ur.getl())
                pose = [float(i) for i in pose.split(",")]
                pose = pose[:3]
                pose[2] += 0.2
                print("Moving to: ", pose)
                ur.translate(pose, 0.8, 0.8)
                ur.grasp()
                pose[2] += 0.2
                pose[1] -= 1.0
                conv.put_message(startmsg)
                print("start placing at: ", pose)
                ur.translate(pose, 0.8, 0.8)
                pose[2] -= 0.2
                ur.translate(pose, 0.8, 0.8)
                print("release")
                ur.release()
                print("Moving to init pose")
                ur.movej(init, 0.01, 1)





if __name__ == "__main__":
    import logging
    s = CellCtrl()
    run_holon(s, logLevel=logging.DEBUG)
