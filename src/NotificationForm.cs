using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    public partial class NotificationForm : Form
    {
        public event MouseEventHandler? OpenClick;
        public event MouseEventHandler? CopyLinkClick;
        public event MouseEventHandler? RightClick;

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        private readonly Timer fadeoutTimer;

        private bool fadingIn = false;

        public bool NotificationClosing { get; set; }

        public TaskbarApplication App { get; set; }

        public NotificationForm(TaskbarApplication app)
        {
            this.InitializeComponent();

            this.App = app;

            this.fadeoutTimer = new Timer
            {
                Interval = 4000
            };
            this.fadeoutTimer.Tick += this.OnTimerTick;

            this.NotificationClosing = false;

            if (this.App != null)
                this.App.CloseNotifications += this.OnParentCloseNotifications;
        }

        public NotificationForm(TaskbarApplication app, string label) : this(app)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            this.Height = 40;

            this.CopyLinkButton.Visible = false;
            this.CopyLinkButton.Enabled = false;

            this.OpenLinkButton.Padding = new Padding(0);

            this.OpenLinkButton.Font = new Font(this.OpenLinkButton.Font.FontFamily, 11, FontStyle.Bold);

            this.OpenLinkButton.Text = label;

            this.OpenLinkButton.Dock = DockStyle.Fill;
            this.OpenLinkButton.BackColor = Color.LightGray;
            this.OpenLinkButton.TextAlign = ContentAlignment.MiddleCenter;

            this.OpenLinkButton.FlatAppearance.BorderColor = Color.Gray;
            this.OpenLinkButton.FlatAppearance.MouseDownBackColor = Color.LightGray;
            this.OpenLinkButton.FlatAppearance.MouseOverBackColor = Color.LightGray;

            int textWidth = TextRenderer.MeasureText(label, this.OpenLinkButton.Font).Width;
            if (textWidth > this.Width)
            {
                this.OpenLinkButton.Text = String.Concat(label.Substring(0, 40), "…");
            }
        }

        public NotificationForm(TaskbarApplication app, QuickAction quickAction) : this(app)
        {
            if (quickAction == null)
                throw new ArgumentNullException(nameof(quickAction));

            this.OpenLinkButton.Text = quickAction.OpenLabel;

            if (quickAction.CanCopy)
            {
                this.CopyLinkButton.Text = String.Format(CultureInfo.CurrentCulture, "Copy link");
            }
            else
            {
                this.CopyLinkButton.Visible = false;
                this.CopyLinkButton.Enabled = false;
                this.OpenLinkButton.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            this.fadeoutTimer.Dispose();

            base.Dispose(disposing);
        }

        private void OnParentCloseNotifications(object? sender, EventArgs e) => this.Fadeout();

        public int StartPosY
        {
            get;
            set;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            this.fadeoutTimer.Stop();
            this.Fadeout();
        }

        public async void FadeIn()
        {
            this.fadingIn = true;

            //Object is not fully invisible. Fade it in
            while (!this.IsDisposed && this.Opacity < 1.0 && this.fadingIn && !this.NotificationClosing)
            {
                await Task.Delay(10).ConfigureAwait(true);
                this.Opacity += 0.05;
            }

            if (!this.IsDisposed)
            {
                this.Opacity = 1; //make fully visible

                this.fadingIn = false;

                this.fadeoutTimer.Start();
            }
        }

        public async void Fadeout()
        {
            this.NotificationClosing = true;

            this.fadingIn = false;

            //Object is fully visible. Fade it out
            while (!this.IsDisposed && this.Opacity > 0.0)
            {
                await Task.Delay(5).ConfigureAwait(true);
                this.Opacity -= 0.05;
            }

            if (!this.IsDisposed)
            {
                this.Opacity = 0; //make fully invisible
                this.Close();
            }
        }

        public void ShowInactiveTopmost()
        {
            NativeMethods.ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
            NativeMethods.SetWindowPos(this.Handle.ToInt32(), HWND_TOPMOST, Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10, this.StartPosY, this.Width, this.Height, SWP_NOACTIVATE);
        }

        private void OnCopyLinkClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && CopyLinkClick != null)
            {
                CopyLinkClick(sender, e);
            }

            this.Fadeout();
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (!this.NotificationClosing && !this.fadingIn)
            {
                this.fadeoutTimer.Stop();

                this.Opacity = 1;
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (!this.NotificationClosing && !this.fadingIn)
            {
                this.fadeoutTimer.Start();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ExStyle |= 0x80;
                return p;
            }
        }

        private void CopyLinkLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && CopyLinkClick != null)
            {
                CopyLinkClick(sender, e);
            }

            this.Fadeout();
        }

        private void OpenLinkLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.App.CloseNotifications -= this.OnParentCloseNotifications;

                RightClick?.Invoke(this, e);
            }
            else if (e.Button == MouseButtons.Left && OpenClick != null)
            {
                OpenClick(sender, e);
            }

            this.Fadeout();
        }
    }
}
