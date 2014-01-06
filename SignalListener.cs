/*
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
using vcCOM;
using System.Collections.Generic;

namespace VC2HMS
{
    public class SignalListener : IvcEventPropertyListener
    {
        private IvcPropertyList2 SignalProperties;
        private String ComponentName;
        private String SignalType;
        private hms.MessageInterfacePrx Publisher;
        private icehms.IceManager iceapp;
        private string PublisherName;
        private string SignalName;
        log4net.ILog logger;


        public SignalListener(string componentName, IvcPropertyList2 signal, icehms.IceManager iceapp)
        {
            this.iceapp = iceapp;
            ComponentName = componentName;
            SignalProperties = signal;
            logger = log4net.LogManager.GetLogger(this.GetType().Name);
            IvcEventProperty eprop = (IvcEventProperty)signal.getPropertyObject("Value");
            SignalType = SignalProperties.getProperty("Type");
            SignalName = (string)SignalProperties.getProperty("Name");
            PublisherName = ComponentName + "::" + SignalName;

            Publisher = hms.MessageInterfacePrxHelper.uncheckedCast(iceapp.getEventPublisher(PublisherName));
            eprop.addListener(this);
        }

        public IvcPropertyList2 getPropertyListObject()
        {
            return SignalProperties;
        }

        public string getComponentName()
        {
            return this.ComponentName;
        }

        public string getSignalName()
        {
            return this.SignalName;
        }

        public string getSignalType()
        {
            return this.SignalType;
        }
        public string getPublisherName()
        {
            return this.PublisherName;
        }


        public void shutdown()
        {
            IvcEventProperty l_EProp = (IvcEventProperty)SignalProperties.getPropertyObject("Value");
            if (l_EProp != null)
            {
                l_EProp.removeListener(this);
            }         
        }

        public void notifySetupChanged(ref IvcProperty Property)
        {
        }

        public hms.Message createMessage(string Value)
        {
            hms.Message msg = new hms.Message();
            msg.sender = ComponentName;
            msg.arguments = new System.Collections.Generic.Dictionary<string, string>();
            msg.arguments.Add("SignalName", SignalName);
            msg.arguments.Add("SignalType", SignalType);
            msg.arguments.Add("SignalValue", Value);
            return msg;
        }

        public void notifyValueChanged(ref IvcProperty Property, object Value)
        {
            logger.Info(String.Format(" {0} generated signal {3} of type {1} and value {2}", ComponentName, Value.GetType(), Value, SignalName)); 
            if (SignalType == "ComponentSignal")
            {
                IvcComponent comp = (IvcComponent)Property.ExtendedValue;
                //IvcComponent comp = getComponent(ref Property, Value);
                if (comp != null)
                {
                    //find if holon exist for component
                    string name = comp.getProperty("Name");
                    string dname = name + comp.getProperty("SessionID"); //name for dynamic components
                    Ice.ObjectPrx prx = iceapp.getHolon(dname);
                    if (prx == null)
                    {
                        prx = iceapp.getHolon(name); //FIXME: we might return wonrg object if it is instanciated
                        dname = name;
                    }
                    if (prx != null)
                    {
                        //m_pub.newComponentSignal(m_Signal.getProperty("Name"), hms.ComponentPrxHelper.uncheckedCast(prx));
                        //Now send a message
                        //Helpers.printMatrix("comp pose: ", comp.RootNode.getProperty("WorldPositionMatrix"));
                        hms.Message msg = createMessage(dname);
                        msg.arguments.Add("ComponentName", dname);
                        msg.arguments.Add("WorldPositionMatrix", Helpers.formatMatrix(comp.RootNode.getProperty("WorldPositionMatrix")));
                        Publisher.put_message(msg);
                    }
                }
            }
            else
            {
                //first resend signal to Ice
                //m_pub.newBooleanSignal(m_Signal.getProperty("Name"), Property.ExtendedValue);
                //Now send a message
                hms.Message msg = createMessage(Value.ToString());
                Publisher.put_message(msg);
            }

        }


        private static void print_properties(IvcComponent comp)
        {
            for (int i = 0; i < comp.PropertyCount; i++)
            {
                string n = comp.getPropertyName(i);
                Console.WriteLine("prop: " + n);
                Console.WriteLine("val: " + comp.getProperty(n));
            }
        }
    }
}
