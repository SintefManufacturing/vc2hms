using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using System.Diagnostics;


namespace vc2ice
{
    public partial class Form1 : Form, VCClient
    {

        private icehms.IceApp m_iceapp;
        private VCApp m_vcapp;
        private VCRobot m_rob;

        public Form1(icehms.IceApp iceapp, VCApp vcapp)
        {
            InitializeComponent(); // MFC component, Windows stuff

            m_iceapp = iceapp;
            m_vcapp = vcapp;
            vcapp.register(this); //get  events from VC

            updateComponentList();     
        }

        private void updateComponentList()
        {
            Console.WriteLine("Updating component list");
            JointListBox.Items.Clear();
            RobotListBox.Items.Clear();
            MachineListBox.Items.Clear();

            trackBar1.Enabled = false;
            m_vcapp.updateDevicesList();

            foreach (VCRobot rob in m_vcapp.Robots)
            {
                RobotListBox.Items.Add(rob.getName());

            }
            foreach (VCHolon holon in m_vcapp.Machines)
            {
                MachineListBox.Items.Add(holon.Name);
            }
        }

        public void VCUpdate()
        {
            Invoke(new UIUpdate(updateComponentList));
        }

        private delegate void UIUpdate();





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
           
            m_rob = m_vcapp.Robots[RobotListBox.SelectedIndex];
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

        private void IceGridRegisterbutton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Not implemented yet");
        }



    }


}




  