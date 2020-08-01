using System;
using System.ComponentModel;
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
        private bool _started;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _macroReminderSettings = new MacroReminderSettings();
            _ticker = new Ticker(DefaultSmallTickIntervalMs, _macroReminderSettings.IntervalMs);
            _notificationPlayer = new NotificationPlayer();
            _macroReminder = new MacroReminder(_macroReminderSettings.DelayMs);
            _macroReminder.OnReminder += _notificationPlayer.PlayNotification;
            _ticker.OnSmallTick += elapsedTimeMs => Invoke(new Action(() => UpdateTimerValue(elapsedTimeMs)));
            _ticker.OnBigTick += elapsedTimeMs => _macroReminder.BigTick(elapsedTimeMs);

            delayTimeSecondsTextBox.Text = (_macroReminderSettings.DelayMs / 1000).ToString();
            intervalTimeSecondsTextBox.Text = (_macroReminderSettings.IntervalMs / 1000).ToString();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            _ticker.Stop();
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
            
            if (!long.TryParse(intervalTimeSecondsTextBox.Text, out var intervalTimeSeconds) || intervalTimeSeconds <= 0)
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
        }

        private void StopTicker()
        {
            _started = false;
            startStopButton.Text = @"Start";
            _ticker.Stop();
        }

        private void startStopButton_Click(object sender, EventArgs e)
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
    }
}