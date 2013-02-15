#!/usr/bin/python

import sys

from icehms import Holon, startHolonStandalone

class Client(Holon):
    def __init__(self):
        Holon.__init__(self )
        print "Monitoring all events"

    def get_topics(self):
        topics = self._icemgr.getAllEventTopics()
        print "Events Topics are: \n"
        top = []
        for name, prx in topics.items():
            top.append(name)
        return top

    def subscribeToAll(self):
        for name in self.get_topics():
            self._subscribeEvent(name)

    def run(self):
        self.subscribeToAll()

    def newEvent(self, name, stringList, bytesStr, ctx=None):
        pass

    def putMessage(self, msg, ctx=None):
        print "New event from: ", msg.sender


if __name__ == "__main__":
    s = Client()
    startHolonStandalone(s)
