using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net.Appender;

using icehms;

namespace VC2HMS
{
    public partial class mainForm : Form, IAppender
    {


        public static icehms.IceManager iceapp = null;
        public static VCManager vcapp = null;
        private log4net.ILog log;
        private int LogMaxSize = 5000;
        public mainForm()
        {
            InitializeComponent();
            //Set up logging
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(typeof(Program));
            ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.AddAppender(this);

        }

        public void Start()
        {
            StartButton.Enabled = false;
            //Parse command line
            string host = ServerBox.Text  ;
            int port = int.Parse(PortBox.Text);
            log.Info(String.Format("Using {0}:{1} as IceHMS discovery server", host, port.ToString()));

            //Create objects and start
            try
            {
                log.Info("Connected to IceHMS");
                iceapp = new IceManager("VC2IceAdapter", host, port, false);

            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ": Could not start IceHMS, are the IceHMS servers running?");
                StartButton.Enabled = true;
                return;
            }
            try
            {
                log.Info("Connected to Visual Component");
                vcapp = new VCManager(iceapp);
                vcapp.start();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ": Could not connect to Visual Component");
                StopButton.Enabled = true;
                return;

            }
            log.Info("VCHMS running");
            StopButton.Enabled = true;


        }

        public void Stop()
        {
            StopButton.Enabled = false;
            log.Info("\nCleaning up\n");
            try
            {
                if (vcapp != null)
                {
                    if ( ! vcapp.isShutdown())
                    {
                        vcapp.shutdown();
                    }
                }
            }
            finally
            {
                if (iceapp != null)
                {
                    iceapp.shutdown();
                }
            }
            log.Info("\nCleanup finished\n");
            StartButton.Enabled = true;
        }

        private void Cleanup(object sender, FormClosingEventArgs e)
        {
            Stop();
            e.Cancel = false; // accept shutdown
        }


        private void StartButton_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        public void DoAppend(log4net.Core.LoggingEvent loggingEvent)
        {



            String msg = String.Format("{0}  {1}: {2}", loggingEvent.LoggerName, loggingEvent.Level.Name, loggingEvent.MessageObject.ToString());
            //this.Invoke(logAppend, msg);
            this.Invoke((MethodInvoker)delegate{ 
                logBox.Items.Insert(0, msg); 
            while (logBox.Items.Count > LogMaxSize){
                logBox.Items.RemoveAt(logBox.Items.Count - 1);
            }
            });

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stop();
            Properties.Settings.Default["Server"] = ServerBox.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void ServerBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {


            ServerBox.Text = Properties.Settings.Default["Server"].ToString();
            StopButton.Enabled = false;    
        }





    }
}
