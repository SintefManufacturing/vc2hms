using System;
using vcCOM;
using System.Collections.Generic;

namespace vc2ice
{
    public class Listener : IvcEventPropertyListener
    {
        IvcPropertyList2 m_Signal;
        String m_ComponentName;
        String m_Type;
        hms.SignalPrx m_pub;
        icehms.IceApp IceApp;
        List<VCComponent> CompList;


        public Listener(string componentName, IvcPropertyList2 signal, icehms.IceApp iceapp)
        {
            IceApp = iceapp;
            m_ComponentName = componentName;
            m_Signal = signal;
            IvcEventProperty l_EProp = (IvcEventProperty)signal.getPropertyObject("Value");
            m_Type = m_Signal.getProperty("Type");
            CompList = new List<VCComponent>();
            //Console.WriteLine("Signal type is: " + m_Type);

            m_pub = hms.SignalPrxHelper.uncheckedCast(iceapp.getEventPublisher(getID()));
            IvcEventPropertyListener l_Listener = this;
            l_EProp.addListener(this);
        }

        public void shutdown()
        {
            IvcEventProperty l_EProp = (IvcEventProperty)m_Signal.getPropertyObject("Value");
            if (l_EProp != null)
            {
                l_EProp.removeListener(this);
            }
            
        }

        public string getName()
        {
            return (string)m_Signal.getProperty("Name");
        }

        public string getComponentName()
        {
            return m_ComponentName;
        }
        public string getID()
        {
            return getComponentName() + "::" + m_Type + "::" + getName();
        }


        public void notifySetupChanged(ref IvcProperty Property)
        {
        }
        /*
        private IvcComponent getComponent(ref IvcProperty Property, object Value)
        {
            if (Value == null || Property.ExtendedValue == null)
            {
                Console.WriteLine("Component signal does not contain extended value this is not normal !!!!!!!!!!!!");
                return null;
            }
            else
            {
                IvcComponent comp = (IvcComponent)Property.ExtendedValue;
                if (comp.RootNode == null)
                {
                    Console.WriteLine("Root node of comp is null!!!!!!!!!!!!");
                    //string name = comp.getProperty("Name");
                    //Console.WriteLine("name");
                    return null;
                }
                else
                {
                    return comp;
                }
            }

        }
        */
        public void notifyValueChanged(ref IvcProperty Property, object Value)
        {
            if (m_Type == "ComponentSignal")
            {
                Console.WriteLine(getID() + " got Component Signal for comp " + Value + " of type: " + Value.GetType());
                IvcComponent comp = (IvcComponent)Property.ExtendedValue;
                //IvcComponent comp = getComponent(ref Property, Value);
                if (comp != null)
                {
                    Console.WriteLine("Find if holon exist");
                    //find if holon exist for component
                    string name = comp.getProperty("Name");
                    string dname = name + comp.getProperty("SessionID"); //name for dynamic components
                    Ice.ObjectPrx prx = IceApp.getHolon(dname);
                    if (prx == null)
                    {
                        prx = IceApp.getHolon(name); //FIXME: we might return wonrg object if it is instanciated
                        dname = name;
                    }
                    if (prx != null)
                    {
                        Console.WriteLine("Sending event and msg");
                        //m_pub.newComponentSignal(m_Signal.getProperty("Name"), hms.ComponentPrxHelper.uncheckedCast(prx));
                        //Now send a message
                        Helpers.printMatrix("comp pose: ", comp.RootNode.getProperty("WorldPositionMatrix"));
                        hms.Message msg = new hms.Message();
                        msg.arguments = new System.Collections.Generic.Dictionary<string, string>();
                        msg.arguments.Add("EmitorName", m_ComponentName);
                        msg.arguments.Add("ComponentName", dname);
                        msg.arguments.Add("SignalName", m_Type);
                        msg.arguments.Add("SignalType", m_Type);
                        msg.arguments.Add("WorldPositionMatrix", String.Join(",", comp.RootNode.getProperty("WorldPositionMatrix")));
                        m_pub.putMessage(msg);
                    }

                }
            }
            else
            {
                //first resend signal to Ice
                //m_pub.newBooleanSignal(m_Signal.getProperty("Name"), Property.ExtendedValue);
                //Now send a message
                Console.WriteLine(getID() + " got Boolean Signal " + (string)m_Signal.getProperty("Name") + "  " + Value.ToString() + " re-sending it to icehms");
                hms.Message msg = new hms.Message();
                msg.arguments = new System.Collections.Generic.Dictionary<string, string>();
                msg.arguments.Add("EmitortName", m_ComponentName);
                msg.arguments.Add("SignalName", m_Type);
                msg.arguments.Add("SignalType", m_Type);
                msg.arguments.Add("SignalValue", Value.ToString());
                m_pub.putMessage(msg);
            }

        }


        private void print_properties(IvcComponent comp)
        {
            string[] list = new string[comp.PropertyCount];
            for (int i = 0; i < comp.PropertyCount; i++)
            {
                string n = comp.getPropertyName(i);
                Console.WriteLine("prop: " + n);
                Console.WriteLine("val: " + comp.getProperty(n));
            }
        }
    }
}
