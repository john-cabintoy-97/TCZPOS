using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Providers
{
    public class ConnectionStateProvider
    {
        public string IpAddress { get; set; } = "";
        public int Port { get; set; } = 8080;
        public string Token { get; set; } = "";
        public bool IsPaired { get; set; } = false;
    }
}
