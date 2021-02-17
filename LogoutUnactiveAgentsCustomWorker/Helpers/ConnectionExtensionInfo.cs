using System;
using System.Collections.Generic;
using System.Text;
using TCX.Configuration;

namespace LogoutUnactiveAgentsCustomWorker.Helpers
{
    public class ConnectionExtensionInfo
    {
        public int callId { get; set; }
        public Extension ext { get; set; }
        public ConnectionQueueInfo queueInfo { get; set; }

        public ConnectionStatus status;
    }
}
