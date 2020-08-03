using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace MacroReminder
{
    public partial class MainForm : Form
    {
        private const long DefaultSmallTickIntervalMs = 1000;
        private const long DefaultBigTickIntervalMs = 5000;

        private Ticker _ticker;
        private Sc2Service _sc2Service;
        private MacroReminder _macroReminder;
        private HotKeyManager _hotKeyManager;
        private NotificationPlayer _notificationPlayer;
        private MacroReminderSettings _macroReminderSettings;

        private bool _started;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            _hotKeyManager?.HandleWindowMessage(m);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _macroReminderSettings = new MacroReminderSettings();
            _ticker = new Ticker(DefaultSmallTickIntervalMs, DefaultBigTickIntervalMs);
            _sc2Service = new Sc2Service();
            _macroReminder = new MacroReminder(_sc2Service, _macroReminderSettings);
            _hotKeyManager = new HotKeyManager(Handle);
            _notificationPlayer = new NotificationPlayer();
            _hotKeyManager = new HotKeyManager(Handle);

            if (!string.IsNullOrEmpty(_macroReminderSettings.CustomNotificationSound))
            {
                SafeSetCustomNotificationSoundFromPath(_macroReminderSettings.CustomNotificationSound);
            }

            _ticker.OnSmallTick += () => Invoke(new Action(UpdateTimerValue));
            _ticker.OnSmallTick += _macroReminder.SmallTick;
            _ticker.OnBigTick += _macroReminder.BigTick;
            _macroReminder.OnReminder += () => Invoke(new Action(_notificationPlayer.PlayNotification));

            _hotKeyManager.OnHotKey += () => { enabledCheckBox.Checked = !_started; };
            _hotKeyManager.RegisterHotKey();

            delayTimeSecondsTextBox.Text = (_macroReminderSettings.DelayMs / 1000).ToString();
            intervalTimeSecondsTextBox.Text = (_macroReminderSettings.IntervalMs / 1000).ToString();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            _hotKeyManager.UnregisterHotKey();
            _ticker.Stop();
        }

        private bool SafeSetCustomNotificationSoundFromPath(string path)
        {
            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    _notificationPlayer.SetCustomNotificationSound(fileStream);
                    return true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"Failed to load custom sound", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateTimerValue()
        {
            var timeSpan = TimeSpan.FromMilliseconds(_sc2Service.EstimateGameTime());
            timerValueLabel.Text = timeSpan.ToString(@"m\:ss");
        }

        private void StartTicker(object sender)
        {
            if (!long.TryParse(delayTimeSecondsTextBox.Text, out var delayTimeSeconds) || delayTimeSeconds <= 0)
            {
                MessageBox.Show(@"Invalid delay!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!long.TryParse(intervalTimeSecondsTextBox.Text, out var intervalTimeSeconds) ||
                intervalTimeSeconds <= 0)
            {
                MessageBox.Show(@"Invalid interval!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _started = true;
            if (enabledCheckBox != sender)
            {
                enabledCheckBox.Checked = true;
            }

            timerValueLabel.Text = @"0:00";
            long delayTimeMs = delayTimeSeconds * 1000;
            long intervalTimeMs = intervalTimeSeconds * 1000;
            _macroReminderSettings.DelayMs = delayTimeMs;
            _macroReminderSettings.IntervalMs = intervalTimeMs;
            _ticker.BigTickIntervalMs = intervalTimeMs;
            _ticker.Start();

            _notificationPlayer.PlayStartNotification();
        }

        private void StopTicker(object sender)
        {
            _started = false;
            _notificationPlayer.PlayStopNotification();
            if (sender != enabledCheckBox)
            {
                enabledCheckBox.Checked = false;
            }

            _ticker.Stop();
        }

        private void StartStopTicker(object sender)
        {
            if (!_started)
            {
                StartTicker(sender);
            }
            else
            {
                StopTicker(sender);
            }
        }

        private void pickSoundButton_Click(object sender, EventArgs e)
        {
            var openFileDialogue = new OpenFileDialog();
            if (openFileDialogue.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (SafeSetCustomNotificationSoundFromPath(openFileDialogue.FileName))
            {
                _macroReminderSettings.CustomNotificationSound = openFileDialogue.FileName;
            }
        }

        private void resetSoundButton_Click(object sender, EventArgs e)
        {
            _notificationPlayer.SetDefaultNotificationSound();
        }

        private void enabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StartStopTicker(sender);
        }
    }
}