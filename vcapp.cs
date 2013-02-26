using System;
using System.Collections.Generic;
using vcCOM;
using System.Runtime.CompilerServices;
using System.Threading;

namespace vc2ice
{
    public class VCAppHolon : VCObject, hms.SimulationOperations_
    {
        public VCApp vcapp;

        public VCAppHolon(VCApp vcapp, icehms.IceApp iceapp)
            : base(iceapp, (IvcPropertyList2)vcapp.Application, "Simulation")
        {
            //we called base with activate=false so we need to create our own "tie servant"
            register((Ice.Object)new hms.SimulationTie_(this));
            this.vcapp = vcapp;
        }

        public void start(Ice.Current current__)
        {
            vcapp.start_simulation();
        }

        public void stop(Ice.Current current__)
        {
            vcapp.stop_simulation();
        }

        public void reset(Ice.Current current__)
        {
            vcapp.reset_simulation();
        }
        public bool isSimulationRunning(Ice.Current current__)
        {
            return vcapp.isSimulationRunning();
        }

    }

    public class VCApp : IvcClient2
    {
        public IvcApplication Application;
        public List<VCComponent> Components;
        private icehms.IceApp IceApp;
        private VCAppHolon Holon;


        public VCApp(icehms.IceApp app)
        {
            IceApp = app;
            Components = new List<VCComponent>();
            Application = (IvcApplication)new vc3DCreate.vcc3DCreate();
            Holon = new VCAppHolon(this, app);
            Application.addClient(this);
            createCurrentComponents();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void shutdown()
        {
            Application.removeClient(this);

                foreach (VCComponent comp in Components)
                {
                    comp.shutdown();
                }
            Holon.shutdown();
        }


        public void start_simulation()
        {
            Application.setProperty("SimulationRunning", true);
        }

        public void stop_simulation()
        {
            Application.setProperty("SimulationRunning", false);
        }

        public void reset_simulation()
        {
            IvcCommand l_restart = Application.getCommand("resetSimulation");
            l_restart.start();
        }

        public bool isSimulationRunning()
        {
            return Application.getProperty("SimulationRunning");
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
                    if (holon.get_name() == name)
                    {
                        return true;
                    }
                }
                return false;


        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void createCurrentComponents()
        {
            for (int i = 0; i < Application.ComponentCount; i++)
            {
                IvcComponent comp = Application.getComponent(i);
                addComponent(comp, (string)comp.getProperty("Name"));
            }
        }



        #region IvcClient Members

        public string ApplicationName
        {
            get { return "VC2IceHMS"; }
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


        private void createComponent(IvcComponent comp, string name)
        {


        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool addComponent(IvcComponent comp, string name)
        {
            if (!isCreated(name))
            {
                VCComponent mycomp;
                if (isRobot(comp))
                {
                    VCRobot rob = new VCRobot(Application, IceApp, comp, name);
                    mycomp = (VCComponent)rob;
                }
                else
                {
                    mycomp = new VCComponent(IceApp, comp, name);
                }
                Components.Add(mycomp);
                return true;
            }
            else { return false; }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void notifyWorld(ref IvcComponent comp, bool Added)
        {
            string name = (string)comp.getProperty("Name");
            //Console.WriteLine("NotifyWorld says:    " + name + Added);

            if (Added)
            {
                addComponent(comp, name);
            }
            else
            {
                removeComponent(comp, name);
            }

        }
        private void removeComponent(IvcComponent comp, string name)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                VCComponent holon = Components[i];
                if (holon.get_name() == name)
                {
                    holon.shutdown();
                    Components.Remove(holon);
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void notifyDynamicComponent(ref IvcComponent comp, ref IvcBehaviour Container, bool Added)
        {
            string cname = (string)comp.getProperty("Name");
            //Console.WriteLine("Dynamic component: " + cname + Added);
            long sessionId = (long)comp.getProperty("SessionID");
            cname = cname + sessionId.ToString();
            if (Added)
            {
                addComponent(comp, cname);
            }
            else
                removeComponent(comp, cname);
            {
            }
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
