using System;
using com.fpnn;
using System.Threading;

namespace ConnectionEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ConnectionEvents <ip> <port>");
                return;
            }

            TCPClient client = new TCPClient(args[0], Int32.Parse(args[1]));
            client.SetConnectionConnectedDelegate((Int64 connectionId, string endpoint, bool connected) => {
                if (connected)
                    Console.WriteLine("Connected event triggered. Remote endpoint is {0}, connection id is {1}", endpoint, connectionId);
                else
                    Console.WriteLine("Connect {0} failed.", endpoint);
            });

            client.SetConnectionCloseDelegate((Int64 connectionId, string endpoint, bool causedByError) => {
                Console.WriteLine("Connection {0} with {1} is closed by error {2}", connectionId, endpoint, causedByError);
            });

            if (client.SyncConnect())
            {
                Thread.Sleep(1000);
                client.Close();
                Thread.Sleep(1000);
            }
        }
    }
}
