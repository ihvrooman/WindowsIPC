using IPC;

var helper = new ConsoleHelper("Console 1", "Pipe1", "Pipe2");
helper.Startup();
helper.WaitForConnection();
helper.BeginRead();
helper.BeginWrite();