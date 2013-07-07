using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

class ClientInstance
{
    public Socket sClient { get; private set; }
    public String RemoteEndpoint { get; private set; }
    public bool Authed { get; private set; }
    public String Name { get; private set; }

    public ClientInstance(Socket cli)
    {
        this.sClient = cli;
        this.RemoteEndpoint = cli.RemoteEndPoint.ToString();
        this.Name = "Unknown";
    }

    public void setName(string name) {
        this.Name = name;
        this.Authed = true;
    }

}
