﻿using System.Net;

using H2MLauncher.Core.Models;

namespace H2MLauncher.Core.Services
{
    public class ServerInfoEventArgs<TServer> : EventArgs
         where TServer : IServerConnectionDetails
    {
        public required GameServerInfo ServerInfo { get; init; }

        public required TServer Server { get; init; }
    }
}