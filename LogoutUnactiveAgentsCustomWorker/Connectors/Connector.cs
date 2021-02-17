using LogoutUnactiveAgentsCustomWorker.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using TCX.Configuration;

namespace LogoutUnactiveAgentsCustomWorker.Connectors
{
    public class Connector
    {
        public static PbxConnectionStatus _connectionStatus;

        private readonly TCXSetup _config;

        public Connector(IConfiguration config)
        {
            _config = config.GetSection("3CXConnectionString").Get<TCXSetup>();
        }

        public bool Connect()
        {
            _connectionStatus = PbxConnectionStatus.NotConnected;
            bool result = true;

            try
            {
                if (_config.Host.Trim().Length > 0)
                {
                    PhoneSystem.CfgServerHost = _config.Host;
                }

                var applicationName = "LogoutInActiveAgents";
                PhoneSystem.ApplicationName = applicationName;
                PhoneSystem.CfgServerPort = _config.Port;
                PhoneSystem.CfgServerUser = _config.User;
                PhoneSystem.CfgServerPassword = _config.Password;


                Tenant t = PhoneSystem.Root.GetTenant();
                if (t == null)
                {

                    result = false;
                }
            }
            catch (Exception ex)
            {

                result = false;
            }

            if (result)
            {
                _connectionStatus = PbxConnectionStatus.Connected;
            }

            return result;
        }
    }
}
