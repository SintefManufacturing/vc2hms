using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using vcCOM;
using System.Diagnostics;

namespace VCRobot
{
    public partial class Form1 : Form, IvcClient
    {
        protected IvcApplication m_application;
        private List<VCRobot> m_robots = new List<VCRobot>();
        private VCRobot m_rob;

        public Form1()
        {
            InitializeComponent();
            Debug.WriteLine(this.Name + "; starting");
            m_application = (IvcApplication)new vc3DCreate.vcc3DCreate();

            IvcClient client = (IvcClient)this;
            m_application.addClient(ref client);
            

            updateComponentList();                   
        }



        private void updateComponentList()
        {
            Debug.WriteLine("Updating component list");
            JointListBox.Items.Clear();
            RobotListBox.Items.Clear();
            m_robots.Clear();
            trackBar1.Enabled = false;

            for (int i = 0; i < m_application.ComponentCount; i++)
            {              
                IvcComponent comp = m_application.getComponent(i);
                string cname = (string)comp.getProperty("Name");
                Console.WriteLine("Studying:    " + cname);
                // for some strange reasons this doe snot work
                object[] result = comp.findBehavioursOfType("RobotController");
                for (int j = 0; j < result.Length; j++)
                {
                    Console.WriteLine(cname + " is a robot!");
                    VCRobot rob = new VCRobot(comp);
                    m_robots.Add(rob);
                    RobotListBox.Items.Add(rob.getName());
                }

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
            //Console.WriteLine("NotifyApplication: ", Convert.ToString(AppReady));
        }

        public void notifyCommand(ref IvcCommand command, int State)
        {
            // This method is called when a command is started or stopped.
            //Console.WriteLine("NotifyCommand: ", Convert.ToString(command));
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
            //Console.WriteLine("NotifySelction: ", Convert.ToString(Selection));

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





        private void Form1_Load(object sender, EventArgs e)
        {
        }




        private void JointListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = true;
            if (m_rob != null){
                trackBar1.Value = Convert.ToInt16(m_rob.getJointVal(JointListBox.SelectedIndex));
            }
        }

        private void RobotListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Selected: " + RobotListBox.SelectedItem.ToString());
           
            m_rob = m_robots[RobotListBox.SelectedIndex];
            updateJoints(m_rob);       
        }

        private void updateJoints(VCRobot rob)
        {
            trackBar1.Enabled = false;
            JointListBox.Items.Clear();
            foreach ( string j in rob.getJoints())
            {
                JointListBox.Items.Add(j);
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (m_rob == null) { return; }
            //Console.WriteLine(m_rob.getj().ToString());
            try
            {
                m_rob.movej(new double[6] { 914,  8,  1119,  0 , 90,  0});
                //m_rob.movej(new double[6] { 0, 0, 0, 0, 0, 0 });
            }
            catch
            {
            }
            //Console.WriteLine(m_rob.getj().ToString());
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Console.WriteLine(trackBar1.Value.ToString());
            if (m_rob != null && JointListBox.SelectedIndex != -1)
            {
                Console.WriteLine("Value: " + JointListBox.SelectedIndex);
                m_rob.setJointVal(JointListBox.SelectedIndex, Convert.ToDouble(trackBar1.Value)); 
            }
        }

    }


}




  