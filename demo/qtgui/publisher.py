#!/usr/bin/python

import time
import sys
from struct import pack, unpack
import logging

from icehms import Holon, run_holon, hms, Message

class Server(Holon):
    def __init__(self, name, logLevel=logging.INFO):
        Holon.__init__(self, name, logLevel=logLevel )

    def run(self):
        pub = self._get_publisher("Conveyor::MySignal")
        counter = 0
        while not self._stop:
            counter +=1
            msg = Message()
            msg.sender = "Conveyor"
            msg.arguments["MessageType"] = "Signal"
            msg.arguments["SignalType"] = "BooleanSignal"
            msg.arguments["SignalValue"] = "False"
            msg.arguments["SignalName"] = "MySignal"
            pub.put_message(msg)
            time.sleep(1)
    

if __name__ == "__main__":
    s = Server("MyPubklisher")
    run_holon(s)
