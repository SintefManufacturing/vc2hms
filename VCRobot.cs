using System;
using System.Collections.Generic;
using vcCOM;

namespace vc2ice
{
    public class VCRobot
    {
        public IvcComponent m_component;
        public string m_name;
        private IvcRobot m_controller;
        private List<IvcEventProperty> m_joints;

        public VCRobot(IvcComponent robot)
        {
            m_component = robot;
            m_name = (string)m_component.getProperty("Name");
            // find the controller
            for (int i = 0; i < robot.RootNode.BehaviourCount; ++i)
            {
                IvcBehaviour beh = m_component.RootNode.getBehaviour(i);
                string btype = (string)beh.getProperty("Type");
                if ( btype == "RobotController")
                {
                    m_controller = (IvcRobot)beh;
                    //populate the joints list
                    IvcPropertyList2 compprops = (IvcPropertyList2)m_component;
                    m_joints = new List<IvcEventProperty>();
                    for (int k = 0; k < m_controller.JointCount; k++)
                    {
                        string jointname = (string)m_controller.getJoint(k).getProperty("Name");
                        IvcEventProperty joint = (IvcEventProperty)compprops.getPropertyObject(jointname);
                        m_joints.Add(joint);
                    }
                    //we are finished
                    break;
                }
            }
            

        }

        public string getName()
        {
            return m_name;
        }

        public List<string> getJoints()
        {
            List<string> list = new List<string>();
            foreach (IvcEventProperty j in m_joints)
            {
                list.Add(j.getProperty("Name"));
            }
            return list;

        }

        public List<double> getj()
        {
            List<double> list = new List<double>();
            foreach (IvcEventProperty j in m_joints)
            {
                list.Add(j.Value);
            }
            
            return list;
        }
        public void setJointVal(int idx, double val)
        {
            m_joints[idx].Value = val;
        }
        public double getJointVal(int idx)
        {
            return m_joints[idx].Value;
        }

        public void setJointsPos(double[] pose)
        {
            for(int i=0; i < m_joints.Count; i++)
            {
                Console.WriteLine("Moving joint: " + i.ToString() + " to " + pose[i].ToString());
                IvcEventProperty j = m_joints[i];
                j.Value = pose[i];
            }

        }

        public void movej(double[] pose, float speed=2)
        {
            Console.WriteLine("Stsrting move"); 
            IvcMotionInterpolator motion = m_controller.createMotionInterpolator();
            IvcMotionTarget target = m_controller.createTarget();

            target.TargetMode = 1; // robot base as reference
            target.MotionType = 1; // Linear
            //target.BaseMatrix = new double[] {1250, 0, -25, 0,0,0};
            //target.ToolMatrix = new double[] { 39, 198, 180, 0, 0, -90 };
            Console.WriteLine("Nb joints: " + target.RobotJointCount);
            Console.WriteLine("Nb bases: " + target.NamedBaseCount);
            printMatrix("Root Node position in World: ", target.WorldToRootNodeMatrix);         
            printMatrix("Robot position in Root Nove: ", target.RootNodeToRobotRootMatrix);
            printMatrix("Flange position n Robot Root: ", target.RobotRootToRobotFlangeMatrix);
            printMatrix("Positioner Root to Positioner Flange: ", target.PositionerRootToPositionerFlangeMatrix);
            Console.WriteLine("Number of possible coinfigurations: " + target.ConfigCount);
            Console.WriteLine("Accuracy Value: " + target.AccuracyValue);
            double[] current = target.RobotRootToRobotFlangeMatrix;
            //adding start point to trajectory
            target.TargetMatrix = current;
            motion.addTarget(ref target);
            target.CurrentConfig = target.NearestConfig;
            
            //Now the real target

            current[2] -= 500;
            target.TargetMatrix = current;
            motion.addTarget(ref target);
            printMatrix("Setting target to : ", current);


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

                m_joints[0].Value = Math.Sin(i) * 90;
                i += 0.1;
                //fm.m_application.render();

            }
        }
    }

}
