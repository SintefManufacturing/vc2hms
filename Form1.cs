using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using vcCOM;

namespace VCRobot
{
    public partial class Form1 : Form, IvcClient
    {
        protected IvcApplication m_application;
        protected IvcSelection m_component_selection;
        protected IvcComponent m_component;


        public Form1()
        {
            InitializeComponent();

            m_application = (IvcApplication)new vc3DCreate.vcc3DCreate();

            IvcClient client = (IvcClient)this;
            m_application.addClient(ref client);

            try
            {
                int cc = m_application.ComponentCount;
                initConnection();

            }
            catch
            {
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void initConnection()
        {
            m_component_selection = m_application.findSelection("Component");
            updateComponentList();
 
        }

        private void updateComponentList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < m_application.ComponentCount; i++)
            {
                IvcComponent comp = m_application.getComponent(i);
                string cname = (string)comp.getProperty("Name");
                listBox1.Items.Add(cname);
            }
        }

        private void driveRobot()
        {
            int itemCount = m_component_selection.ItemCount;
            if (itemCount == 0)
            {
                listBox2.Items.Clear();
                listBox2.Items.Add("No component selected");
            }
            else
            {
                listBox2.Items.Clear();
                m_component = (IvcComponent)m_component_selection.getItem(0);
                string robName = (string)m_component.getProperty("Name");
                listBox2.Items.Add(robName);
                try
                {
                    int bc = m_component.RootNode.BehaviourCount;
                    IvcRobot robot = null;
                    for (int i = 0; i < bc; ++i)
                    {
                        IvcBehaviour beh = m_component.RootNode.getBehaviour(i);
                        string btype = (string)beh.getProperty("Type");
                        listBox2.Items.Add(btype);
                        if (btype == "RobotController")
                        {
                            robot = (IvcRobot)beh;
                            listBox2.Items.Add(beh.ToString());
                            int jc = (int)robot.JointCount;
                            listBox2.Items.Add(jc);
                            break;
                        }

                    }
                    if (robot != null)
                    {
                        IvcMotionTester tester = (IvcMotionTester)robot.getMotionTester();
                        //listBox2.Items.Add(robot.ToString());
                        IvcMotionInterpolator motionPath = (IvcMotionInterpolator)robot.createMotionInterpolator();
                        IvcMotionTarget target = (IvcMotionTarget)robot.createTarget();

                        double[] position = new double[5];
                        position[0] = 1250;
                        position[1] = 0;
                        position[2] = -25;
                        position[3] = 0;
                        position[4] = 0;
                        position[5] = 0;

                        for (int i = 0; i < 5; i++)
                        {
                            listBox2.Items.Add(position[i]);
                        
                        }

                            //target.TargetMatrix;

                            //target.TargetMode = 4;
                            //target.TargetMatrix = position;

                            //tester.set_CurrentTarget(ref target);
                            //m_application.render();



                            target.BaseMatrix = position;
                        listBox2.Items.Add(position);

                        position[0] = 39;
                        position[1] = 198;
                        position[2] = 180;
                        position[3] = 0;
                        position[4] = 0;
                        position[5] = -90;

                        target.ToolMatrix = position;


                        double[] positionA = new double[5];
                        positionA[0] = 1000;
                        positionA[1] = 0;
                        positionA[2] = 1000;
                        positionA[3] = 0;
                        positionA[4] = 90;
                        positionA[5] = 0;

                        listBox2.Items.Add("Entered");

                        double[] positionB = new double[5];
                        positionA[0] = 1250;
                        positionA[1] = 0;
                        positionA[2] = 1500;
                        positionA[3] = 0;
                        positionA[4] = -90;
                        positionA[5] = 0;




                        target.MotionType = 1;
                        target.TargetMatrix = positionA;
                        target.CurrentConfig = target.NearestConfig;
                        motionPath.addTarget(ref target);
                        target.TargetMatrix = positionB;
                        motionPath.addTarget(ref target);

                        double endtime = (double)motionPath.getCycleTimeAtTarget(1);
                        listBox2.Items.Add(endtime);



                        for (double time = 0.0; time < endtime; time += 0.1)
                        {
                            motionPath.interpolate(time, ref target);
                            if (target.getConfigWarning(target.CurrentConfig) > 0)
                            {
                                listBox2.Items.Clear();
                                listBox2.Items.Add("point unreachable");
                            }



                        }




                    
                    
                    }

                }
                catch
                { 
                }





                //IvcBehaviour beh = m_component.RootNode.getBehaviour(0)
                ////IvcRobot robot = m_component.findBehaviour("RobotController");
                //string robControllerName = (string)robot.getProperty("Name");
                //listBox2.Items.Add(robControllerName);

                
            }


        
        }





        private delegate void UIUpdate();


        #region IvcClient Members

        public string ApplicationName
        {
            get { throw new NotImplementedException(); }
        }

        public void notifyApplication(bool AppReady)
        {
            throw new NotImplementedException();
        }

        public void notifyCommand(ref IvcCommand command, int State)
        {
            // This method is called when a command is started or stopped.
        }

        public void notifyProgress(double Progress)
        {
            //throw new NotImplementedException();
        }

        public void notifySelection(ref IvcSelection Selection, int SelectionTypeChange)
        {
            // This method is when one of the following happens:
            // - The currently active selection type changes. In this case 
            //   SelectionTypeChange == 1, and Selection is the new currently active 
            //   selection type.
            // - Objects are selected or unselected. In this case 
            //   SelectionTypeChange == 0, and Selection is the selection type where
            //   the contents changed. Usually, this is the currently active selection
            //   type, but if the selection was changed programmatically, it can be
            //   another selection type.

            Invoke(new UIUpdate(driveRobot));


            

        }

        public void notifySimHeartbeat(double SimTime)
        {
            throw new NotImplementedException();
        }

        public void notifySimulation(int State)
        {
            // when simulation state changes
        }

        public void notifyWorld(ref IvcComponent Component, bool Added)
        {


            Invoke(new UIUpdate(updateComponentList));



        }

        public bool queryContextMenu()
        {
            throw new NotImplementedException();
        }

        #endregion

    }


}




  