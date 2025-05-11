using System.Windows.Forms;

namespace Proyecto
{
    partial class Form1:Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Name = "MainForm";
            this.Text = "Super Smash Trees";
            this.ResumeLayout(false);
        }
    }
}
