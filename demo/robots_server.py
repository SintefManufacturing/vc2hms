#!/usr/bin/python2

import math3d
from icehms import _LightHolon, AgentManager, hms

from urx import Robot

class IceRobot(hms.Message, hms.GenericRobot, hms.Agent, _LightHolon):
    def __init__(self, name, ip):
        _LightHolon.__init__(self, name)
        self.robot = Robot(ip)
        self._ilog("Robot: ", name, " created", level=1)

    def movel(self, pose, vel, acc, current=None):
        self.robot.movel(pose, acc, vel)

    def movej(self, pose, vel, acc, current=None):
        self.robot.movej(pose, acc, vel)

    def getl(self, current=None):
        return self.robot.getl()

    def getj(self, current=None):
        return self.robot.getj()

    def setTCP(self, tcp, current=None):
        self.robot.set_tcp(tcp)

    def grasp(self, current=None):
        self.robot.set_digital_out(4, 1)
        self.robot.set_digital_out(3, 0)

    def release(self, current=None):
        self.robot.set_digital_out(4, 0)
        self.robot.set_digital_out(3, 1)

    def cleanup(self):
        self.robot.cleanup()




if __name__ == "__main__":
    from math import pi
    ida = IceRobot( "Ida", "192.168.1.5")
    sophie = IceRobot( "Sophie" , "192.168.1.6")

    icalib = math3d.Transform()
    icalib.orient.rotateZ(pi/4)
    icalib.orient.rotateYB(pi)
    icalib.pos = math3d.Vector(0, 0, 0.83)
    ida.robot.set_calibration_matrix(icalib)
    ida.robot.set_tcp((0, 0, 0.278, 0, 0, 0))

    scalib = math3d.Transform()
    scalib.orient.rotateZ(pi/18 - pi/2)
    scalib.orient.rotateXT(pi)
    scalib.pos = math3d.Vector(0, 0, 0.825)
    sophie.robot.set_calibration_matrix(scalib)
    sophie.robot.set_tcp((0, 0, 0.288, 0, 0, 0))

    mgr = AgentManager("LocalAgentManager")
    mgr.addHolon(ida)
    mgr.addHolon(sophie)
    mgr.waitForShutdown()


