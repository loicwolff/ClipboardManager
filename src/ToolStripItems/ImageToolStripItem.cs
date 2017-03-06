using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    class ImageToolStripItem : ToolStripControlHost
    {
        public event ImageToolStripEventHandler ImageClicked;

        public Panel MainPanel { get; set; }

        public Image BackgroundImage { get; set; }

        public ImageToolStripItem(Image image, ImageToolStripEventHandler eventHandler) : base(new Panel())
        {
            this.Click += OnClick;

            ImageClicked = eventHandler;

            BackgroundImage = image;

            this.MainPanel = this.Control as Panel;
            MainPanel.BackgroundImage = image;
            MainPanel.BackgroundImageLayout = ImageLayout.Zoom;
            MainPanel.BorderStyle = BorderStyle.FixedSingle;
            MainPanel.BackColor = Color.Transparent;
            MainPanel.AutoSize = false;
            MainPanel.Size = new System.Drawing.Size(200, 75);
            MainPanel.MinimumSize = MainPanel.Size;
            //MainPanel.Click += MainPanel_Click;
        }

        void OnClick(object sender, EventArgs e)
        {
            if (ImageClicked != null)
            {
                ImageClicked(this, new ImageToolStripEventArgs(BackgroundImage));
            }
        }

        void MainPanel_Click(object sender, EventArgs e)
        {
            if (ImageClicked != null)
            {
                ImageClicked(this, new ImageToolStripEventArgs(BackgroundImage));
            }
        }
    }

    public delegate void ImageToolStripEventHandler(object sender, ImageToolStripEventArgs e);

    public class ImageToolStripEventArgs : EventArgs
    {
        public Image Image { get; set; }

        public ImageToolStripEventArgs(Image image)
        {
            Image = image;
        }
    }
}
