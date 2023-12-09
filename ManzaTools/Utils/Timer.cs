using CounterStrikeSharp.API.Modules.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManzaTools.Utils
{
    public class Timer
    {
        public static CounterStrikeSharp.API.Modules.Timers.Timer CreateTimer(float interval, Action callback, TimerFlags? flags = null)
        {
            var timer = new CounterStrikeSharp.API.Modules.Timers.Timer(interval, callback, flags ?? 0);
            return timer;
        }
    }
}
