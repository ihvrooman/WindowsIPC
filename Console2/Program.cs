using IPC;

var helper = new ConsoleHelper("Console 2", "Pipe2", "Pipe1");
helper.Startup();
helper.WaitForConnection();
helper.BeginRead();
helper.BeginWrite();
