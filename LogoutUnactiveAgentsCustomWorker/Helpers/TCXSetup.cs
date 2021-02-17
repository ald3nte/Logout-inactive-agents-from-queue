using System;
using System.Collections.Generic;
using System.Text;

namespace LogoutUnactiveAgentsCustomWorker.Helpers
{
    public struct TCXSetup
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

    }
}
