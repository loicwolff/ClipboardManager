using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    public static class SafeClipboard
    {
        public static ClipItem CurrentClipItem
        {
            get
            {
                if (ContainsText())
                {
                    string text = GetText();

                    return new ClipItem
                    {
                        Text = String.IsNullOrWhiteSpace(text) ? String.Empty : text,
                    };
                }
                else
                {
                    return ClipItem.Empty;
                }
            }
        }

        public static void SetImage(Image image)
        {
            try
            {
                Clipboard.SetImage(image);
            }
            catch (ExternalException)
            {
                // retry
                Clipboard.SetImage(image);
            }

        }

        public static void SetText(string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                try
                {
                    Clipboard.SetText(text, TextDataFormat.Text);
                }
                catch (ExternalException)
                {
                    // retry
                    Clipboard.SetText(text, TextDataFormat.Text);
                }
            }
        }

        public static void Clear()
        {
            Clipboard.Clear();
        }

        public static bool ContainsText()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch (ExternalException)
            {
                return Clipboard.ContainsText();
            }
        }

        public static string GetText()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch (ExternalException)
            {
                return Clipboard.GetText();
            }
        }

        public static bool ContainsImage()
        {
            try
            {
                return Clipboard.ContainsImage();
            }
            catch (ExternalException)
            {
                return Clipboard.ContainsImage();
            }
        }

        public static Image GetImage()
        {
            try
            {
                return Clipboard.GetImage();
            }
            catch (ExternalException)
            {
                return Clipboard.GetImage();
            }
        }
    }
}
