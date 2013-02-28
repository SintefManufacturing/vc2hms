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
            log4net.Config.XmlConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
            icehms.IceManager iceapp = null;
            VCManager vcapp = null;
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
            Console.WriteLine(String.Format("Using {0}:{1} as IceHMS discovery server", host, port.ToString()));
            try
            {
                iceapp = new IceManager("VC2IceAdapter", host, port, false);
                vcapp = new VCManager(iceapp);
                Console.WriteLine("VC2IceHMS running, press any key to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                log.Fatal( ex.Message + ": Could not start IceHMS, are the IceHMS servers running?"); //ex);
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
