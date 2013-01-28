using System;
using System.Collections.Generic;
using vcCOM;
using vcCOMExecutor;

namespace vc2ice
{
    [Serializable]
    public class UnreachableException : Exception
    {
        public string ErrorMessage
        {
            get
            {
                return base.Message.ToString();
            }
        }

        public UnreachableException(string errorMessage)
            : base(errorMessage) { }

        public UnreachableException(string errorMessage, Exception innerEx)
            : base(errorMessage, innerEx) { }
    }

    public enum MoveType { Linear, Joint };
    public class Move
    {
        IvcMotionInterpolator Motion;
        IvcMotionTarget Target;
        private IvcRobot Controller;
        
        double StartTime; 
        double EndTime;
        IvcApplication  App;
        hms.CSYS CSRef;

        public Move(IvcApplication app, MoveType move, IvcRobot controller, double[] pose, double speed = 2, double acc = 1, hms.CSYS cref = hms.CSYS.World)
        {
            Controller = controller;
            Motion = Controller.createMotionInterpolator();
            Target = Controller.createTarget();
            CSRef = cref;

            App = app;

            if (move == MoveType.Linear)
            {
                setupLinearMove(pose);
            }
            else
            {
                setupJointMove(pose);
            }


            //Console.WriteLine("Is target reachable: " + Target.getConfigWarning(Motion.TargetCount - 1));
            //Target.getConfigWarning(0);
            //Target.getConfigWarning(1);

            //if ( Target.getConfigWarning(Motion.TargetCount - 1) == 1) 
            //{
            //   throw(new UnreachableException("Target is not reachable"));
            //}

            double end = Motion.getCycleTimeAtTarget(Motion.TargetCount - 1);
            Console.WriteLine("Computed time: " + Convert.ToString(end));
            Console.WriteLine("Starting move");

            StartTime = App.getProperty("SimTime");
            EndTime = StartTime + end;
        }

        public void setupLinearMove(double[] pose)
        {
            Target.MotionType = 1; // 1 is Linear, 0 joint
            switch (CSRef)
            {
                case hms.CSYS.Base:
                    Target.TargetMode = 1; // robot base as reference
                    break;
                case hms.CSYS.World:
                    Target.TargetMode = 4; //  world as reference
                    break;
                default:
                    goto case hms.CSYS.World;

            }
            double[] current = Target.RobotRootToRobotFlangeMatrix;

            //adding start point to trajectory
            Target.TargetMatrix = current;
            Motion.addTarget(ref Target);
            Target.CurrentConfig = Target.NearestConfig;

            //Now the real target
            Target.TargetMatrix = pose;
            Motion.addTarget(ref Target);
        }

        public void setupJointMove(double[] pose)
        {
            Target.MotionType = 0; // 1 is Linear, 0 joint
            Target.TargetMode = 1; // robot base as reference

            double[] current = Target.RobotRootToRobotFlangeMatrix;

            //adding start point to trajectory
            Target.TargetMatrix = current;
            Motion.addTarget(ref Target);
            Target.CurrentConfig = Target.NearestConfig;

            //Now the real target
            for (int i = 0; i < pose.Length; i++)
            {
                Target.setJointValue(i, pose[i]);
            }
            Motion.addTarget(ref  Target);
        }


        public bool isFinished()
        {
            double t = App.getProperty("SimTime");
            if (t > EndTime ) 
            {
                return true;
            }
            else{
                return false;
            }

        }

        public double[] getPose()
        {
            double[] bjoints = new double[Controller.JointCount];
            double now = App.getProperty("SimTime");
            if (now > EndTime)
            {
                now = EndTime;
            }
            now =  now - StartTime;
            try
            {
                Motion.interpolate(now, ref Target);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Interpolation error: " + ex);
            }
            for (int i = 0; i < Target.RobotJointCount; i++)
            {
                bjoints[i] = Target.getJointValue(i);
            }
            Helpers.printMatrix("Interpolated joint pose is: ", bjoints);
            return bjoints;
        }  
    }
    

 
    public class VCRobot : VCComponent, hms.RobotOperations_, IvcExecutorClient
    {
        private IvcRobot Controller;
        private List<IvcEventProperty> Joints;
        private IvcApplication App;
        //private hms.RobotMotionCommandTie_ RobotServant;
        //public override hms.HolonTie Servant { get; set; }
        private IvcExecutor Executor;
        private Move CurrentMove;
        private IvcSignalMap DigitalInput;
        private IvcSignalMap DigitalOutput;
        private hms.CSYS defaultCSYS = hms.CSYS.World;


