using System;

namespace MacroReminder
{
    public class MacroReminder
    {
        public Action OnReminder = () => {};

        private readonly Sc2Service _sc2Service;
        private readonly MacroReminderSettings _macroReminderSettings;

        private bool _started;
        private long _nextReminder;

        public MacroReminder(Sc2Service sc2Service, MacroReminderSettings macroReminderSettings)
        {
            _sc2Service = sc2Service;
            _macroReminderSettings = macroReminderSettings;
        }

        public void SmallTick()
        {
            if (!_started)
            {
                return;
            }

            if (_sc2Service.EstimateGameTime() < _nextReminder)
            {
                return;
            }
            
            OnReminder();
            while (_nextReminder < _sc2Service.EstimateGameTime())
            {
                _nextReminder += _macroReminderSettings.IntervalMs;
            }
        }

        public void BigTick()
        {
            if (!_sc2Service.HasGameStarted())
            {
                _started = false;
                return;
            }

            if (!_started)
            {
                _nextReminder = _macroReminderSettings.DelayMs;
                _started = true;
            }

            _sc2Service.FetchGameTime();
        }
    }
}