using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vcCOM;


namespace vc2ice
{
    public class VCHolon : icehms.Holon
    {
        IvcComponent Component;
        public List<Listener> Signals;


        public VCHolon(icehms.IceApp app, IvcComponent comp) : base(app, (string)comp.getProperty("Name"))
        {
            Component = comp;
            //Name = (string)comp.getProperty("Name"); //done in base class
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


    }
}
