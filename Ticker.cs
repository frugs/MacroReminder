using System;
using System.Diagnostics;
using System.Threading;

namespace MacroReminder
{
    public class Ticker
    {
        public Action<long> OnSmallTick = l => { };

        public Action<long> OnBigTick = l => { };
        
        private readonly long _smallTickIntervalMs;

        private Thread _timerThread;
        private bool _started;
        public Ticker(long smallTickIntervalMs, long bigTickIntervalMs)
        {
            _smallTickIntervalMs = smallTickIntervalMs;
            BigTickIntervalMs = bigTickIntervalMs;
        }
        
        public long BigTickIntervalMs { get; set; }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _timerThread = new Thread(TrackTime);
            _timerThread.Start();
        }

        public void Stop()
        {
            _started = false;
            _timerThread.Abort();
            _timerThread = null;
        }
        
        private void TrackTime()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            long nextSmallTick = _smallTickIntervalMs;
            long nextBigTick = BigTickIntervalMs;
            bool aborted = false;
            while (!aborted)
            {
                try
                {
                    Thread.Sleep(500);
                }
                catch (ThreadInterruptedException)
                {
                    continue;
                }
                catch (ThreadAbortException)
                {
                    aborted = true;
                    continue;
                }

                if (stopwatch.ElapsedMilliseconds < nextSmallTick)
                {
                    continue;
                }

                nextSmallTick += _smallTickIntervalMs;

                OnSmallTick(stopwatch.ElapsedMilliseconds);

                if (stopwatch.ElapsedMilliseconds < nextBigTick)
                {
                    continue;
                }

                nextBigTick += BigTickIntervalMs;

                OnBigTick(stopwatch.ElapsedMilliseconds);
            }
        }
    }
}