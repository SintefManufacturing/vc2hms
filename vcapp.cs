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

    public class VCAppHolon : VCObject, hms.SimulationOperations_
    {
        public IvcApplication Application;

        //public VCAppHolon(VCApp vcapp, icehms.IceApp iceapp)  :  base(iceapp, (string)vcapp.Application.getProperty("ApplicationName") , false)
        public VCAppHolon(IvcApplication vcapp, icehms.IceApp iceapp)
            : base(iceapp, (IvcPropertyList2) vcapp, "Simulation")
        {
            //we called base with activate=false so we need to create our own "tie servant"
            register((Ice.Object)new hms.SimulationTie_(this));
            Application = vcapp;
        }

        public void start(Ice.Current current__)
        {
            Application.setProperty("SimulationRunning", true);
        }

        public void stop(Ice.Current current__)
        {
             Application.setProperty("SimulationRunning", false);
        }

        public void reset(Ice.Current current__)
        {
            IvcCommand l_restart = Application.getCommand("resetSimulation");
            l_restart.start();
        }

    }

    public class VCApp: IvcClient  
    {
        public IvcApplication Application;
        public List<VCRobot> Robots;
        public List<VCComponent> Components;
        private List<VCClient> m_clients;
        private icehms.IceApp IceApp;
        private VCAppHolon Holon;


        public VCApp(icehms.IceApp app)  
        {
            IceApp = app;
            Robots = new List<VCRobot>();
            Components = new List<VCComponent>();
            m_clients = new List<VCClient>();
            Application = (IvcApplication)new vc3DCreate.vcc3DCreate();
            Holon = new VCAppHolon(Application, app);

            IvcClient client = (IvcClient)this;
            Application.addClient(ref client);  
        }

        public void register(VCClient client)
        {
            m_clients.Add(client);
        }

        private void reset()
        {
            foreach (VCComponent holon in Components)
            {
                holon.shutdown();
            }
            foreach (VCRobot holon in Robots)
            {
                holon.shutdown();
            }
            Components.Clear();
            Robots.Clear();
        }

        public void shutdown()
        {        

            Holon.shutdown();
        }

        public void updateDevicesList()
        {
            reset();

            for (int i = 0; i < Application.ComponentCount; i++)
            {
                IvcComponent comp = Application.getComponent(i);
                string cname = (string)comp.getProperty("Name");
                Console.WriteLine("Studying:    " + cname);
                // find robots
                object[] result = comp.findBehavioursOfType("RobotController");
                bool registered = false;
                for (int j = 0; j < result.Length; j++)
                {
                    Console.WriteLine(cname + " is a robot!");
                    VCRobot rob = new VCRobot(Application, IceApp, comp);
                    Robots.Add(rob);
                    //double[] q = rob.getj();
                    //q[0] += 100;
                    //rob.movej(q, 2, 2);
                    //rob.setDigitalOut(1, true);
                    registered = true;

                }
                if (! registered )
                {
                // Find machines
                result = comp.findBehavioursOfType("ComponentSignal");
                if (result.Length > 0)
                {
                    Components.Add(new VCComponent(IceApp, comp));
                    registered = true;
                }
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
