#ifndef _VC2ICE_ICE
#define _VC2ICE_ICE

#include <hms.ice>

module hms {

	interface PropertyList {
		string getProperty(string name);
		StringSeq getPropertyList();
		void setProperty(string name, string val);
	} ;

	interface Simulation extends Holon, PropertyList { //interface to the entire Visual Component simulation
        void start();
        void stop();
        void reset();
    };

	interface VCComponent extends Holon, PropertyList {

	};

	interface Robot extends GenericRobot, PropertyList {
        
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
