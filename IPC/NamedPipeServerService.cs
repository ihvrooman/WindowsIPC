using System.IO.Pipes;
using System.Text;

namespace IPC
{
    public static class NamedPipeServerService
    {
        public static NamedPipeServerStream GenerateNamedPipeServerStream(string name)
        {
            return new NamedPipeServerStream(name, PipeDirection.Out, 1);
        }

        public static NamedPipeClientStream GenerateNamedPipeClientStream(string serverName)
        {
            return new NamedPipeClientStream(".", serverName, PipeDirection.In);
        }
    }
}