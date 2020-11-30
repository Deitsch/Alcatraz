using RegistrationServer.Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationServer.Spread
{
    class LobbyDto
    {
        public string OriginalSender { get; set; }
        public LobbyInfo LobbyInfo { get; set; }
    }
}
