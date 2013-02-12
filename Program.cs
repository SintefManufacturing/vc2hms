using System;
//using System.Windows.Forms;
using icehms;
using System.Threading;

namespace vc2ice
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            icehms.IceApp iceapp = null;
            VCApp vcapp = null;
            try
            {
                iceapp = new IceApp("VC2IceAdapter", "utgaard.sintef.no", 12000, false);
                //iceapp = new IceApp("VC2IceAdapter", "192.168.1.15", 12000, false);
                vcapp = new VCApp(iceapp);
                //Application.Run(new Window(iceapp, vcapp));  
                vcapp.createCurrentComponents();
                while (true)
                {
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex )
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (vcapp != null)
                {
                    vcapp.shutdown();
                }
                if (iceapp != null)
                {
                    iceapp.shutdown();
                }
            }

        }
    }
}
