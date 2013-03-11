﻿/*
    Copyright 2013 Olivier Roulet-Dubonnet
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */



using System;
using System.Collections.Generic;
using vcCOM;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VC2HMS
{
    public class VCAppHolon : VCObject, hms.SimulationOperations_
    {
        private VCManager VCMgr;


        public VCAppHolon(VCManager vcapp, icehms.IceManager iceapp, IvcPropertyList2 plist)
            : base(vcapp, plist, "Simulation")
        {
            //we called base with activate=false so we need to create our own "tie servant"
            register((Ice.Object)new hms.SimulationTie_(this));
            this.VCMgr = vcapp;
        }

        public void start(Ice.Current current__)
        {
            VCMgr.start_simulation();
        }

        public void stop(Ice.Current current__)
        {
            VCMgr.stop_simulation();
        }

        public void reset(Ice.Current current__)
        {
            VCMgr.reset_simulation();
        }
        public bool isSimulationRunning(Ice.Current current__)
        {
            return VCMgr.isSimulationRunning();
        }

    }

    public class VCManager : IvcClient2
    {
        public IvcApplication IvcApp { get; set; }
        private List<VCComponent> Components;
        public icehms.IceManager IceMgr { get; set; }
        private VCAppHolon Holon;
        log4net.ILog logger;


        public VCManager(icehms.IceManager app)
        {
            IceMgr = app;
            logger = log4net.LogManager.GetLogger(this.GetType().Name);
            Components = new List<VCComponent>();
            IvcApp = (IvcApplication)new vc3DCreate.vcc3DCreate();
            Holon = new VCAppHolon(this, app, (IvcPropertyList2)IvcApp);

            //print_commands();
        }

        public void start()
        {
            IvcApp.addClient(this);
            createCurrentComponents();
        }

        private void print_commands()
        {
            Console.WriteLine("Available IvcApp commands are: ");
            for (int i = 0; i < IvcApp.CommandCount; i++)
            {
                Console.WriteLine(IvcApp.getCommandName(i));
            }
            Console.WriteLine();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void shutdown()
        {
            IvcApp.removeClient(this);

            foreach (VCComponent comp in Components)
            {
                comp.shutdown();
            }
            Holon.shutdown();
        }


        public void start_simulation()
        {
            IvcApp.setProperty("SimulationRunning", true);
        }

        public void stop_simulation()
        {
            IvcApp.setProperty("SimulationRunning", false);
        }

        public void reset_simulation()
        {
            IvcCommand l_restart = IvcApp.getCommand("resetSimulation");
            l_restart.start();
        }

        public bool isSimulationRunning()
        {
            return IvcApp.getProperty("SimulationRunning");
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
            for (int i = 0; i < IvcApp.ComponentCount; i++)
            {
                IvcComponent comp = IvcApp.getComponent(i);
                addComponent(comp, (string)comp.getProperty("Name"));
            }
            Console.WriteLine(String.Format("VC2HMS: {0} Component Holons created at Startup", IvcApp.ComponentCount));
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



        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool addComponent(IvcComponent comp, string name)
        {
            logger.Info(String.Format("Adding component {0}", name));
            if (!isCreated(name))
            {
                VCComponent mycomp;
                if (VCRobot.isRobot(comp))
                {
                    VCRobot rob = new VCRobot(this, comp, name);
                    mycomp = (VCComponent)rob;
                }
                else
                {
                    mycomp = new VCComponent(this, comp, name);
                }
                Components.Add(mycomp);
                return true;
            }
            else
            {
                logger.Info(String.Format("Component {0} was allready created", name));
                return false;
            }

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
                removeComponent(name);
            }

        }
        private void removeComponent(string name)
        {
            logger.Info(String.Format("Removing component {0}", name));
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
            cname = cname + "-" + sessionId.ToString();
            //cname = "Dynamic-" + cname +  sessionId.ToString();
            if (Added)
            {
                addComponent(comp, cname);
            }
            else
                removeComponent(cname);
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