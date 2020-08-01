using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MacroReminder
{
    public class HotKeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint ModControl = 0x0002;
        private const uint ModShift = 0x0004;
        private const uint WmHotKey = 0x0312;

        private const int HotKeyId = 1;

        private readonly IntPtr _handle;

        private bool _registered;

        public HotKeyManager(IntPtr handle)
        {
            _handle = handle;
        }

        public Action OnHotKey = () => { };

        public void RegisterHotKey()
        {
            if (_registered)
            {
                return;
            }

            var success = RegisterHotKey(_handle, HotKeyId, (int) (ModControl | ModShift), (int) Keys.C);
            if (!success)
            {
                MessageBox.Show(@"Failed to register hot key!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _registered = true;
        }

        public void UnregisterHotKey()
        {
            if (!_registered)
            {
                return;
            }

            var success = UnregisterHotKey(_handle, HotKeyId);
            if (!success)
            {
                MessageBox.Show(@"Failed to unregister hot key!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _registered = false;
        }

        public void HandleWindowMessage(Message message)
        {
            if (message.Msg != WmHotKey)
            {
                return;
            }

            var id = message.WParam.ToInt32();
            if (id != HotKeyId)
            {
                return;
            }

            OnHotKey();
        }
    }
}