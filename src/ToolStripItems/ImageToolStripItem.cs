using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipboardManager
{
    internal class ImageToolStripItem : ToolStripControlHost
    {
        public event ImageToolStripEventHandler ImageClicked;

        public Panel MainPanel { get; set; }

        public Image Background { get; set; }

        public ImageToolStripItem(Image image, ImageToolStripEventHandler eventHandler) : base(new Panel())
        {
            this.Click += this.OnClick;

            ImageClicked = eventHandler;

            this.Background = image;

            this.MainPanel = this.Control as Panel;
            this.MainPanel.BackgroundImage = image;
            this.MainPanel.BackgroundImageLayout = ImageLayout.Zoom;
            this.MainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.MainPanel.BackColor = Color.Transparent;
            this.MainPanel.AutoSize = false;
            this.MainPanel.Size = new System.Drawing.Size(200, 75);
            this.MainPanel.MinimumSize = this.MainPanel.Size;
        }

        void OnClick(object? sender, EventArgs e)
        {
            ImageClicked?.Invoke(this, new ImageToolStripEventArgs(this.Background));
        }

        void MainPanel_Click(object sender, EventArgs e)
        {
            ImageClicked?.Invoke(this, new ImageToolStripEventArgs(this.Background));
        }
    }

    public delegate void ImageToolStripEventHandler(object sender, ImageToolStripEventArgs e);

    public class ImageToolStripEventArgs : EventArgs
    {
        public Image Image { get; set; }

        public ImageToolStripEventArgs(Image image)
        {
            this.Image = image;
        }
    }
}
