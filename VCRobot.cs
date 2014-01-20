/*
    Copyright 2013 Olivier Roulet-Dubonnet
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */



using System;
using System.Threading;
using System.Collections.Generic;
using vcCOM;
using vcCOMExecutor;
using vcCOMsimulate;

namespace VC2HMS
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

    [Serializable]
    public class NotRobotException : Exception
    {
        public string ErrorMessage
        {
            get
            {
                return base.Message.ToString();
            }
        }

        public NotRobotException(string errorMessage)
            : base(errorMessage) { }

        public NotRobotException(string errorMessage, Exception innerEx)
            : base(errorMessage, innerEx) { }
    }


    public enum MoveType { Linear, Joint };


    public class Move
    {
        IvcMotionInterpolator Motion;
        IvcMotionTarget Target;
        private VCRobot Robot;
        private IvcApplication ivc;
        log4net.ILog logger;

        double StartTime;
        double EndTime;

        public Move(IvcApplication ivc, VCRobot robot, MoveType move, double[] pose, double vel, double acc)
        {
            this.ivc = ivc;
            Robot = robot;
            logger = log4net.LogManager.GetLogger(robot.get_name() + "::" + this.GetType().Name);
            Motion = Robot.Controller.createMotionInterpolator();
            Target = Robot.Controller.createTarget();
            Target.JointSpeed = 100; // % of max specified speed


            if (move == MoveType.Linear)
            {
                setupLinearMove(pose, vel);
            }
            else
            {
                setupJointMove(pose, vel);
            }


            if (Target.getConfigWarning(Motion.TargetCount - 1) == 1)
            {
                throw (new UnreachableException("Target is not reachable"));
            }

            double end = Motion.getCycleTimeAtTarget(Motion.TargetCount - 1);
            //Console.WriteLine("Starting move");

            StartTime = ivc.getProperty("SimTime");
            EndTime = StartTime + end;
        }

        public void setupLinearMove(double[] pose, double vel)
        {
            logger.Info("Starting linear move to: " + Helpers.strMatrix(pose) + " with velocity: " + vel.ToString() + "m/s");
            Target.MotionType = 1; // 1 is Linear, 0 joint
            Target.CartesianSpeed = vel;

            if (Robot.currentCSYS == hms.CSYS.World)
            {
                Target.TargetMode = 4;
            }
            else if (Robot.currentCSYS == hms.CSYS.Base)
            {
                Target.TargetMode = 1;
            }
            else
            {
                throw (new NotImplementedException(string.Format("Support for % not implemented yet", Robot.currentCSYS.ToString())));

            }

            //adding start point to trajectory
            Target.TargetMatrix = Robot.getl_mm();
            Motion.addTarget(ref Target);
            Target.CurrentConfig = Target.NearestConfig;

            //Now the real target
            Target.TargetMatrix = pose;
            Motion.addTarget(ref Target);
        }

        public void setupJointMove(double[] pose, double vel)
        {
            logger.Info("Starting joint move to: " + Helpers.strMatrix(pose) + " with velocity: " + vel.ToString() + "degree/s");
            Target.MotionType = 0; // 1 is Linear, 0 joint
            Target.TargetMode = 1; // robot base as reference
            Target.AngularSpeed = vel;
            Target.JointSpeed = vel; // this should be % of max specified speed but something is broken

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
            Motion.addTarget(ref Target);
        }


        public bool isFinished()
        {
            double t = ivc.getProperty("SimTime");
            if (t > EndTime)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public double[] getPose()
        {
            double[] bjoints = new double[Robot.Controller.JointCount];
            double now = ivc.getProperty("SimTime");
            if (now > EndTime)
            {
                now = EndTime;
            }
            now = now - StartTime;
            Motion.interpolate(now, ref Target);
            bjoints = ((IvcMotionTarget4)Target ).GetAllJointValues();
            /*
            for (int i = 0; i < Target.RobotJointCount; i++)
            {
                bjoints[i] = Target.getJointValue(i);
            }
             * */
            //Helpers.printMatrix("Interpolated joint pose is: ", bjoints);
            return bjoints;
        }
    }



    public class VCRobot : VCComponent, hms.RobotOperations_, IvcExecutorClient
    {
        public IvcRobot Controller{get; set;}
        private List<IvcEventProperty> Joints;
        private IvcApplication ivc;
        //private hms.RobotMotionCommandTie_ RobotServant;
        //public override hms.HolonTie Servant { get; set; }
        private IvcExecutor Executor;
        private Move CurrentMove;
        private IvcSignalMap DigitalInput;
        private IvcSignalMap DigitalOutput;
        public hms.CSYS currentCSYS { get; set; }


        public VCRobot(VCManager vcapp, IvcComponent robot, string name)
            : base(vcapp, robot, name, false)
        {
            //we called base with activate=false so we need to create our own "tie servant"
            register((Ice.Object)new hms.RobotTie_(this));
            currentCSYS = hms.CSYS.World;
            ivc = vcapp.IvcApp;
            object[] result = Component.findBehavioursOfType("RobotController");
            if (result.Length == 0)
            {
                throw new NotRobotException(name + " does  not contain a RobotController behaviour");
            }
            else
            {
                Controller = (IvcRobot)result[0];
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
            Executor = (IvcExecutor)result[0];
            Executor.addExecutorClient(this);
            // IvcRslExecutor rslex =  (IvcRslExecutor) Executor;
            ((IvcPropertyList)Executor).setProperty("ExecutionMode", true);  //we are ready

            //testing
            string pname = ((IvcPropertyList)Executor).getProperty("DigitalMapIn");
            DigitalInput = (IvcSignalMap)Component.findBehaviour(pname);
            pname = ((IvcPropertyList)Executor).getProperty("DigitalMapOut");
            DigitalOutput = (IvcSignalMap)Component.findBehaviour(pname);
        }

        public static bool isRobot(IvcComponent comp)
        {
            object[] result = comp.findBehavioursOfType("RobotController");
            if (result.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void shutdown()
        {
            ((IvcPropertyList)Executor).setProperty("ExecutionMode", false);  //we are ready
            Executor.removeExecutorClient(this);
            base.shutdown();
        }


        public void set_csys(hms.CSYS csys, Ice.Current current = null)
        {
            currentCSYS = csys;
        }

        public string[] getJoints()
        {
            List<string> list = new List<string>();
            foreach (IvcEventProperty j in Joints)
            {
                list.Add(j.getProperty("Name"));
            }
            return list.ToArray();
        }

        public double[] getj(Ice.Current current = null)
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
            for (int i = 0; i < Joints.Count; i++)
            {
                //Console.WriteLine("Moving joint: " + i.ToString() + " to " + pose[i].ToString());
                IvcEventProperty j = Joints[i];
                j.Value = pose[i];
            }

        }

        public void movej(double[] pose, double acc = 0.1, double vel = 0.05, Ice.Current icecurrent = null)
        {
            for (int i = 0; i < pose.Length; i++)
            {
                pose[i] = pose[i] * 180 / Math.PI;
            }
            //Helpers.printMatrix("New joint move command", pose);
            lock (this)
            {
                CurrentMove = new Move(ivc, this, MoveType.Joint, pose, vel * 180 / Math.PI, acc * 180 / Math.PI);
            }
            while (is_program_running() == true && !isShutdown())
            {
                Thread.Sleep(50);
            }
        }

        public void translate(double[] pos, double acc, double vel, Ice.Current current = null)
        {
            double[] pose = getl();
            pose[0] = pos[0];
            pose[1] = pos[1];
            pose[2] = pos[2];
            movel(pose, acc, vel);
        }

        public void orient(double[] orientation, double acc, double vel, Ice.Current current = null)
        {
            double[] pose = getl();
            pose[3] = orientation[0];
            pose[4] = orientation[1];
            pose[5] = orientation[2];
            movel(pose, acc, vel);
        }

        public double[] getl_mm(IvcMotionTarget target = null)
        {
            if (target == null) { target = Controller.createTarget(); }
            double[] matrix;
            switch (currentCSYS)
            {
                case hms.CSYS.Base:
                    matrix = target.RobotRootToRobotFlangeMatrix.Clone();
                    return matrix;

                case hms.CSYS.World:
                    matrix = Helpers.AddMatrix(target.WorldToRootNodeMatrix, target.RootNodeToRobotRootMatrix);
                    matrix = Helpers.AddMatrix(matrix, target.RobotRootToRobotFlangeMatrix);
                    return matrix;
                default:
                    goto case hms.CSYS.World;
            }
        }

        public double[] getl(Ice.Current current = null)
        {
            double[] matrix = getl_mm();
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = matrix[i] / 1000;
            }
            return matrix;
        }

        public void set_csys(hms.CSYS cref)
        {
            currentCSYS = cref;
        }

        public void movel(double[] pose, double acc = 0.01, double vel = 0.01, Ice.Current icecurrent = null)
        {
            //log("New move command: " + Helpers.strMatrix(pose));
            for (int i = 0; i < pose.Length; i++)
            {
                pose[i] = pose[i] * 1000;
            }
            lock (this)
            {
                CurrentMove = new Move(ivc, this, MoveType.Linear, pose, vel * 1000, acc * 1000);
            }
            while (is_program_running() == true && !isShutdown())
            {
                Thread.Sleep(10);
            }
        }

        public void notifyBinaryInputValueChange(int Index, bool Value)
        {
            logger.Debug("Binary input: " + Index + " has been changed to: " + Value);
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
        public bool is_program_running(Ice.Current icecurrent = null)
        {
            if (CurrentMove != null)
            {
                return !CurrentMove.isFinished();
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



        public void set_digital_out(int nb, bool val, Ice.Current current__ = null)
        {
            logger.Info(String.Format("Set digital out {0} to {1}", nb, val));
            Executor.setBinaryOutput(0, nb, val);
            //IvcSignal sig =   DigitalOutput.getPortSignal(nb);
            //sig.setProperty("Value", val);
        }

        public void set_analog_out(int nb, bool val, Ice.Current current__ = null)
        {
            throw new NotImplementedException();
        }

        public bool get_digital_input(int nb, Ice.Current current__ = null)
        {
            IvcSignal sig = DigitalOutput.getPortSignal(nb);
            return sig.getProperty("Value");
        }

        public bool get_analog_input(int nb, Ice.Current current__ = null)
        {
            throw new NotImplementedException();
        }

        public void set_tool(int tool, Ice.Current current__ = null)
        {
            throw new NotImplementedException();
        }

        public void set_tcp(double[] tcp, Ice.Current current__ = null)
        {
            throw new NotImplementedException();
        }

        public void grasp(Ice.Current current__)
        {
            set_digital_out(1, true);
        }

        public void release(Ice.Current current__)
        {
            set_digital_out(1, false);
        }


    }

}
