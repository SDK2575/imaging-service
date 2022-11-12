using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseImaging.ImagingServices.DataAccess
{
   /// <summary>
   /// Simple Interval Timer for profiling.
   /// </summary>
   public class SimpleTimer
   {
      public enum TimerState
      {
         NotStarted,
         Stopped,
         Started
      }

      TimerState state;
      long ticksAtStart, intervalTicks;
      //static int decimalPlaces;
      static string formatString;
      static bool initialized = false;

      public SimpleTimer()
      {
         if (!initialized)
         {
            //decimalPlaces = (int)Math.Log10(TimeSpan.TicksPerSecond) - 2;
            formatString = string.Format("{{0:F{0}}}", 3);
            initialized = true;
         }

         state = TimerState.NotStarted;
      }

      public void Start()
      {
         state = TimerState.Started;

         ticksAtStart = CurrentTicks;
      }

      public void Stop()
      {
         intervalTicks = CurrentTicks - ticksAtStart;
         state = TimerState.Stopped;
      }

      public float GetSeconds()
      {
         if (state != TimerState.Stopped)
            throw new Exception("Timer is either still running or has not been started");

         return (float)intervalTicks / (float)TimeSpan.TicksPerSecond;
      }

      public float GetMilliSeconds()
      {
         if (state != TimerState.Stopped)
            throw new Exception("Timer is either still running or has not been started");

         return (float)intervalTicks / (float)TimeSpan.TicksPerMillisecond;
      }

      public long GetTicks()
      {
         if (state != TimerState.Stopped)
            throw new Exception("Timer is either still running or has not been started");

         return intervalTicks;
      }

      private long CurrentTicks
      {
         get { return DateTime.Now.Ticks; }
      }

      public override string ToString()
      {
         if (state != TimerState.Stopped)
            return "Interval timer, state: " + state.ToString();

         return string.Format(formatString, GetSeconds());
         //return string.Format( GetSeconds().ToString() );
      }

   }
}
