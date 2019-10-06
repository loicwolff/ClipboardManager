using System.Windows.Forms;

namespace ClipboardManager
{
    internal class NonSelectableButton : Button
    {
        public NonSelectableButton() => this.SetStyle(ControlStyles.Selectable, false);
    }
}
