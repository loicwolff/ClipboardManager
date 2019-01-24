namespace ClipboardManager
{
    using System.Drawing;
    using System.Windows.Forms;

    class LabelToolStripItem : ToolStripControlHost
    {
        public LabelToolStripItem(string text) : base(new Label())
        {
            var label = this.Control as Label;
            label.Text = text;
            label.BackColor = Color.Transparent;
            label.ForeColor = Color.Gray;
        }
    }
}
