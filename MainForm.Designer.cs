namespace UnlockToPhoto;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(0, 0);
        this.Name = "MainForm";
        this.ShowInTaskbar = false;
        this.Text = "解锁即拍照";
        this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        this.ResumeLayout(false);
    }
}
