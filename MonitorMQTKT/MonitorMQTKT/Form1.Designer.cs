
namespace MonitorMQTKT
{
    partial class FrmMonitor
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpErrores = new System.Windows.Forms.GroupBox();
            this.txtErrores = new System.Windows.Forms.TextBox();
            this.grpErrores.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpErrores
            // 
            this.grpErrores.Controls.Add(this.txtErrores);
            this.grpErrores.Location = new System.Drawing.Point(21, 304);
            this.grpErrores.Name = "grpErrores";
            this.grpErrores.Size = new System.Drawing.Size(1019, 220);
            this.grpErrores.TabIndex = 0;
            this.grpErrores.TabStop = false;
            this.grpErrores.Text = "Errores";
            // 
            // txtErrores
            // 
            this.txtErrores.Location = new System.Drawing.Point(26, 40);
            this.txtErrores.Multiline = true;
            this.txtErrores.Name = "txtErrores";
            this.txtErrores.Size = new System.Drawing.Size(969, 163);
            this.txtErrores.TabIndex = 0;
            // 
            // FrmMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 536);
            this.Controls.Add(this.grpErrores);
            this.Name = "FrmMonitor";
            this.Text = "FrmMonitor";
            this.grpErrores.ResumeLayout(false);
            this.grpErrores.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpErrores;
        private System.Windows.Forms.TextBox txtErrores;
    }
}

