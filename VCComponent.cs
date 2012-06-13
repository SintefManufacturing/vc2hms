using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;


namespace vc2ice
{
    public class VCComponent : icehms.Holon, hms.VCComponentOperations_
    {
        public IvcComponent Component;
        public List<Listener> Signals;


        public VCComponent(icehms.IceApp app, IvcComponent comp, bool activate=true) : base(app, (string)comp.getProperty("Name"), false)
        {
            Component = comp;
            //Name = (string)comp.getProperty("Name"); //done in base class
            if (activate)
            {
                register((Ice.Object)new hms.VCComponentTie_(this));
            }
            Signals = new List<Listener>();
            registerSignals();
            
        }



        public override void shutdown()
        {
            base.shutdown();
        }

        private void registerSignals()
        {
            object[] result = Component.findBehavioursOfType("ComponentSignal");
            object[] result2 =   Component.findBehavioursOfType("BooleanSignal");
            List<object> list = new List<object>();
            list.AddRange(result);
            list.AddRange(result2);
            foreach (object behav in list)
            {
                Console.WriteLine(Name + " has signal!");
                Listener listen = new Listener(Name, (IvcPropertyList2)behav, IceApp);
                Signals.Add(listen);
            }

        }



        public string getProperty(string name, Ice.Current current__)
        {
            return Convert.ToString(Component.getProperty(name));
        }

        public string[] getPropertyList(Ice.Current current__)
        {
            string[] list = new string[Component.PropertyCount];
            for (int i=0; i<Component.PropertyCount;i++)
            {
                list[i] = Component.getPropertyName(i);
            }
            return list;
        }

        public void setProperty(string name, string val, Ice.Current current__)
        {
            //everything comes as sting from ICe so we must convert it to correct type
            IvcPropertyList2 plist = (IvcPropertyList2) Component;
            IvcProperty prop = plist.getPropertyObject(name);
            //Type tp = prop.GetType();
           
            //       log("SETPROP: " + prop.getProperty("Type"));
            //log("SETPROP: " + tp + prop + tp.ToString() + tp.MakeGenericType() ) ;
            //prop.Value =  Convert.ChangeType(val, (Type) prop.getProperty("Type")  );
            string stype = prop.getProperty("Type");
            switch (stype) {
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
}
