using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace vc2ice
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            IceApp iceapp = null;
            VCApp vcapp = null;
            try
            {
                iceapp = new IceApp();
                vcapp = new VCApp();
                Application.Run(new Form1(iceapp, vcapp));
            }
            finally
            {
                if (iceapp != null)
                {
                    iceapp.cleanup();
                }
            }

        }
    }
}
