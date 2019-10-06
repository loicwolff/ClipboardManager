namespace ClipboardManager
{
    using System.Drawing;
    using System.Windows.Forms;

    class LabelToolStripItem : ToolStripControlHost
    {
        public LabelToolStripItem(string text) : base(new Label())
        {
            if (this.Control is Label label)
            {
                label.Text = text;
                label.BackColor = Color.Transparent;
                label.ForeColor = Color.Gray;
            }
        }
    }
}
