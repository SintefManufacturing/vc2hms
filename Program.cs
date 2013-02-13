using System;
using icehms;
using System.Threading;

namespace vc2ice
{
    static class Program
    {
        //[STAThread]
        static void Main()
        {
            icehms.IceApp iceapp = null;
            VCApp vcapp = null;
            try
            {
                iceapp = new IceApp("VC2IceAdapter", "utgaard.sintef.no", 12000, false);
                //iceapp = new IceApp("VC2IceAdapter", "192.168.1.15", 12000, false);
                vcapp = new VCApp(iceapp);
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                try
                {
                    if (vcapp != null)
                    {
                        vcapp.shutdown();
                    }
                }
                finally
                {
                    if (iceapp != null)
                    {
                        iceapp.shutdown();
                    }
                }
            }
            Console.WriteLine("Press any key to close window");
            Console.ReadLine();
        }
    }
}
