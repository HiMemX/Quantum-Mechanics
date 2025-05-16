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
            this.SuspendLayout();
            // 
            // glControlPanel
            // 
            this.glControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlPanel.Location = new System.Drawing.Point(0, 0);
            this.glControlPanel.Name = "glControlPanel";
            this.glControlPanel.Size = new System.Drawing.Size(800, 450);
            this.glControlPanel.TabIndex = 0;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.glControlPanel);
            this.Name = "mainForm";
            this.Text = "Quantum Simulation";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel glControlPanel;
    }
}

