using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace MacroReminder
{
    public partial class MainForm : Form
    {
        private const long DefaultSmallTickIntervalMs = 1000;

        private Ticker _ticker;
        private NotificationPlayer _notificationPlayer;
        private MacroReminder _macroReminder;
        private MacroReminderSettings _macroReminderSettings;
        private HotKeyManager _hotKeyManager;
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
            _ticker = new Ticker(DefaultSmallTickIntervalMs, _macroReminderSettings.IntervalMs);
            _notificationPlayer = new NotificationPlayer();
            _macroReminder = new MacroReminder(_macroReminderSettings.DelayMs);
            _hotKeyManager = new HotKeyManager(Handle);

            if (!string.IsNullOrEmpty(_macroReminderSettings.CustomNotificationSound))
            {
                SafeSetCustomNotificationSoundFromPath(_macroReminderSettings.CustomNotificationSound);
            }

            _macroReminder.OnReminder += () => Invoke(new Action(_notificationPlayer.PlayNotification));
            _ticker.OnSmallTick += elapsedTimeMs => Invoke(new Action(() => UpdateTimerValue(elapsedTimeMs)));
            _ticker.OnBigTick += elapsedTimeMs => _macroReminder.BigTick(elapsedTimeMs);

            _hotKeyManager.OnHotKey += StartStop;
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

        private void UpdateTimerValue(long elapsedTimeMs)
        {
            var timeSpan = TimeSpan.FromMilliseconds(elapsedTimeMs);
            timerValueLabel.Text = timeSpan.ToString(@"m\:ss");
        }

        private void StartTicker()
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
            startStopButton.Text = @"Stop";
            timerValueLabel.Text = @"0:00";
            long delayTimeMs = delayTimeSeconds * 1000;
            long intervalTimeMs = intervalTimeSeconds * 1000;
            _macroReminderSettings.DelayMs = delayTimeMs;
            _macroReminderSettings.IntervalMs = intervalTimeMs;
            _macroReminder.DelayTimeMs = delayTimeMs;
            _ticker.BigTickIntervalMs = intervalTimeMs;
            _ticker.Start();

            _notificationPlayer.PlayStartNotification();
        }

        private void StopTicker()
        {
            _started = false;
            _notificationPlayer.PlayStopNotification();
            startStopButton.Text = @"Start";
            _ticker.Stop();
        }

        private void StartStop()
        {
            if (!_started)
            {
                StartTicker();
            }
            else
            {
                StopTicker();
            }
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            StartStop();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (!_started)
            {
                StartTicker();
            }
            else
            {
                StopTicker();
                StartTicker();
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
    }
}