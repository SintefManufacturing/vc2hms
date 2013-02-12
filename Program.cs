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
                /*
               while (Console.ReadKey(true).Key != ConsoleKey.Escape)
               {
                   Thread.Sleep(10);
               }
               Console.WriteLine("VC2IceHMS Finished");
               //Console.ReadKey(true);
               
                
               while (true)
               {
                   Thread.Sleep(10);
               }
                   */
                //System("pause");

            }
            catch (Exception ex)
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
            Console.WriteLine("Press any key to close");
            Console.ReadLine();

        }
    }
}
