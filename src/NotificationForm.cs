using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    public partial class NotificationForm : Form
    {
        public event MouseEventHandler OpenClick;
        public event MouseEventHandler CopyLinkClick;
        public event MouseEventHandler RightClick;

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        private Timer fadeoutTimer;

        private bool fadingIn = false;

        public string ApplicationName { get; set; }

        public string QuickActionValue { get; set; }

        public bool NotificationClosing { get; set; }

        public TaskbarApplication App { get; set; }

        /// <summary>
        /// Constructeur de base
        /// </summary>
        /// <param name="app">L'application parent</param>
        public NotificationForm(TaskbarApplication app)
        {
            InitializeComponent();

            App = app;

            fadeoutTimer = new Timer
            {
                Interval = 4000
            };
            fadeoutTimer.Tick += OnTimerTick;

            NotificationClosing = false;

            App.CloseNotifications += OnParentCloseNotifications;
        }

        /// <summary>
        /// Constructeur pour affichage d'un message d'info
        /// </summary>
        /// <param name="app"></param>
        /// <param name="label"></param>
        public NotificationForm(TaskbarApplication app, string label) : this(app)
        {
            Height = 40;

            CopyLinkButton.Visible = false;
            CopyLinkButton.Enabled = false;

            OpenLinkButton.Padding = new Padding(0);

            OpenLinkButton.Font = new Font(OpenLinkButton.Font.FontFamily, 11, FontStyle.Bold);

            OpenLinkButton.Text = label;

            OpenLinkButton.Dock = DockStyle.Fill;
            OpenLinkButton.BackColor = Color.LightGray;
            OpenLinkButton.TextAlign = ContentAlignment.MiddleCenter;

            OpenLinkButton.FlatAppearance.BorderColor = Color.Gray;
            OpenLinkButton.FlatAppearance.MouseDownBackColor = Color.LightGray;
            OpenLinkButton.FlatAppearance.MouseOverBackColor = Color.LightGray;

            int textWidth = TextRenderer.MeasureText(label, OpenLinkButton.Font).Width;
            if (textWidth > this.Width)
            {
                OpenLinkButton.Text = String.Concat(label.Substring(0, 40), "…");
            }
        }

        /// <summary>WW
        /// Constructeur notification launcher
        /// </summary>
        /// <param name="app">L'application parent</param>
        /// <param name="quickAction">La Quick Action associée</param>
        /// <param name="values">Les paramètres extrais</param>
        public NotificationForm(TaskbarApplication app, QuickAction quickAction) : this(app)
        {
            OpenLinkButton.Text = quickAction.OpenLabel;

            if (quickAction.CanCopy)
            {
                CopyLinkButton.Text = String.Format("Copy link");
            }
            else
            {
                CopyLinkButton.Visible = false;
                CopyLinkButton.Enabled = false;
                OpenLinkButton.Dock = DockStyle.Fill;
            }
        }

        private void OnParentCloseNotifications(object sender, EventArgs e) => Fadeout();

        public int StartPosY
        {
            get;
            set;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            fadeoutTimer.Stop();
            Fadeout();
        }

        public async void FadeIn()
        {
            fadingIn = true;

            //Object is not fully invisible. Fade it in
            while (!this.IsDisposed && this.Opacity < 1.0 && fadingIn && !NotificationClosing)
            {
                await Task.Delay(10);
                Opacity += 0.05;
            }

            if (!this.IsDisposed)
            {
                Opacity = 1; //make fully visible

                fadingIn = false;

                fadeoutTimer.Start();
            }
        }

        public async void Fadeout()
        {
            NotificationClosing = true;

            fadingIn = false;

            //Object is fully visible. Fade it out
            while (!this.IsDisposed && this.Opacity > 0.0)
            {
                await Task.Delay(5);
                Opacity -= 0.05;
            }

            if (!this.IsDisposed)
            {
                Opacity = 0; //make fully invisible
                Close();
            }
        }

        public void ShowInactiveTopmost()
        {
            NativeMethods.ShowWindow(Handle, SW_SHOWNOACTIVATE);
            NativeMethods.SetWindowPos(Handle.ToInt32(), HWND_TOPMOST, Screen.PrimaryScreen.WorkingArea.Width - Width - 10, StartPosY, this.Width, this.Height, SWP_NOACTIVATE);
        }

        private void OnCopyLinkClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && CopyLinkClick != null)
            {
                CopyLinkClick(sender, e);
            }

            Fadeout();
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (!NotificationClosing && !fadingIn)
            {
                fadeoutTimer.Stop();

                Opacity = 1;
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (!NotificationClosing && !fadingIn)
            {
                fadeoutTimer.Start();
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

            Fadeout();
        }

        private void OpenLinkLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                App.CloseNotifications -= OnParentCloseNotifications;

                RightClick?.Invoke(this, e);
            }
            else if (e.Button == MouseButtons.Left && OpenClick != null)
            {
                OpenClick(sender, e);
            }

            Fadeout();
        }
    }
}
