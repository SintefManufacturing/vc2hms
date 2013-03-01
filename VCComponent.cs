using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;
using System.Runtime.CompilerServices;
using System.Threading;


namespace VC2HMS
{
    public class VCObject : icehms.Holon
    {
        private IvcPropertyList2 PList;

        public VCObject(VCManager vcapp, IvcPropertyList2 plist, string name)
            : base(vcapp.IceMgr, name, false)
        {
            PList = plist;
        }

        public string getProperty(string name, Ice.Current current__)
        {
            return Convert.ToString(PList.getProperty(name));
        }

        public string[] getPropertyList(Ice.Current current__)
        {
            string[] list = new string[PList.PropertyCount];
            for (int i = 0; i < PList.PropertyCount; i++)
            {
                list[i] = PList.getPropertyName(i);
            }
            return list;
        }

        public void setProperty(string name, string val, Ice.Current current__)
        {
            //everything comes as string from Ice so we must convert it to correct type
            IvcProperty prop = PList.getPropertyObject(name);
            //Type tp = prop.GetType();

            //       logger.Error("SETPROP: " + prop.getProperty("Type"));
            //logger.Error("SETPROP: " + tp + prop + tp.ToString() + tp.MakeGenericType() ) ;
            //prop.Value =  Convert.ChangeType(val, (Type) prop.getProperty("Type")  );
            string stype = prop.getProperty("Type");
            switch (stype)
            {
                case "Real":
                    prop.Value = Convert.ToDouble(val);
                    break;
                case "Integer":
                    prop.Value = Convert.ToInt64(val);
                    break;
                case "String":
                    prop.Value = val;
                    break;
                default:
                    logger.Error("Uknown format for property: " + name + " and value: " + val + " of type: " + stype);
                    break;
            }

        }




    }


    public class VCBehaviour : VCObject, hms.BehaviourOperations_
    {
        private IvcBehaviour Behaviour;

        public VCBehaviour(VCManager app, IvcBehaviour beha)
            : base(app, (IvcPropertyList2)beha, (string)beha.getProperty("Name"))
        {
            Behaviour = beha;
            register((Ice.Object)new hms.BehaviourTie_(this), false);
        }
    }



    public class VCComponent : VCObject, hms.ComponentOperations_
    {
        public VCManager VCMgr { get; set; }
        protected IvcComponent Component;
        private List<SignalListener> Signals;
        private List<VCBehaviour> Behaviours;
        private bool _shutdown = false;

        public VCComponent(VCManager vcapp, IvcComponent comp, string name, bool activate = true, bool icegrid = true)
            : base(vcapp, (IvcPropertyList2)comp, name)
        {
            VCMgr = vcapp;
            Component = comp;
            Behaviours = new List<VCBehaviour>();

            if (activate)
            {
                register((Ice.Object)new hms.ComponentTie_(this), icegrid);
            }
            Signals = new List<SignalListener>();
            registerSignals();

        }
        public bool isShutdown()
        {
            return _shutdown;
        }

        public override void shutdown()
        {
            foreach (VCBehaviour b in Behaviours)
            {
                b.shutdown();
            }
            deregisterSignals();
            base.shutdown();
            _shutdown = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void registerSignals()
        {
            List<object> list = new List<object>();
            list.AddRange(Component.findBehavioursOfType("ComponentSignal"));
            list.AddRange(Component.findBehavioursOfType("BooleanSignal"));
            list.AddRange(Component.findBehavioursOfType("IntegerSignal"));
            list.AddRange(Component.findBehavioursOfType("StringSignal"));
            list.AddRange(Component.findBehavioursOfType("MatrixSignal"));
            list.AddRange(Component.findBehavioursOfType("RealSignal"));
            foreach (IvcPropertyList2 behav in list)
            {
                //logger.Error(String.Format("Creating SignalListener {0} in component {1} ", behav.getProperty("Name"), this.Name));
                SignalListener listen = new SignalListener(Name, behav, IceApp);
                Signals.Add(listen);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void deregisterSignals()
        {
            foreach (SignalListener listen in Signals)
            {
                listen.shutdown();
            }
        }

        public override void put_message(hms.Message message, Ice.Current current)
        {
            string mtype;
            try
            {
                mtype = message.arguments["MessageType"];
            }
            catch (KeyNotFoundException)
            {
                logger.Error("Error message misses MessageType key" + message.ToString());
                return;
            }
            switch (mtype)
            {
                case "Signal":
                    generateSignal(message);
                    break;
                case "Command":
                    applyCommand(message);
                    break;
                default:
                    logger.Error("Unimplemented message type: " + mtype);
                    break;
            }
        }

        private void applyCommand(hms.Message message)
        {
            /*
            string command;
            try
            {
                command = message.arguments["Command"];
            }
            catch (KeyNotFoundException e)
            {
                logger.Error("Error command message misses expected key: Command" + message.ToString());
                return;
            }
            try
            {
                IvcCommand cmd = VCMgr.IvcApp.getCommand(command);
                cmd.setProperty("Component", Component);
                cmd.execute();
            }
            else
            {
                logger.Error(String.Format("Command {0} not implemented", command));
            }
             */
            logger.Warn("Command messages are not implemeted yet");
        }

        private void generateSignal(hms.Message message)
        {
            string type;
            string name;
            string val;
            try
            {
                type = message.arguments["SignalType"];
                name = message.arguments["SignalName"];
                val = message.arguments["SignalValue"];
            }
            catch (KeyNotFoundException e)
            {
                logger.Error("Error message misses expected key: " + message.ToString() + "\n Exception: " + e.Message);
                return;
            }
            IvcSignal signal;
            switch (type)
            {
                case "BooleanSignal":
                    signal = getSignal(type, name);
                    if (signal != null)
                    {
                        signal.setProperty("Value", Convert.ToBoolean(val));
                    }
                    break;
                case "StringSignal":
                    signal = getSignal(type, name);
                    if (signal != null)
                    {
                        signal.setProperty("Value", val);
                    }
                    break;
                case "IntegerSignal":
                    signal = getSignal(type, name);
                    if (signal != null)
                    {
                        signal.setProperty("Value", Convert.ToInt64(val));
                    }
                    break;
                case "RealSignal":
                    signal = getSignal(type, name);
                    if (signal != null)
                    {
                        signal.setProperty("Value", Convert.ToDouble(val));
                    }
                    break;
                default:
                    logger.Error("Unimplemented type: " + type);
                    break;
            }
        }

        private IvcSignal getSignal(string signalType, string name)
        {
            object[] behavs = Component.findBehavioursOfType(signalType);
            foreach (IvcBehaviour2 behav in behavs)
            {
                if (behav.getProperty("Name") == name)
                {
                    logger.Info(String .Format("Generating signal {0} of type {1} in component {2}", name, signalType, Name));
                    return (IvcSignal)behav;
                }
            }
            logger.Error(String.Format("Component {0} does not have a signal of name: {1}", this.Name, name));
            return null;
        }

        public string[] getBehaviourList(Ice.Current current__)
        {
            string[] result = new string[Component.RootNode.BehaviourCount];
            for (int i = 0; i < Component.RootNode.BehaviourCount; i++)
            {
                IvcBehaviour b = Component.RootNode.getBehaviour(i);
                result[i] = b.getProperty("Name");
            }
            return result;
        }

        public hms.BehaviourPrx getBehaviour(string name, Ice.Current current__)
        {
            VCBehaviour tmp = new VCBehaviour(VCMgr, Component.findBehaviour(name));
            Behaviours.Add(tmp);
            return hms.BehaviourPrxHelper.checkedCast(tmp.Proxy);
        }
    }
}
