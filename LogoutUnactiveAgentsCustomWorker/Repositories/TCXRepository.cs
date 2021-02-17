using LogoutUnactiveAgentsCustomWorker.Connectors;
using LogoutUnactiveAgentsCustomWorker.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCX.Configuration;

namespace LogoutUnactiveAgentsCustomWorker.Repositories
{
    public class TCXRepository : ITCXRepository
    {
        private readonly ILogger<TCXRepository> _logger;
        private readonly Connector _connection;
        private IList<ConnectionQueueInfo> _connectionsListQueues;
        private IList<ConnectionExtensionInfo> _connectionsListExtensions;
        private string minimumAvailableAgents;
        public TCXRepository(ILogger<TCXRepository> logger, Connector connection,IConfiguration config)
        {
            _logger = logger;
            _connection = connection;
            _connection.Connect();
            _connectionsListQueues = new List<ConnectionQueueInfo>();
            _connectionsListExtensions = new List<ConnectionExtensionInfo>();
            minimumAvailableAgents = config.GetSection("MinimumAvailableAgents").Value;
        }


        public void StartLiteningChanges()
        {
            PhoneSystem.Root.Inserted += new NotificationEventHandler(Root_Inserted);
            PhoneSystem.Root.Deleted += new NotificationEventHandler(Root_Deleted);
        }


        private void Root_Deleted(object sender, NotificationEventArgs e)
        {

            Delete(e);
           
        }
        private void Root_Inserted(object sender, NotificationEventArgs e)
        {
            InsertOrUpdate(e);
        }

        private void Delete(NotificationEventArgs e)
        {
            if (e.ConfObject is ActiveConnection)
            {
                var activCon = e.ConfObject as ActiveConnection;
                _logger.LogDebug("Event:Deleted object ConfObject is Active Conneciton");
                if (activCon.DN is Extension)
                { 
                    var ext = activCon.DN as Extension;
                    _logger.LogDebug("Deleted DN is ext Status: {status}, callId: {callId}", activCon.Status, activCon.CallID);

                    var activeConnection = _connectionsListExtensions.Where(c => c.callId == activCon.CallID && c.ext.Number == ext.Number).FirstOrDefault();

                    if (activeConnection != null)
                    {
                        _logger.LogInformation("Call is ended..");
                        activeConnection.status = activCon.Status;

                        if (activeConnection.status != ConnectionStatus.Connected)
                        {
                            _logger.LogInformation("Call is unanswered, starting the logout process..");

                            LogoutExtensionFromAllQUeues(activeConnection.ext); 
                        }

                        _connectionsListExtensions.Remove(activeConnection);
                    }

                }

            }
        }


        private void InsertOrUpdate(NotificationEventArgs e)
        {

            if (e.ConfObject is ActiveConnection)
            {
                var activCon = e.ConfObject as ActiveConnection;
               
                if (activCon.DN is Queue)
                {

                    AddQueueCallToList(activCon);
                }

                if (activCon.DN is Extension)
                {
                    AddExtensionCallToList(activCon);
                    
                }

            }
        }

        private void AddExtensionCallToList(ActiveConnection activCon)
        {
            var ext = activCon.DN as Extension;
            _logger.LogDebug("Active Conneciton dd is Extension:");
            _logger.LogDebug("Ext num: {num}, status: {status}", ext.Number, activCon.Status); //False

            var activeConnection = _connectionsListQueues.Where(c => c.callId == activCon.CallID).FirstOrDefault();

            if (activeConnection != null && activCon.IsInbound == true)
            {
                _connectionsListExtensions.Add(new ConnectionExtensionInfo { callId = activCon.CallID, queueInfo = activeConnection, ext = ext });
                _logger.LogInformation("Call from queue come to extension..");

            }
        }

        private void AddQueueCallToList(ActiveConnection activCon)
        {

            var queue = activCon.DN as Queue;

            var activeConnection = _connectionsListQueues.Where(c => c.callId == activCon.CallID).FirstOrDefault();

            if (activeConnection == null)
            {
                _logger.LogDebug("New calls to queue");
                _connectionsListQueues.Add(new ConnectionQueueInfo { callId = activCon.CallID, queue = queue });
            }
   
        }

        private bool CheckIfIsMinimumNumberOfAgents(Queue queue)
        {
            _logger.LogInformation("Check if in {quque} is minimum Available agents",queue.Number);
            int AvailableAgents = 0;
            foreach (var agent in queue.QueueAgents)
            {
                if (agent.QueueStatus == QueueStatusType.LoggedIn)
                {
                    var a = agent.DN as Extension;
                    
                    if (a.QueueStatus == QueueStatusType.LoggedIn)
                    {
                        AvailableAgents += 1;
                    }
                    try
                    {
                        if (AvailableAgents > int.Parse(minimumAvailableAgents)) 
                        {
                            _logger.LogInformation("In queue : {queue}, is enough agents to logout inactive agent");
                            return true;
                        }
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("minimumAvailableAgents has a bad value, go to appstettings.json and fix that. Exception: " + e.Message);
                    }
                    
                }
            }
            return false;
        }


        private void LogoutExtensionFromAllQUeues(Extension ex)
        {
            if (ex == null)
            {
                return;
            }

            foreach (var qm in ex.QueueMembership)
            {
                if (CheckIfIsMinimumNumberOfAgents(qm.Queue))
                {
                    _logger.LogInformation("Logout Agent: {agentNum} from quques.", ex.Number);
                    qm.QueueStatus = QueueStatusType.LoggedOut;
                }

            }

            ex.Save();
            ex.Refresh();
        }
    }
}
