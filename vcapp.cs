using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;

namespace vc2ice
{


    public interface VCClient
    {
        void VCUpdate();

    }

    public class VCApp: IvcClient
    {
        private IvcApplication m_application;
        public List<VCRobot> Robots;
        public List<VCHolon> Machines;
        private List<VCClient> m_clients;
        private icehms.IceApp IceApp;


        public VCApp(icehms.IceApp app)
        {
            IceApp = app;
            Robots = new List<VCRobot>();
            Machines = new List<VCHolon>();
            m_clients = new List<VCClient>();
            m_application = (IvcApplication)new vc3DCreate.vcc3DCreate();

            IvcClient client = (IvcClient)this;
            m_application.addClient(ref client);  
        }

        public void register(VCClient client)
        {
            m_clients.Add(client);
        }

        public void cleanup()
        {
            Robots.Clear();
            foreach (VCHolon holon in Machines)
            {
                holon.shutdown();
            }
            Machines.Clear();
        }

        public void updateDevicesList()
        {

            cleanup();

            for (int i = 0; i < m_application.ComponentCount; i++)
            {
                IvcComponent comp = m_application.getComponent(i);
                string cname = (string)comp.getProperty("Name");
                Console.WriteLine("Studying:    " + cname);
                // find robots
                object[] result = comp.findBehavioursOfType("RobotController");
                for (int j = 0; j < result.Length; j++)
                {
                    Console.WriteLine(cname + " is a robot!");
                    VCRobot rob = new VCRobot(m_application, IceApp, comp);
                    Robots.Add(rob);

                }
                // Find machines
                result = comp.findBehavioursOfType("ComponentSignal");
                if (result.Length > 0)
                {
                    Machines.Add(new VCHolon(IceApp, comp));


                }
            }


        }
    


           
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
            Console.WriteLine("Warning clients of VC layout update");

            foreach (VCClient client in m_clients)
            {

                client.VCUpdate();

            }


        }

        public bool queryContextMenu()
        {
            return true;
        }

        #endregion


    }
}
