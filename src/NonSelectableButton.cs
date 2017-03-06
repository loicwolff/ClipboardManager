using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    class NonSelectableButton : Button
    {
        public NonSelectableButton()
        {
            this.SetStyle(ControlStyles.Selectable, false);
        }
    }
}
