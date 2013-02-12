using System;
using System.Collections.Generic;
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
            : base(iceapp, (IvcPropertyList2)vcapp, "Simulation")
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

    public class VCApp : IvcClient2
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

        private bool isRobot(IvcComponent comp)
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
        private bool isCreated(string name)
        {
            foreach (VCComponent holon in Components)
            {
                if (holon.getName() == name)
                {
                    return true;
                }
            }
            foreach (VCRobot holon in Robots)
            {
                if (holon.getName() == name)
                {
                    return true;
                }
            }
            return false;

        }

        public void createCurrentComponents()
        {
            for (int i = 0; i < Application.ComponentCount; i++)
            {
                IvcComponent comp = Application.getComponent(i);
                addComponent(comp);
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
            //throw new NotImplementedException();
        }

        public void notifySimulation(int State)
        {
            // when simulation state changes
        }


        private bool addComponent(IvcComponent comp)
        {
            string cname = (string)comp.getProperty("Name");
            Console.WriteLine("Addind:    " + cname);
            if (!isCreated(cname))
            {
                if (isRobot(comp))
                {
                    Console.WriteLine(cname + " is a robot!");
                    VCRobot rob = new VCRobot(Application, IceApp, comp);
                    Robots.Add(rob);
                }
                else
                {
                    object[] result = comp.findBehavioursOfType("ComponentSignal"); //create component for objects which have components signals
                    if (result.Length > 0)
                    {
                        Components.Add(new VCComponent(IceApp, comp));
                    }
                }
                return true;
            }
            else { return false; }

        }
        public void notifyWorld(ref IvcComponent comp, bool Added)
        {
            string name = (string)comp.getProperty("Name");
            Console.WriteLine("NotifyWorld says:    " + name + Added);

            if (Added)
            {
                addComponent(comp);
            }
            else
            {
                removeComponent(comp);
            }

        }
        private void removeComponent(IvcComponent comp)
        {
            string name = (string)comp.getProperty("Name");
            Console.WriteLine("Removing:    " + name);

            for (int i = 0; i < Components.Count; i++)
            {
                VCComponent holon = Components[i];
                if (holon.getName() == name)
                {
                    holon.shutdown();
                    Components.Remove(holon);
                    return;
                }
            }
            for (int i = 0; i < Robots.Count; i++)
            {
                VCRobot holon = Robots[i];
                if (holon.getName() == name)
                {
                    holon.shutdown();
                    Robots.Remove(holon);
                    return;
                }
            }
        }

        public bool queryContextMenu()
        {
            return true;
        }

        #endregion




        public void notifyContainer(ref IvcApplication Application, bool Added)
        {
            //throw new NotImplementedException();
        }

        public void notifyDynamicComponent(ref IvcComponent Component, ref IvcBehaviour Container, bool Added)
        {

            Console.WriteLine("Dynamic component added!!!!!!!!!!!!!!!!!: " + Added);

        }

        public void notifyExternalDragLeave()
        {
            //throw new NotImplementedException();
        }

        public int queryExternalDragDrop(ref object Data, int CursorX, int CursorY, int ShiftState)
        {
            return 0;
        }

        public int queryExternalDragEnter(ref object Data, int CursorX, int CursorY, int ShiftState)
        {
            return 0;
        }

        public int queryExternalDragOver(int CursorX, int CursorY, int ShiftState)
        {
            return 0;
        }
    }
}
