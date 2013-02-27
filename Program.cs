using System;
using icehms;
using System.Threading;

namespace VC2Ice
{
    static class Program
    {
        //[STAThread]
        static void Main(string[] args)
        {
            icehms.IceApp iceapp = null;
            VCApp vcapp = null;
            string host = "Localhost";
            int port = 12000;
            if (args.Length == 1)
            {
                host = args[0];
            }
            else if (args.Length == 2)
            {

                host = args[0];
                port = Convert.ToInt16(args[1]);
            }
            try
            {
                iceapp = new IceApp("VC2IceAdapter", host, port, false);
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
