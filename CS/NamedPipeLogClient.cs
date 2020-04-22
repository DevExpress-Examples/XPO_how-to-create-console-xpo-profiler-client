using DevExpress.Xpo.Logger;
using DevExpress.Xpo.Logger.Transport;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;

namespace XpoProfilerConsole {
    class NamedPipeLogClient : ILogClient, IDisposable {
        const int NamedPipeConnectTimeout = 5000;
        readonly Host host;
        NamedPipeClientStream namedPipeClient;

        public NamedPipeLogClient(Host host) {
            this.host = host;
        }

        public LogMessage[] GetCompleteLog() {
            try {
                EnsureCreateNamedPipe();
                namedPipeClient.WriteByte(1);
                return GetResponseFromNamedPipe();
            } catch {
                DisposeNamedPipe();
                throw;
            }
        }

        public LogMessage GetMessage() {
            try {
                EnsureCreateNamedPipe();
                namedPipeClient.WriteByte(2);
                LogMessage[] messages = GetResponseFromNamedPipe();
                if(messages.Length == 1) {
                    return messages[0];
                } else if(messages.Length == 0) {
                    return null;
                }
                throw new InvalidOperationException("Invalid response from server.");
            } catch {
                DisposeNamedPipe();
                throw;
            }
        }

        public LogMessage[] GetMessages(int messageAmount) {
            try {
                EnsureCreateNamedPipe();
                namedPipeClient.WriteByte(3);
                namedPipeClient.Write(BitConverter.GetBytes(messageAmount), 0, 4);
                return GetResponseFromNamedPipe();
            } catch {
                DisposeNamedPipe();
                throw;
            }
        }

        LogMessage[] GetResponseFromNamedPipe() {
            byte[] buf = new byte[4096];
            int bytesRead = namedPipeClient.Read(buf, 0, 4);
            if(bytesRead != 4) {
                throw new InvalidOperationException("Invalid response from server.");
            }
            int dataLength = BitConverter.ToInt32(buf, 0);
            if(dataLength < 0) {
                throw new InvalidOperationException("Invalid response from server.");
            }
            if(dataLength == 0) {
                return new LogMessage[0];
            }
            var serializer = new DataContractSerializer(typeof(LogMessage[]));
            bytesRead = 0;
            using(var stream = new MemoryStream()) {
                while(bytesRead != dataLength) {
                    int partLength = namedPipeClient.Read(buf, 0, Math.Min(dataLength - bytesRead, buf.Length));
                    if(partLength == 0 || !namedPipeClient.IsConnected) {
                        throw new InvalidOperationException("Invalid response from server.");
                    }
                    stream.Write(buf, 0, partLength);
                    bytesRead += partLength;
                }
                stream.Position = 0;
                return (LogMessage[])serializer.ReadObject(stream);
            }
        }

        void EnsureCreateNamedPipe() {
            if(namedPipeClient == null) {
                namedPipeClient = new NamedPipeClientStream(host.HostAddress, host.ServiceName, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation);
                namedPipeClient.Connect(NamedPipeConnectTimeout);
            }
        }
        void DisposeNamedPipe() {
            if(namedPipeClient != null) {
                namedPipeClient.Dispose();
                namedPipeClient = null;
            }
        }
        public bool Start() {
            try {
                EnsureCreateNamedPipe();
                return true;
            } catch {
                return false;
            }
        }
        public bool Stop() {
            DisposeNamedPipe();
            return true;
        }
        public void Dispose() {
            DisposeNamedPipe();
        }
    }
}
