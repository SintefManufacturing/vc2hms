using System;
using vcCOM;

namespace vc2ice
{
    class Listener : IvcEventPropertyListener
    {
        IvcPropertyList2 m_Signal;
        String m_ComponentName;
        String m_Type;

        public Listener(string componentName, IvcPropertyList2 signal)
        {
            m_ComponentName = componentName;
            m_Signal = signal;
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
                          Console.WriteLine(comp.RootNode);
                          if (comp.RootNode != null)
                          {
                              Helpers.printMatrix("comp pose: ", comp.RootNode.getProperty("WorldPositionMatrix"));
                          }


                          //Helpers.printMatrix("comp pose: ", comp.RootNode.RootFeature.getProperty("WorldPositionMatrix"));
                      }
                  }
                  //IvcComponent comp = (IvcComponent)Value;
                  //Helpers.printMatrix("comp pose: ",comp.RootNode.RootFeature.getProperty( "PositionMatrix") );
              }
              else
              {
                  Console.WriteLine( getID() + " got Boolean Signal " + (string)m_Signal.getProperty("Name") + "  " + Value.ToString());

              }
          
          }
          #endregion
    }
}
