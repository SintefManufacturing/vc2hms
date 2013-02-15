using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;


namespace vc2ice
{

    public class VCObject : icehms.Holon
    {
        public IvcPropertyList2 PList;


        public VCObject(icehms.IceApp app, IvcPropertyList2 plist, string name)
            : base(app, name, false)
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
        public IvcBehaviour Behaviour;

        public VCBehaviour(icehms.IceApp app, IvcBehaviour beha)
            : base(app, (IvcPropertyList2)beha, (string)beha.getProperty("Name"))
        {
            Behaviour = beha;
            register((Ice.Object)new hms.BehaviourTie_(this), false);
        }


    }



    public class VCComponent : VCObject, hms.ComponentOperations_
    {
        public IvcComponent Component;
        public List<SignalListener> Signals;
        public List<VCBehaviour> Behaviours;
        private bool _shutdown = false;


        public VCComponent(icehms.IceApp app, IvcComponent comp, string name, bool activate = true, bool icegrid = true)
            : base(app, (IvcPropertyList2)comp, name)
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
        }

        private void registerSignals()
        {
            List<object> list = new List<object>();
            list.AddRange(Component.findBehavioursOfType("ComponentSignal"));
            list.AddRange(Component.findBehavioursOfType("BooleanSignal"));
            list.AddRange(Component.findBehavioursOfType("IntegerSignal"));
            list.AddRange(Component.findBehavioursOfType("StringSignal"));
            list.AddRange(Component.findBehavioursOfType("MatrixSignal"));
            list.AddRange(Component.findBehavioursOfType("RealSignal"));
            foreach (object behav in list)
            {
                SignalListener listen = new SignalListener(Name, (IvcPropertyList2)behav, IceApp);
                Signals.Add(listen);
            }
        }

        private void deregisterSignals()
        {
            foreach (SignalListener listen in Signals)
            {
                listen.shutdown();
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
