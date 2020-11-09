using RegistrationServer.Spread.Interface;
using spread;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RegistrationServer.Spread
{
    public class SpreadConn : ISpreadConn
    {
        SpreadConnection _spreadConnection { get; }
        SpreadGroup _spreadGroup { get; }

        public SpreadConn()
        {
            try
            {
                _spreadConnection = new SpreadConnection();
                Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
                _spreadGroup = JoinGroup(ConfigFile.SPREAD_GROUP_NAME);

                recThread rt = new recThread(_spreadConnection);
                Thread rtt = new Thread(new ThreadStart(rt.run));
                rtt.Start();
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

        //Parameter user must be unique!
        public void Connect(string address, int port, string user, bool priority, bool groupMembership)
        {
            try
            {
                _spreadConnection.Connect(address, port, user, priority, groupMembership);
                Console.WriteLine($"Log: connected with: {address}; {port}; {user}; {priority}; {groupMembership}");
            }
            catch (SpreadException e)
            {
                Console.Error.WriteLine("There was an error connecting to the daemon.");
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }

        public SpreadGroup JoinGroup(string groupName)
        {
            SpreadGroup spreadGroup = new SpreadGroup();
            spreadGroup.Join(_spreadConnection, groupName);
            return spreadGroup;
        }

        public void SendMessage(string message)
        {
            SendMessage(message, _spreadGroup);
        }

        public void SendMessage(string message, SpreadGroup spreadGroup)
        {
            SpreadMessage msg = new SpreadMessage();
            msg.Data = Encoding.ASCII.GetBytes(message);
            msg.AddGroup(spreadGroup);
            msg.IsSafe = true;
            _spreadConnection.Multicast(msg);
        }

        public string ReceiveMessage()
        {
            SpreadMessage spreadMessage = _spreadConnection.Receive();

            if (spreadMessage.IsMembership)
            {
                MembershipInfo info = spreadMessage.MembershipInfo;
                if (info.IsCausedByJoin)
                    return "Joined: " + info.Joined;
                else if (info.IsCausedByLeave)
                    return $"Left: {info.Left}";
                else if (info.IsCausedByDisconnect)
                    return $"Disconnected: {info.Disconnected}";
                else
                    return $"Memership change";
            }
            else
            {
                return Encoding.ASCII.GetString(spreadMessage.Data);
            }
        }
    }
}
