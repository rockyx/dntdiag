using System;
using System.Diagnostics;

namespace DNT.Diag
{
    public class Timer
    {
        private const double NANO = 1 * 1000 * 1000 * 1000;
        private const double MICRO = 1 * 1000 * 1000;
        private const double MILLI = 1 * 1000;
        static readonly double TICKS_PER_NANO = Stopwatch.Frequency / NANO;
        static readonly double TICKS_PER_MICRO = Stopwatch.Frequency / MICRO;
        static readonly double TICKS_PER_MILLI = Stopwatch.Frequency / MILLI;
        static readonly double NANO_PER_TICKS = NANO / Stopwatch.Frequency;
        static readonly double MICRO_PER_TICKS = MICRO / Stopwatch.Frequency;
        static readonly double MILLI_PER_TICKS = MILLI / Stopwatch.Frequency;
        private long ticks;

        public Timer()
        {
            ticks = 0;
        }

        public Timer(long ticks)
        {
            this.ticks = ticks;
        }

        public TimeSpan ToTimeSpan()
        {
            return TimeSpan.FromTicks(ticks);
        }

        public long Nanoseconds
        {
            get { return (long)(ticks * NANO_PER_TICKS); }
            set { ticks = (long)(value * TICKS_PER_NANO); }
        }

        public long Microseconds
        {
            get { return (long)(ticks * MICRO_PER_TICKS); }
            set { ticks = (long)(value * TICKS_PER_MICRO); }
        }

        public long Milliseconds
        {
            get { return (long)(ticks * MILLI_PER_TICKS); }
            set { ticks = (long)(value * TICKS_PER_MILLI); }
        }

        public long Seconds
        {
            get { return (long)(ticks / Stopwatch.Frequency); }
            set { ticks = (long)(value * Stopwatch.Frequency); }
        }

        public long Ticks
        {
            get { return ticks; }
            set { ticks = value; }
        }

        public static Timer FromMicroseconds(long time)
        {
            return new Timer((long)(time * TICKS_PER_MICRO));
        }

        public static Timer FromMilliseconds(long time)
        {
            return new Timer((long)(time * TICKS_PER_MILLI));
        }

        public static Timer FromSeconds(long time)
        {
            return new Timer(time * Stopwatch.Frequency);
        }
    }
}

