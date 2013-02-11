namespace vc2ice
{
    partial class Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.JointListBox = new System.Windows.Forms.ListBox();
            this.RobotListBox = new System.Windows.Forms.ListBox();
            this.MoveButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.TestButton = new System.Windows.Forms.Button();
            this.MachineListBox = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.IceGridAddressTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.IceGridRegisterbutton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // JointListBox
            // 
            this.JointListBox.FormattingEnabled = true;
            this.JointListBox.ItemHeight = 16;
            this.JointListBox.Location = new System.Drawing.Point(176, 32);
            this.JointListBox.Margin = new System.Windows.Forms.Padding(4);
            this.JointListBox.Name = "JointListBox";
            this.JointListBox.Size = new System.Drawing.Size(200, 308);
            this.JointListBox.TabIndex = 0;
            this.JointListBox.SelectedIndexChanged += new System.EventHandler(this.JointListBox_SelectedIndexChanged);
            // 
            // RobotListBox
            // 
            this.RobotListBox.FormattingEnabled = true;
            this.RobotListBox.ItemHeight = 16;
            this.RobotListBox.Location = new System.Drawing.Point(2, 32);
            this.RobotListBox.Name = "RobotListBox";
            this.RobotListBox.Size = new System.Drawing.Size(157, 308);
            this.RobotListBox.TabIndex = 7;
            this.RobotListBox.SelectedIndexChanged += new System.EventHandler(this.RobotListBox_SelectedIndexChanged);
            // 
            // MoveButton
            // 
            this.MoveButton.Location = new System.Drawing.Point(422, 99);
            this.MoveButton.Name = "MoveButton";
            this.MoveButton.Size = new System.Drawing.Size(75, 23);
            this.MoveButton.TabIndex = 8;
            this.MoveButton.Text = "Move";
            this.MoveButton.UseVisualStyleBackColor = true;
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(422, 142);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 9;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Robots";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(203, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Joints";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(2, 346);
            this.trackBar1.Maximum = 360;
            this.trackBar1.Minimum = -180;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(515, 56);
            this.trackBar1.TabIndex = 12;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // TestButton
            // 
            this.TestButton.Location = new System.Drawing.Point(422, 212);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(75, 23);
            this.TestButton.TabIndex = 18;
            this.TestButton.Text = "Test";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
            // 
            // MachineListBox
            // 
            this.MachineListBox.FormattingEnabled = true;
            this.MachineListBox.ItemHeight = 16;
            this.MachineListBox.Location = new System.Drawing.Point(524, 32);
            this.MachineListBox.Name = "MachineListBox";
            this.MachineListBox.Size = new System.Drawing.Size(407, 308);
            this.MachineListBox.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(524, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 17);
            this.label3.TabIndex = 20;
            this.label3.Text = "Signals";
            // 
            // IceGridAddressTextBox
            // 
            this.IceGridAddressTextBox.Location = new System.Drawing.Point(1024, 51);
            this.IceGridAddressTextBox.Name = "IceGridAddressTextBox";
            this.IceGridAddressTextBox.Size = new System.Drawing.Size(162, 22);
            this.IceGridAddressTextBox.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1058, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 22;
            this.label4.Text = "Ice Server IP";
            // 
            // IceGridRegisterbutton
            // 
            this.IceGridRegisterbutton.Location = new System.Drawing.Point(1210, 51);
            this.IceGridRegisterbutton.Name = "IceGridRegisterbutton";
            this.IceGridRegisterbutton.Size = new System.Drawing.Size(75, 23);
            this.IceGridRegisterbutton.TabIndex = 23;
            this.IceGridRegisterbutton.Text = "Register";
            this.IceGridRegisterbutton.UseVisualStyleBackColor = true;
            this.IceGridRegisterbutton.Click += new System.EventHandler(this.IceGridRegisterbutton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 573);
            this.Controls.Add(this.IceGridRegisterbutton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.IceGridAddressTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MachineListBox);
            this.Controls.Add(this.TestButton);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.MoveButton);
            this.Controls.Add(this.RobotListBox);
            this.Controls.Add(this.JointListBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "VC2IceHMS";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox JointListBox;
        private System.Windows.Forms.ListBox RobotListBox;
        private System.Windows.Forms.Button MoveButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button TestButton;
        private System.Windows.Forms.ListBox MachineListBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox IceGridAddressTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button IceGridRegisterbutton;

    }
}

