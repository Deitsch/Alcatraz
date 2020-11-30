using RegistrationServer.Spread.Interface;
using spread;
using System;

namespace RegistrationServer.Spread
{
    class SpreadConnectionWrapper : ISpreadConnectionWrapper
    {
        public SpreadConnection SpreadConnection { get; }
        public SpreadGroup SpreadGroup { get; private set; }
        public string UserName { get; private set; }

        public SpreadConnectionWrapper()
        {
            SpreadConnection = new SpreadConnection();
        }

        //Parameter user must be unique!
        public void Connect(string address, int port, string user, bool priority, bool groupMembership)
        {
            try
            {
                SpreadConnection.Connect(address, port, user, priority, groupMembership);
                Console.WriteLine($"Log: connected with: {address}; {port}; {user}; {priority}; {groupMembership}");
                SpreadGroup = JoinGroup(ConfigFile.SPREAD_GROUP_NAME);
                UserName = user.Substring(0, 8);
            }
            catch (SpreadException e)
            {
                Console.Error.WriteLine("There was an error connecting to the daemon.");
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't find the daemon " + ConfigFile.SPREAD_ADDRESS);
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }

        private SpreadGroup JoinGroup(string groupName)
        {
            SpreadGroup spreadGroup = new SpreadGroup();
            spreadGroup.Join(SpreadConnection, groupName);
            return spreadGroup;
        }
    }
}
