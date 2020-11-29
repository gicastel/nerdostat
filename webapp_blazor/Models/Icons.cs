using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorClient.Models
{
    public static class ConnectionStatusIcon
    {
        public static string ON => "icons/wifi_on-24px.svg";
        public static string OFF => "icons/wifi_off-24px.svg";
    }

    public static class HeaterStatusIcon
    {
        public static string ON => "icons/flash_on-24px.svg";
        public static string OFF => "icons/flash_off-24px.svg";
    }
}
