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
using icehms;
using System.Threading;

namespace VC2HMS
{
    static class Program
    {
        public static icehms.IceManager iceapp = null;
        public static VCManager vcapp = null;

        static void Main(string[] args)
        {
            //Set up logging
            log4net.Config.XmlConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
            
            //Parse command line
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

            //Now catch ctrl-c to shutdown properly
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("\nCancelEvent catched\n");
                eventArgs.Cancel = true; //
            };

            //Create objects and start
            try
            {
                iceapp = new IceManager("VC2IceAdapter", host, port, false);
                vcapp = new VCManager(iceapp);
                Console.WriteLine("VC2HMS running, press any key to exit");
                vcapp.start();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ": Could not start IceHMS, are the IceHMS servers running?");
            }
            finally
            {
                cleanup();
            }
            Console.WriteLine("Press any key to close window");
            Console.ReadLine();
        }

        static void cleanup()
        {
            Console.WriteLine("\nCleaning up\n");
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
    }
}
