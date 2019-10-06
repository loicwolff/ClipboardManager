namespace ClipboardManager
{
    using System;
    using System.Windows.Forms;

    using Clipboard = SafeClipboard;

    /// <summary>
    /// Provides notifications when the contents of the clipboard is updated.
    /// http://stackoverflow.com/questions/2226920/how-to-monitor-clipboard-content-changes-in-c
    /// </summary>
    public sealed class ClipboardNotifier
    {
        /// <summary>
        /// Occurs when the contents of the clipboard is updated.
        /// </summary>
        public static event ClipboardEventHandler? ClipboardUpdate;

        public delegate void ClipboardEventHandler(object? sender, ClipboardEventArgs e);

        private static int LastEventTimeStamp;

        internal static NotificationForm _form = new NotificationForm();

        /// <summary>
        /// Raises the <see cref="ClipboardUpdate"/> event.
        /// </summary>
        /// <param name="e">Event arguments for the event.</param>
        private static void OnClipboardUpdate(ClipboardEventArgs e) => ClipboardUpdate?.Invoke(null, e);

        /// <summary>
        /// Hidden form to recieve the WM_CLIPBOARDUPDATE message.
        /// </summary>
        internal class NotificationForm : Form
        {
            public NotificationForm()
            {
                NativeMethods.SetParent(this.Handle, NativeMethods.HWND_MESSAGE);

                NativeMethods.AddClipboardFormatListener(this.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE && Environment.TickCount - LastEventTimeStamp > 500)
                {
                    string clipboard = Clipboard.GetText();

                    LastEventTimeStamp = Environment.TickCount;

                    OnClipboardUpdate(new ClipboardEventArgs(new ClipItem(clipboard)));
                }

                base.WndProc(ref m);
            }
        }
    }

    public class ClipboardEventArgs : EventArgs
    {
        public ClipboardEventArgs(ClipItem clipItem)
        {
            this.ClipItem = clipItem;
        }

        public ClipItem ClipItem { get; set; }
    }
}