        public VCRobot(IvcApplication vc, icehms.IceApp app, IvcComponent robot)  : base (app, robot, false)
        {

            //we called base with activate=false so we need to create our own "tie servant"
            register((Ice.Object)new hms.RobotTie_(this));

            App = vc;
            object[] result = Component.findBehavioursOfType("RobotController");
            if (result.Length == 0)
            {
                log("robot without controller found");
            }
            else
            {
                Controller = (IvcRobot) result[0];
                // controller is set now populate the joints list
                IvcPropertyList2 compprops = (IvcPropertyList2)Component;
                Joints = new List<IvcEventProperty>();
                for (int k = 0; k < Controller.JointCount; k++)
                {
                    string jointname = (string)Controller.getJoint(k).getProperty("Name");
                    IvcEventProperty joint = (IvcEventProperty)compprops.getPropertyObject(jointname);
                    Joints.Add(joint);
                }
            }          
            //executor stuff
            result = Component.findBehavioursOfType("RslExecutor");
            Executor = (IvcExecutor) result[0];  
            Executor.addExecutorClient(this);
             IvcRslExecutor rslex =  (IvcRslExecutor) Executor;
            ((IvcPropertyList)Executor).setProperty("ExecutionMode", true);  //we are ready
            string name = ((IvcPropertyList)Executor).getProperty("DigitalMapIn");
            DigitalInput = (IvcSignalMap) Component.findBehaviour(name);
            name = ((IvcPropertyList)Executor).getProperty("DigitalMapOut");
            DigitalOutput = (IvcSignalMap)Component.findBehaviour(name);
            log("nb ports output; " + DigitalOutput.getProperty("PortCount"));
            //DigitalOutput.getPortSignal(2);
        }


        public void setCSYS(hms.CSYS csys, Ice.Current current = null)
        {
            defaultCSYS = csys;
        }
        public List<string> getJoints()
        {
            List<string> list = new List<string>();
            foreach (IvcEventProperty j in Joints)
            {
                list.Add(j.getProperty("Name"));
            }
            return list;
        }

        public double[] getj(Ice.Current current=null)
        {
            List<double> list = new List<double>();
            foreach (IvcEventProperty j in Joints)
            {
                list.Add(j.Value * 3.14159 / 180);
            }         
            return list.ToArray();
        }

        public void setJointVal(int idx, double val)
        {
            Joints[idx].Value = val;
        }

        public double getJointVal(int idx)
        {
            return Joints[idx].Value;
        }

        public void setJointsPos(double[] pose)
        {
            for(int i=0; i < Joints.Count; i++)
            {
                //Console.WriteLine("Moving joint: " + i.ToString() + " to " + pose[i].ToString());
                IvcEventProperty j = Joints[i];
                j.Value = pose[i];
            }

        }

        public void movej(double[] pose, double acc = 2, double speed=0.1, Ice.Current icecurrent=null)
        {
            for (int i=0; i < pose.Length; i++ )
            {
                pose[i] = pose[i] * 180 / 3.14159;
            }
            log("New joint move command: ");
            lock (this)
            {
                CurrentMove = new Move(App, MoveType.Joint, Controller, pose, speed * 180 / 3.141, acc * 180 / 3.141);
            }
        }



