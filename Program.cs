using System;
using System.Windows.Forms;
using icehms;

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
            icehms.IceApp iceapp = null;
            VCApp vcapp = null;
            try
            {
                iceapp = new IceApp("VC2IceAdapter", "utopia.sintef.no", 12000, false);
                vcapp = new VCApp(iceapp);
                Application.Run(new Form1(iceapp, vcapp));
            }
            catch
            {
                if (iceapp != null)
                {
                    vcapp.shutdown();
                    iceapp.shutdown();
                }
            }
            finally
            {
                if (iceapp != null)
                {
                    vcapp.shutdown();
                    iceapp.shutdown();
                }
            }

        }
    }
}
