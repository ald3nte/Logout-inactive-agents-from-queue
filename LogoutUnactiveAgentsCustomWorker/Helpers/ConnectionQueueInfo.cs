using System;
using System.Collections.Generic;
using System.Text;
using TCX.Configuration;

namespace LogoutUnactiveAgentsCustomWorker.Helpers
{
    public class ConnectionQueueInfo
    {
        public int callId { get; set; }
        public Queue queue { get; set; }

        public override string ToString()
        {
            return callId.ToString() + ", " + queue.Number.ToString();
        }
    }
}
