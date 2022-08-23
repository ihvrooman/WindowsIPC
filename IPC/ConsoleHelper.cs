using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _disposed = false;

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
            Console.WriteLine($"{ServerPipeName} initialized and connected.");

            ConnectToServerPipe(ServerPipeNameToConnectTo);
        }

        public void WaitForConnection()
        {
            Console.WriteLine("Waiting for connection...");
            _serverPipe.WaitForConnection();
            Console.WriteLine("Connection established.");

            Console.WriteLine(Environment.NewLine + Environment.NewLine + "Write the message that you wish to send over the pipe and press ENTER to send." + Environment.NewLine + Environment.NewLine + "Type CLOSE to exit the application." + Environment.NewLine + Environment.NewLine);
        }

        public void BeginWrite()
        {
            _writer = new StreamWriter(_serverPipe)
            {
                AutoFlush = true
            };

            while (!_disposed)
            {
                var s = Console.ReadLine();

                if (!_disposed && s != null)
                {
                    lock (_consoleLock)
                    {
                        _writer.WriteLine(s);
                    }

                    if (s.ToLower() == "close")
                    {
                        Dispose();
                    }
                }
            }
        }

        public void BeginRead()
        { 
            _reader = new StreamReader(_clientPipe);
            Task.Run(() =>
            {
                while (!_disposed)
                {
                    var m = _reader.ReadLine();

                    if (!string.IsNullOrEmpty(m))
                    {
                        if (m.ToLower() == "close")
                        {
                            Dispose();
                            break;
                        }

                        lock (_consoleLock)
                        {
                            Console.WriteLine($"Message from {ServerPipeNameToConnectTo}: {m}");
                        }
                    }

                    Task.Delay(100).Wait();
                }
            });
        }

        private void ConnectToServerPipe(string serverPipeName)
        {
            if (serverPipeName != ServerPipeName)
            {
                Console.WriteLine($"Connecting to server pipe '{serverPipeName}'...");
                Task.Delay(10000).Wait();
                _clientPipe = NamedPipeServerService.GenerateNamedPipeClientStream(serverPipeName);
                _clientPipe.Connect();
                Console.WriteLine("Connected.");
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Shutting down...");
            _disposed = true;
            Task.Delay(200).Wait();
            _reader.Dispose();
            _writer.Dispose();
            _clientPipe.Close();
            _serverPipe.Close();
            _clientPipe.Dispose();
            _serverPipe.Dispose();
            Process.GetCurrentProcess().Close();
            Console.Write("Press ENTER to close...");
        }
    }
}
