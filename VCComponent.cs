using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;
using System.Runtime.CompilerServices;
using System.Threading;


namespace VC2Ice
{

    public class VCObject : icehms.Holon
    {
        private IvcPropertyList2 PList;

        public VCObject(icehms.IceApp iceapp, IvcPropertyList2 plist, string name)
            : base(iceapp, name, false)
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

            //       log("SETPROP: " + prop.getProperty("Type"));
            //log("SETPROP: " + tp + prop + tp.ToString() + tp.MakeGenericType() ) ;
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
                    log("Uknown format for property: " + name + " and value: " + val + " of type: " + stype);
                    break;
            }

        }




    }


    public class VCBehaviour : VCObject, hms.BehaviourOperations_
    {
        private IvcBehaviour Behaviour;

        public VCBehaviour(icehms.IceApp app, IvcBehaviour beha)
            : base(app, (IvcPropertyList2)beha, (string)beha.getProperty("Name"))
        {
            Behaviour = beha;
            register((Ice.Object)new hms.BehaviourTie_(this), false);
        }
    }



    public class VCComponent : VCObject, hms.ComponentOperations_
    {
        protected IvcComponent Component;
        private List<SignalListener> Signals;
        private List<VCBehaviour> Behaviours;
        private bool _shutdown = false;


        public VCComponent(icehms.IceApp iceapp, IvcComponent comp, string name, bool activate = true, bool icegrid = true)
            : base(iceapp, (IvcPropertyList2)comp, name)
        {
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
                //log(String.Format("Creating SignalListener {0} in component {1} ", behav.getProperty("Name"), this.Name));
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
                log("Error message misses expected key: " + message.ToString() + "\n Exception: " + e);
                return;
            }
            switch (type)
            {
                case "BooleanSignal":
                    object[] behavs = Component.findBehavioursOfType("BooleanSignal");
                    foreach (IvcBehaviour2 behav in behavs)
                    {
                        if (behav.getProperty("Name") == name)
                        {
                            ((IvcSignal)behav).setProperty("Value", Convert.ToBoolean(val));
                            return;
                        }
                    }
                    log(String.Format("Component {0} does not have a signal of name: {1}", this.Name, name));
                    break;
                case "ComponentSignal":
                    break;
                default:
                    log("Unimplemented type: " + type);
                    break;
            }
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
            VCBehaviour tmp = new VCBehaviour(IceApp, Component.findBehaviour(name));
            Behaviours.Add(tmp);
            Console.WriteLine(tmp);
            //return (hms.Behaviour) tmp;
            return hms.BehaviourPrxHelper.checkedCast(tmp.Proxy);
        }
    }
}
