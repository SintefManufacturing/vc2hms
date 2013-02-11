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
        int CompCounter;
        List<VCComponent> CompList;


        public Listener(string componentName, IvcPropertyList2 signal, icehms.IceApp iceapp)
        {
            IceApp = iceapp;
            m_ComponentName = componentName;
            m_Signal = signal;
            IvcEventProperty l_EProp = (IvcEventProperty)signal.getPropertyObject("Value");
            m_Type = m_Signal.getProperty("Type");
            CompCounter = 0;
            CompList = new List<VCComponent>();
            //Console.WriteLine("Signal type is: " + m_Type);

            m_pub = hms.SignalPrxHelper.uncheckedCast(iceapp.getEventPublisher(getID()));
            IvcEventPropertyListener l_Listener = this;
            l_EProp.addListener(ref l_Listener);
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


        #region IvcEventPropertyListener Members
        public void notifySetupChanged(ref IvcProperty Property)
        {
        }

        public void notifyValueChanged(ref IvcProperty Property, object Value)
        {


            if (m_Type == "ComponentSignal")
            {
                if (Value != null)
                {
                    Console.WriteLine(getID() + " got Component Signal of type: " + Value.GetType());

                    if (Property.ExtendedValue != null)
                    {
                        IvcComponent comp = (IvcComponent)Property.ExtendedValue;
                        Console.WriteLine(getID() + " Extended type: " + comp.GetType());
                        //Console.WriteLine( comp.getProperty("Container::Location"));

                        if (comp.RootNode != null)
                        {
                            //first resend signal to Ice
                            //FIXME: should probably keep list of components for gc and use uniq names otherwise they are will be unreachable!!
                            CompCounter++;
                            if (CompCounter > 6) { CompCounter = 0; }
                            VCComponent mycomp = new VCComponent(IceApp, Property.ExtendedValue, true, false);
                            CompList.Insert(CompCounter - 1, mycomp);

                            m_pub.newComponentSignal(m_Signal.getProperty("Name"), hms.ComponentPrxHelper.uncheckedCast(mycomp.Proxy));
                            //Now send a message
                            Helpers.printMatrix("comp pose: ", comp.RootNode.getProperty("WorldPositionMatrix"));
                            hms.Message msg = new hms.Message();
                            msg.arguments = new System.Collections.Generic.Dictionary<string, string>();
                            msg.arguments.Add("ComponentName", m_ComponentName);
                            msg.arguments.Add("SignalName", m_Type);
                            msg.arguments.Add("SignalType", m_Type);
                            msg.arguments.Add("WorldPositionMatrix", String.Join(",", comp.RootNode.getProperty("WorldPositionMatrix")));
                            m_pub.putMessage(msg);
                        }


                        //Helpers.printMatrix("comp pose: ", comp.RootNode.RootFeature.getProperty("WorldPositionMatrix"));
                    }
                }
                //IvcComponent comp = (IvcComponent)Value;
                //Helpers.printMatrix("comp pose: ",comp.RootNode.RootFeature.getProperty( "PositionMatrix") );
            }
            else
            {
                //first resend signal to Ice
                m_pub.newBooleanSignal(m_Signal.getProperty("Name"), Property.ExtendedValue);
                //Now send a message

                Console.WriteLine(getID() + " got Boolean Signal " + (string)m_Signal.getProperty("Name") + "  " + Value.ToString());
                hms.Message msg = new hms.Message();
                msg.arguments = new System.Collections.Generic.Dictionary<string, string>();
                msg.arguments.Add("ComponentName", m_ComponentName);
                msg.arguments.Add("SignalName", m_Type);
                msg.arguments.Add("SignalType", m_Type);
                msg.arguments.Add("SignalValue", Value.ToString());
                m_pub.putMessage(msg);
            }

        }
        #endregion
    }
}
