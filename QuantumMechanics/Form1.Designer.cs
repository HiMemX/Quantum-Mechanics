namespace QuantumMechanics
{
    partial class mainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControlPanel = new System.Windows.Forms.Panel();
            this.debugLogTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.glControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControlPanel
            // 
            this.glControlPanel.Controls.Add(this.stopButton);
            this.glControlPanel.Controls.Add(this.startButton);
            this.glControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlPanel.Location = new System.Drawing.Point(0, 0);
            this.glControlPanel.Name = "glControlPanel";
            this.glControlPanel.Size = new System.Drawing.Size(1020, 606);
            this.glControlPanel.TabIndex = 0;
            // 
            // debugLogTextBox
            // 
            this.debugLogTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugLogTextBox.Location = new System.Drawing.Point(0, 606);
            this.debugLogTextBox.Multiline = true;
            this.debugLogTextBox.Name = "debugLogTextBox";
            this.debugLogTextBox.ReadOnly = true;
            this.debugLogTextBox.Size = new System.Drawing.Size(1020, 65);
            this.debugLogTextBox.TabIndex = 0;
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startButton.Location = new System.Drawing.Point(942, 577);
            this.startButton.Name = "startButton";
            this.startButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stopButton.Location = new System.Drawing.Point(942, 548);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 1;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 671);
            this.Controls.Add(this.glControlPanel);
            this.Controls.Add(this.debugLogTextBox);
            this.Name = "mainForm";
            this.Text = "Quantum Simulation";
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.glControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel glControlPanel;
        private System.Windows.Forms.TextBox debugLogTextBox;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button startButton;
    }
}

