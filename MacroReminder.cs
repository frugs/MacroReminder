using System;

namespace MacroReminder
{
    public class MacroReminder
    {

        public Action OnReminder = () => {};

        public MacroReminder(long delayTimeMs)
        {
            DelayTimeMs = delayTimeMs;
        }
        
        public long DelayTimeMs { get; set; }
        
        public void BigTick(long elapsedTimeMs)
        {
            if (elapsedTimeMs > DelayTimeMs)
            {
                OnReminder();
            }
        }
    }
}