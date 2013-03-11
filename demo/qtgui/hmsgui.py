# This is the BE Line Control Monitor application with GUI
from PyQt4.QtCore import *
from PyQt4.QtGui import *
import time
from threading import Lock

from icehms import Holon, AgentManager

from ui_hmsgui import Ui_HMS

class HMSGui(QWidget, Ui_HMS):
    def __init__(self, parent=None):
        QWidget.__init__(self, parent)
        self.setupUi(self)

    def signal1Slot(self, stype, sname, sval):
        print("We got signal:, ", sname, sval)
        print type(sval)
        item = QListWidgetItem()
        item.setText(sname + "::" + str(sval))
        self.listWidget.addItem(item)

    def signal2Slot(self, stype, sname, sval):
        print("We got signal, should do something in the gui", sval)


class QHolon(QObject, Holon):
    """
    to be used to create a brige between vc2hms signals and Qt Signal
    """
    def __init__(self, parent):
        QObject.__init__(self, parent)
        Holon.__init__(self, "GUIHolon")
        self._lock = Lock()
        self._sigs = []

    def connect_sig(self, sender, name, slot):
        """Using old pyqt signal syntax in order to create signals on the fly"""
        sigid = sender + "::" + name 
        signame = sigid + "(PyQt_PyObject, PyQt_PyObject, PyQt_PyObject)"
        print(signame, SIGNAL(signame), slot)
        self.connect(self, SIGNAL(signame), slot)
        with self._lock:
            self._sigs.append((sigid, signame))
        self._subscribe_topic(sigid)

    def _get_msg_vals(self, msg):
        mtype = None
        mname = None
        mval = None
        if msg.arguments.has_key("SignalType"):
            mtype = msg.arguments["SignalType"]
        if msg.arguments.has_key("SignalName"):
            mname = msg.arguments["SignalName"]
        if msg.arguments.has_key("SignalValue"):
            mval = msg.arguments["SignalValue"]
        return (mtype, mname, mval)

    def put_message(self, msg, cur):
        """
        Override Holon put_message method, this mean nothin arrive in the mailbox
        """
        print msg
        with self._lock:
            stype, sname, sval  = self._get_msg_vals(msg)
            msgid = msg.sender + "::" + sname
            if stype == "BooleanSignal":
                sval = bool(sval)
            elif stype == "IntegerSignal":
                sval = int(sval)
            elif stype == "RealSignal":
                sval = float(sval)
            for sigid, signame in self._sigs:
                if  msgid == sigid:
                    self.emit(SIGNAL(signame), stype, sname, sval)

            
            




class GUIHolon(QHolon):
    
    def __init__(self, window):
        QHolon.__init__(self, window)
        self.window = window
    
    def run(self):
        #first connect signals to method in gui application
        self.connect_sig("Conveyor", "MySignal", self.window.signal1Slot)

if __name__ == '__main__':
    import sys
    
    app = QApplication(sys.argv)
    window = HMSGui()
    mgr = AgentManager("GUIAdapter")
    try: 
        holon = GUIHolon(window)
        mgr.add_holon(holon)
        window.show()
        app.exec_()
    finally:
        mgr.shutdown()


