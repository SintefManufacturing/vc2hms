#ifndef _VC2ICE_ICE
#define _VC2ICE_ICE

#include <hms.ice>

module hms {

	interface Simulation { //interface to the entire Visual Component simulation
        void start();
        void stop();
        void reset();
    };

	interface Robot extends RobotMotionCommand {
        
    };

	interface Conveyor extends Holon {
        void start();
        void stop();
    };

	interface Feeder extends Holon {
        void start();
        void stop();
        void setSequence(DoubleSeq); //a sequence of double representing the frequency for every object prpoduced by the feeder
    };
}


#endif 
