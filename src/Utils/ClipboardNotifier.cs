using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ClipboardManager
{
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
        public static event ClipboardEventHandler ClipboardUpdate;

        public delegate void ClipboardEventHandler(object sender, ClipboardEventArgs e);

        private static int LastEventTimeStamp;

        public static NotificationForm _form = new NotificationForm();

        /// <summary>
        /// Raises the <see cref="ClipboardUpdate"/> event.
        /// </summary>
        /// <param name="e">Event arguments for the event.</param>
        private static void OnClipboardUpdate(ClipboardEventArgs e)
        {
            var handler = ClipboardUpdate;
            if (handler != null)
            {
                handler(null, e);
            }
        }

        /// <summary>
        /// Hidden form to recieve the WM_CLIPBOARDUPDATE message.
        /// </summary>
        public class NotificationForm : Form
        {
            private string LastClipboard { get; set; }

            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);

                NativeMethods.AddClipboardFormatListener(Handle);

                LastClipboard = null;
            }

            private bool IsNewClipboard(out string clipItem)
            {
                string clipboardText = Clipboard.GetText();
                
                clipItem = null;

                // si le presse-papier n'est vide, et qu'il est différent 
                if (!String.IsNullOrWhiteSpace(clipboardText) && clipboardText != LastClipboard)
                {
                    clipItem = clipboardText;
                    return true;
                }

                return false;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE &&
                    Environment.TickCount - LastEventTimeStamp > 500)
                {
                    string clipboard = Clipboard.GetText();

                    LastClipboard = clipboard;

                    LastEventTimeStamp = Environment.TickCount;

                    OnClipboardUpdate(new ClipboardEventArgs() { ClipItem = new ClipItem() { Text = clipboard } });
                }

                base.WndProc(ref m);
            }
        }
    }

    public class ClipboardEventArgs : EventArgs
    {
        public ClipItem ClipItem { get; set; }
    }

    internal static class NativeMethods
    {
        // See http://msdn.microsoft.com/en-us/library/ms649021%28v=vs.85%29.aspx
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        // See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        // See http://msdn.microsoft.com/en-us/library/ms633541%28v=vs.85%29.aspx
        // See http://msdn.microsoft.com/en-us/library/ms649033%28VS.85%29.aspx
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}
