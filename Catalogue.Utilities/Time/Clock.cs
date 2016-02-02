using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Utilities.Time
{
    /// <summary>
    /// System clock whose time source can be statically configured.
    /// (This allows deterministic time values to be returned for unit tests.)
    /// </summary>
    public static class Clock
    {
        static Clock()
        {
            // by default, use the system clock
            CurrentUtcDateTimeGetter = () => DateTime.UtcNow;
        }

        /// <summary>
        /// Set this function to one that returns whatever value you want.
        /// </summary>
        public static Func<DateTime> CurrentUtcDateTimeGetter { get; set; }

        /// <summary>
        /// Returns the current UTC time according to the system clock or the configured time source.
        /// </summary>
        public static DateTime NowUtc { get { return CurrentUtcDateTimeGetter(); } }
    }
}
