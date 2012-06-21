using System;
using vcCOM;

namespace vc2ice
{
    public class Listener : IvcEventPropertyListener
    {
        IvcPropertyList2 m_Signal;
        String m_ComponentName;
        String m_Type;
        hms.SignalPrx m_pub;
        icehms.IceApp IceApp;


        public Listener(string componentName, IvcPropertyList2 signal, icehms.IceApp iceapp)
        {
            IceApp = iceapp;
            m_ComponentName = componentName;
            m_Signal = signal;
            m_pub = hms.SignalPrxHelper.uncheckedCast( iceapp.getEventPublisher(getID()) );
            IvcEventProperty l_EProp = (IvcEventProperty)signal.getPropertyObject("Value");
            IvcEventPropertyListener l_Listener = this;
            l_EProp.addListener(ref l_Listener);
            m_Type = m_Signal.getProperty("Type");
       }

        public string getName()
        {
            return (string) m_Signal.getProperty("Name");
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
                              VCComponent mycomp = new VCComponent(IceApp, Property.ExtendedValue, true, false);
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

                  Console.WriteLine( getID() + " got Boolean Signal " + (string)m_Signal.getProperty("Name") + "  " + Value.ToString());
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
