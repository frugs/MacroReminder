using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using MacroReminder.Properties;

namespace MacroReminder
{
    public partial class MainForm : Form
    {
        private const long DefaultSmallTickIntervalMs = 1000;
        
        private const long DefaultBigTickIntervalMs = 30 * 1000;

        private const long DefaultDelayTimeMs = 5 * 60 * 1000;

        private Ticker _ticker;
        private NotificationPlayer _notificationPlayer;
        private MacroReminder _macroReminder;
        private bool _started;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _ticker = new Ticker(DefaultSmallTickIntervalMs, DefaultBigTickIntervalMs);
            _notificationPlayer = new NotificationPlayer();
            _macroReminder = new MacroReminder(DefaultDelayTimeMs);
            _macroReminder.OnReminder += _notificationPlayer.PlayNotification;
            _ticker.OnSmallTick += elapsedTimeMs => Invoke(new Action(() => UpdateTimerValue(elapsedTimeMs)));
            _ticker.OnBigTick += elapsedTimeMs => _macroReminder.BigTick(elapsedTimeMs);

            delayTimeSecondsTextBox.Text = (DefaultDelayTimeMs / 1000).ToString();
            intervalTimeSecondsTextBox.Text = (DefaultBigTickIntervalMs / 1000).ToString();
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
            _macroReminder.DelayTimeMs = delayTimeSeconds * 1000;
            _ticker.BigTickIntervalMs = intervalTimeSeconds * 1000;
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