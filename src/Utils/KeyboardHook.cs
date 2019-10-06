namespace ClipboardManager
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp
    /// </summary>
    public sealed class KeyboardHook : IDisposable
    {
        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        internal sealed class Window : NativeWindow, IDisposable
        {
            private const int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs>? KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        internal Window _window = new Window();
        private int _currentId;

        // register the event of the inner native window.
        public KeyboardHook()
        {
            this._window.KeyPressed += (object? sender, KeyPressedEventArgs args) => KeyPressed?.Invoke(this, args);
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotkey(ModifierKeys modifier, Keys key)
        {
            // register the hot key.
            if (!NativeMethods.RegisterHotKey(this._window.Handle, ++this._currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs>? KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = this._currentId; i > 0; i--)
            {
                NativeMethods.UnregisterHotKey(this._window.Handle, i);
            }

            // dispose the inner native window.
            this._window.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            this.Modifier = modifier;
            this.Key = key;
        }

        public ModifierKeys Modifier { get; }

        public Keys Key { get; }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : int
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
