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

        public class VCRobot
        {
            private Form1 fm;
            private ListBox lb;
            private IvcComponent rb;
            private int bc;
            private string robname;
            private IvcServo jointController;
            private List<IvcEventProperty> joints;

            public VCRobot(Form1 form, ListBox listBox, IvcComponent robot)
            {
                fm = form;
                lb = listBox;
                rb = (IvcComponent)robot;
                robname = (string)rb.getProperty("Name");
                bc = robot.RootNode.BehaviourCount;
                //lb.Items.Add(string.Format("Component name: {0}. {1} behaviour(s) found in component", robname, bc));
                jointController = null;

                try
                {
                    for (int i = 0; i < bc; ++i)
                    {
                        IvcBehaviour beh = rb.RootNode.getBehaviour(i);
                        string btype = (string)beh.getProperty("Type");
                        if (btype == "ServoController" || btype == "RobotController")
                        {
                            jointController = (IvcServo)beh;
                            break;
                        }
                    }
                    if (jointController != null)
                    {
                        IvcPropertyList2 compprops = (IvcPropertyList2)rb;
                        joints = new List<IvcEventProperty>();
                        for (int i = 0; i < jointController.JointCount; i++)
                        {
                            string jointname = (string)jointController.getJoint(i).getProperty("Name");
                            IvcEventProperty joint = (IvcEventProperty)compprops.getPropertyObject(jointname);
                            joints.Add(joint);
                        }
                    }
                }
                catch
                { 
                }
            }

            public string getName()
            {
                return robname;
            }

            public void getJoints()
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    lb.Items.Add(joints[i].getProperty("Name"));
                                    
                }
            }


            public void move()
            {
                double i = 0.0;
                while (true)
                {
                    
                    joints[0].Value = Math.Sin(i) * 90;
                    i += 0.1;
                    fm.m_application.render();
   
                }            
            }
        }

        private List<VCRobot> m_robots = new List<VCRobot>();

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
                VCRobot rob = new VCRobot(this, listBox1, comp);
                m_robots.Add(rob);
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
            //throw new NotImplementedException();
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

            //Invoke(new UIUpdate(driveRobot));

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
            return true;
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            VCRobot rob = m_robots[Convert.ToInt32(textBox1.Text)];
            listBox1.Items.Add(rob.getName());
            rob.getJoints();
            rob.move();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



    }


}




  