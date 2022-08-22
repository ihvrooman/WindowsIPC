using System.IO.Pipes;
using System.Text;

namespace IPC
{
    public static class NamedPipeServerService
    {
        public static Encoding Encoding = Encoding.Unicode;
        public static NamedPipeServerStream GenerateNamedPipeServerStream(string name)
        {
            return new NamedPipeServerStream(name, PipeDirection.InOut, 10);
        }

        public static NamedPipeClientStream GenerateNamedPipeClientStream(string serverName, string name)
        {
            return new NamedPipeClientStream(serverName, name);
        }

        public static void Write(this NamedPipeServerStream namedPipeServerStream, string message)
        {
            using (StreamWriter sw = new (namedPipeServerStream))
            {
                sw.AutoFlush = true;
                sw.Write(message);
            }
        }

        public static string Read(this NamedPipeClientStream namedPipeClientStream)
        {
            using (StreamReader sr = new (namedPipeClientStream))
            {
                return sr.ReadToEnd();
            }
            //    var s = string.Empty;
            //var bytes = new byte[500];
            //while (namedPipeServerStream.Read(bytes, 0, int.MaxValue) > 0)
            //{
            //    s += Encoding.GetString(bytes);
            //}

            //return s;
        }
    }
}