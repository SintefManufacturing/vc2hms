#ifndef _VC2ICE_ICE
#define _VC2ICE_ICE

#include <hms.ice>

module hms {

	interface Simulation extends Holon { //interface to the entire Visual Component simulation
        void start();
        void stop();
        void reset();
    };

	interface VCComponent extends Holon {
		string getProperty(string name);
		StringSeq getPropertyList();
		void setProperty(string name, string val);
	};

	interface Robot extends GenericRobot {
		string getProperty(string name);
		StringSeq getPropertyList();
		void setProperty(string name, string val);
        
    };

	interface Conveyor extends VCComponent {
        void start();
        void stop();
    };

	interface Feeder extends VCComponent {
        void start();
        void stop();
        void setSequence(DoubleSeq feedseq); //a sequence of double representing the frequency for every object prpoduced by the feeder
    };
};


#endif 
