#!/usr/bin/python
import sys
from struct import unpack
import logging
import time

from icehms import Holon, startHolonStandalone, hms

#class Client(Holon, hms.GenericEventInterface):
class CellCtrl(Holon):
    def __init__(self):
        Holon.__init__(self, "CellCtrl", logLevel=logging.INFO )

    def run(self):
        #self._subscribeTopic(self._tn)
        self._subscribeEvent("Sensor Conveyor::SensorSignal")
        ur = self._icemgr.getHolon("UR10")
        ur = ur.ice_timeout(100000)
        ur.setCSYS(hms.CSYS.World)
        #ur.movel((1.153155317981248, -0.03674964688098066, 1.221240297626111, 4.712485335623291e-06, -9.668942806603778e-06, 0.17298867413255042), 0.1, 0.01)
        init = [1.14, 0, 2.06, 6.720322348499538, 4.905581723624449, -1.6546270059573371]
        print("Moving to init pose")
        ur.movej(init, 100, 30)
        while not self._stop:
            time.sleep(0.1)
            if len(self.mailbox) > 0:
                msg = self.mailbox.pop()
                pose  = msg.arguments["WorldPositionMatrix"]
                print("We have a part at", pose)
                print("We are at: ", ur.getl())
                pose = [float(i) for i in pose.split(",")]
                pose = pose[:3]
                pose[2] += 0.1
                print("Moving to: ", pose)
                ur.translate(pose, 0.1, 0.1)
                ur.grasp()
                pose[2] += 0.2
                pose[1] -= 1.0
                print("start placing at: ", pose)
                ur.translate(pose, 0.1, 0.1)
                pose[2] -= 0.2
                ur.translate(pose, 0.1, 0.1)
                print("release")
                ur.release()
                print("Moving to init pose")
                ur.movej(init, 100, 30)





if __name__ == "__main__":
    s = CellCtrl()
    startHolonStandalone(s)
