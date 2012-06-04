using System;
using System.Collections.Generic;
using vcCOM;

namespace vc2ice
{
    public class VCRobot : VCMachine  , hms.RobotMotionCommandOperations_
    {
        private IvcRobot Controller;
        private List<IvcEventProperty> Joints;
        private IvcApplication m_application;
        //private hms.RobotMotionCommandTie_ RobotServant;
        //public override hms.HolonTie Servant { get; set; }

        public VCRobot(IvcApplication vc, icehms.IceApp app, IvcComponent robot)  : base (app, robot, false)
        {

            //we called base with activate=false wo we need to create our own "tie servant"
            register((Ice.Object)new hms.RobotMotionCommandTie_(this));

            m_application = vc;
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
                list.Add(j.Value);
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
                Console.WriteLine("Moving joint: " + i.ToString() + " to " + pose[i].ToString());
                IvcEventProperty j = Joints[i];
                j.Value = pose[i];
            }

        }

        public void movej(double[] pose, double speed = 2, double acc=1, Ice.Current icecurrent=null)
        {
            Console.WriteLine("Starting move"); 
            IvcMotionInterpolator motion = Controller.createMotionInterpolator();
            IvcMotionTarget target = Controller.createTarget();

            target.TargetMode = 1; // robot base as reference
            target.MotionType = 1; // Linear
            double[] current = target.RobotRootToRobotFlangeMatrix;
            //adding start point to trajectory
            target.TargetMatrix = current;
            motion.addTarget(ref target);
            target.CurrentConfig = target.NearestConfig;
            
            //Now the real target
            for (int i=0; i < pose.Length; i++)
            {
                target.setJointValue(i, pose[i]);
            }

            motion.addTarget(ref  target);


            Console.WriteLine("Is target reachable: " + target.getConfigWarning(0));
            Console.WriteLine("Is target reachable 1: " + target.getConfigWarning(motion.TargetCount - 1));
            Console.WriteLine(target.CurrentConfig);
            Console.WriteLine(target.NearestConfig);
            double endTime = motion.getCycleTimeAtTarget(motion.TargetCount - 1);
            Console.WriteLine("Computed time: " + Convert.ToString( endTime));
            Console.WriteLine("Starting move");
            printMatrix("t 1  ", motion.getTarget(0).TargetMatrix);
            printMatrix("t 2  ", motion.getTarget(1).TargetMatrix);
            double[] bjoints = new double[6];
            for (double time=0; time <= endTime; time += 0.01)
            {
                motion.interpolate(time, ref target);
                //.WriteLine("Config: " + target.CurrentConfig);
                printMatrix("Current pose: ", target.TargetMatrix);
                //Console.WriteLine(target.getConfigWarning(target.CurrentConfig));
                for (int i = 0; i < target.RobotJointCount; i++)
                {
                    bjoints[i] = target.getJointValue(i);
                }
                setJointsPos(bjoints);
            }
            // make sure we reach the final destination, even after approximation
            for (int i = 0; i < target.RobotJointCount; i++)
            {
                bjoints[i] = target.getJointValue(i);
            }
            setJointsPos(bjoints);
            Console.WriteLine("Target reached");

        }

        public double[] getl(hms.CSYS cref, Ice.Current current = null)
        {
            IvcMotionInterpolator motion = Controller.createMotionInterpolator();
            IvcMotionTarget target = Controller.createTarget();
            double[] matrix;
            switch (cref)
            {
                case hms.CSYS.Base:
                    return target.RobotRootToRobotFlangeMatrix;
                case hms.CSYS.World:
                    matrix = Helpers.AddMatrix(target.WorldToRootNodeMatrix, target.RootNodeToRobotRootMatrix );
                    matrix = Helpers.AddMatrix(matrix, target.RobotRootToRobotFlangeMatrix );
                    return matrix;
                default:
                    goto case hms.CSYS.World;
                    
            }


        }


        public void movel(double[] pose, double speed = 2, double acc = 1, hms.CSYS cref = hms.CSYS.World, Ice.Current icecurrent = null)
        {
            Console.WriteLine("Starting move");
            IvcMotionInterpolator motion = Controller.createMotionInterpolator();
            IvcMotionTarget target = Controller.createTarget();
          
            target.MotionType = 0; // 1 is Linear, 0 joint
            switch (cref)
            {
                case hms.CSYS.Base:
                    target.TargetMode = 1; // robot base as reference
                    break;
                case hms.CSYS.World:
                    target.TargetMode = 4; //  world as reference
                    break;
                default:
                    goto case hms.CSYS.World;
                    
            }
            double[] current = target.RobotRootToRobotFlangeMatrix;

            //adding start point to trajectory
            target.TargetMatrix = current;
            motion.addTarget(ref target);
            target.CurrentConfig = target.NearestConfig;

            //Now the real target
            target.TargetMatrix = pose;
            motion.addTarget(ref target);
            printMatrix("Setting target to : ", current);

            Console.WriteLine("Is target reachable 1: " + target.getConfigWarning(motion.TargetCount - 1));

            double endTime = motion.getCycleTimeAtTarget(motion.TargetCount - 1);
            Console.WriteLine("Computed time: " + Convert.ToString(endTime));
            Console.WriteLine("Starting move");
            double[] bjoints = new double[6];
            double movestart = m_application.getProperty("SimTime");
            double moveend = movestart + endTime;
            double now = movestart;
            double relnow;
            while (now <= moveend)
            {
                now = m_application.getProperty("SimTime");
                relnow = now - movestart;                
                motion.interpolate(relnow, ref target);
                for (int i = 0; i < target.RobotJointCount; i++)
                {
                    bjoints[i] = target.getJointValue(i);
                }
                setJointsPos(bjoints);
            }
            // make sure we reach the final destination, even after approximation
            for (int i = 0; i < target.RobotJointCount; i++)
            {
                bjoints[i] = target.getJointValue(i);
            }
            setJointsPos(bjoints);
            Console.WriteLine("Target reached");
        }








        public void printMatrix(string header, double[] a)
        {
            Console.Write(header + ": {");
             for (int i = 0; i < a.Length; i++) { Console.Write(a[i] + "  "); }
            Console.WriteLine("}");
            
        }

        public void move()
        {
            double i = 0.0;
            while (true)
            {

                Joints[0].Value = Math.Sin(i) * 90;
                i += 0.1;
                //fm.m_application.render();

            }
        }
    }

}
