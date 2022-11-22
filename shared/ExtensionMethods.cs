using System;
using System.Collections.Generic;

namespace Nerdostat.Shared
{
    public static class ExtensionMethods
    {
        public static long ToEpochSeconds (this DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime FromEpochSeconds (this long epochSeconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(epochSeconds);
        }
    }
}
