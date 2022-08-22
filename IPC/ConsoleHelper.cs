using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPC
{
    public class ConsoleHelper
    {
        private readonly string ServerPipeName;
        private readonly string ServerPipeNameToConnectTo;
        private readonly string ConsoleName;
        private object _consoleLock = new();
        private NamedPipeServerStream _serverPipe;
        private NamedPipeClientStream _clientPipe;
        private static Mutex _mutex1 = new Mutex(false, "Pipe1");
        private static Mutex _mutex2 = new Mutex(false, "Pipe2");

        public ConsoleHelper(string consoleName, string pipeName, string serverPipeNameToConnectTo)
        {
            ConsoleName = consoleName;
            ServerPipeName = pipeName;
            ServerPipeNameToConnectTo = serverPipeNameToConnectTo;
        }

        public void Startup()
        {
            Console.WriteLine($"{ConsoleName} starting up...");
            Console.WriteLine("Initializing server pipe...");
            _serverPipe = NamedPipeServerService.GenerateNamedPipeServerStream(ServerPipeName);
            if (ServerPipeName == "Pipe1")
            {
                _mutex1.Close();
            }
            else if(ServerPipeName == "Pipe2")
            {
                _mutex2.Close();
            }

            Console.WriteLine($"{ServerPipeName} initialized and connected.");

            ConnectToServerPipe(ServerPipeNameToConnectTo);
        }

        public void WaitForConnection()
        {
            Console.WriteLine("Waiting for connection...");
            _serverPipe.WaitForConnection();
            Console.WriteLine("Connection established.");

            Console.WriteLine(Environment.NewLine + Environment.NewLine + "Write the message that you wish to send over the pipe and press ENTER to send.");
        }

        public void BeginWrite()
        {
            while (true)
            {
                var s = Console.ReadLine();
                if (s != null)
                {
                    lock (_consoleLock)
                    {
                        _serverPipe.Write(s);
                    }
                }
            }
        }

        public void BeginRead()
        {
            Task.Run(() =>
            {
                var m = _clientPipe.Read();

                if (!string.IsNullOrEmpty(m))
                {
                    lock (_consoleLock)
                    {
                        Console.WriteLine($"Message from {ServerPipeNameToConnectTo}: {m}");
                    }
                }

                Task.Delay(100).Wait();
            });
        }

        private void ConnectToServerPipe(string serverPipeName)
        {
            if (serverPipeName != ServerPipeName)
            {
                Console.WriteLine($"Connecting to server pipe '{serverPipeName}'...");
                var name = ServerPipeName + "Client";
                var mutex = ServerPipeNameToConnectTo == "Pipe1" ? _mutex1 : _mutex2;
                if (!mutex.WaitOne(0, false))
                {
                    _clientPipe = NamedPipeServerService.GenerateNamedPipeClientStream(serverPipeName, name);
                    _clientPipe.Connect();
                }
                //mutex.Close();
                Console.WriteLine("Connected.");
            }
        }
    }
}
