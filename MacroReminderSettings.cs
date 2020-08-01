namespace MacroReminder
{
    public class MacroReminderSettings
    {
        public long DelayMs
        {
            get => Properties.Settings.Default.DelayMs;
            set
            {
                Properties.Settings.Default.DelayMs = value;
                Properties.Settings.Default.Save();
            }
        }

        public long IntervalMs
        {
            get => Properties.Settings.Default.IntervalMs;
            set
            {
                Properties.Settings.Default.IntervalMs = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}