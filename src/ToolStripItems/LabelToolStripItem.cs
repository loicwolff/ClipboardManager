using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    class LabelToolStripItem : ToolStripControlHost
    {
        public LabelToolStripItem(string text) : base(new Label())
        {
            var label = Control as Label;
            label.Text = text;
            label.BackColor = Color.Transparent;
            label.ForeColor = Color.Gray;
        }
    }
}