        public double[] getl(Ice.Current current=null)
        {
            IvcMotionInterpolator motion = Controller.createMotionInterpolator();
            IvcMotionTarget target = Controller.createTarget();
            double[] matrix;
            switch (defaultCSYS)
            {
                case hms.CSYS.Base:
                     matrix = target.RobotRootToRobotFlangeMatrix.Copy();
                     for (int i = 0; i < matrix.Length; i++)
                     {
                         matrix[i] = matrix[i] / 1000;
                     }
                    return matrix;
                case hms.CSYS.World:
                    matrix = Helpers.AddMatrix(target.WorldToRootNodeMatrix, target.RootNodeToRobotRootMatrix );
                    matrix = Helpers.AddMatrix(matrix, target.RobotRootToRobotFlangeMatrix );
                    for (int i = 0; i < matrix.Length; i++)
                    {
                        matrix[i] = matrix[i] / 1000;
                    }
                    return matrix;
                default:
                    goto case hms.CSYS.World;       
            }
        }

        public void setCSYS(hms.CSYS cref){
            defaultCSYS = cref;
        }

        public void movel(double[] pose, double acc = 2, double speed = 1, Ice.Current icecurrent=null)
        {
            log("New move command: ");
            for (int i = 0; i < pose.Length; i++)
            {
                pose[i] = pose[i] * 1000;
            }
            lock (this)
            {
                CurrentMove = new Move(App, MoveType.Linear, Controller, pose, speed*1000, acc*1000, defaultCSYS);
            }
        }



        public void notifyBinaryInputValueChange(int Index, bool Value)
        {
            log("Binary input: " + Index + " has been changed to: " + Value);
        }

        public void notifyIntegerInputValueChange(int Index, int Value)
        {

        }

        public void notifyJointReportInterval(int Interval)
        {

        }

        public void notifyRealInputValueChange(int Index, double Value)
        {

        }

        public void notifyReset()
        {

        }

        public void notifyRun(int Time)
        {
            //do something for amount of given time then call freese
            //log("Run method: " + Time);
            lock (this)
            {
                if (CurrentMove != null)
                {
                    log("Run method: moving");
                    setJointsPos(CurrentMove.getPose());
                    if (CurrentMove.isFinished())
                    {
                        CurrentMove = null;
                    }
                }
            }
            //App.render();
            Executor.freeze();

        }
        public bool isProgramRunning(Ice.Current icecurrent = null)
        {
            if (CurrentMove != null)
            {
                return ! CurrentMove.isFinished();
            }
            return false;
        }

        public void notifyStartSimulation()
        {

        }

        public void notifyStopSimulation()
        {
 
        }

        public void notifyStringInputValueChange(int Index, string Value)
        {

        }

        public void queryBinaryIOConfiguration(ref int Inputs, ref int Outputs)
        {

        }

        public void queryIntegerIOConfiguration(ref int Inputs, ref int Outputs)
        {

        }

        public void queryRealIOConfiguration(ref int Inputs, ref int Outputs)
        {

        }

        public void queryStringIOConfiguration(ref int Inputs, ref int Outputs)
        {
 
        }



        public void setDigitalOut(int nb, bool val, Ice.Current current__=null)
        {
            Executor.setBinaryOutput(0, nb, val);
            //IvcSignal sig =   DigitalOutput.getPortSignal(nb);
            //sig.setProperty("Value", val);
        }

        public void setAnalogOut(int nb, bool val, Ice.Current current__=null)
        {
            throw new NotImplementedException();
        }

        public bool getDigitalInput(int nb, Ice.Current current__=null)
        {
            IvcSignal sig = DigitalOutput.getPortSignal(nb);
            return sig.getProperty("Value");
        }

        public bool getAnalogInput(int nb, Ice.Current current__=null)
        {
            throw new NotImplementedException();
        }

        public void setTool(int tool, Ice.Current current__=null)
        {
            throw new NotImplementedException();
        }

        public void setTCP(double[] tcp, Ice.Current current__=null)
        {
            throw new NotImplementedException();
        }

        public void grasp(Ice.Current current__)
        {
            setDigitalOut(1, true);
        }

        public void release(Ice.Current current__)
        {
            setDigitalOut(1, false);
        }


    }

}
