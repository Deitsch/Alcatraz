using RegistrationServer.Spread.Interface;
using System;

namespace RegistrationServer
{
    public class SpreadManager
    {
        private readonly ISpreadConnection _spreadConn;

        public SpreadManager(ISpreadConnection spreadConn)
        {
            _spreadConn = spreadConn;
        }

        public void RunSpread()
        {
            _spreadConn.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
            _spreadConn.Run();
        }
    }
}
