using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using DevExpress.Xpo.Logger;
using DevExpress.Xpo.Logger.Transport;

namespace XpoProfilerConsole {
    class Program {

        static void Main(string[] args) {
            try {
                ILogClient logClient = CreateLogClient(args);
                if(logClient == null) {
                    DisplayUsage();
                    return;
                }
                logClient.Start();
                ReadLoop(logClient);
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        static void DisplayUsage() {
            Console.WriteLine("Usage:");
            Console.WriteLine("-protocol [nettcp -host hostName[-port portNumber]]");
            Console.WriteLine("  | [wshttp | basichttp -host hostName [-port portNumber] -servicename serviceName]");
            Console.WriteLine("  | [webapi -host hostName [-port portNumber] -path pathFromRoot [[-login username] [-password password] | [-token token]]]");
            Console.WriteLine("  | [namedpipes -host hostName -pipename -pipeName]");
        }

        static string GetCmdLineParameter(string[] args, string paramName) {
            for(int i = 0; i < args.Length - 1; i += 2) {
                if(string.Equals(args[i], paramName, StringComparison.CurrentCultureIgnoreCase)) {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }

        static ILogClient CreateLogClient(string[] args) {
            string protocol = GetCmdLineParameter(args, "-protocol");
            string host = GetCmdLineParameter(args, "-host");
            string serviceName = GetCmdLineParameter(args, "-serviceName");
            string portString = GetCmdLineParameter(args, "-port");
            string login = GetCmdLineParameter(args, "-login");
            string password = GetCmdLineParameter(args, "-password");
            string pipeName = GetCmdLineParameter(args, "-pipename");
            string token = GetCmdLineParameter(args, "-token");
            string path = GetCmdLineParameter(args, "-path");
            int port = !string.IsNullOrWhiteSpace(portString) ? int.Parse(portString, CultureInfo.InvariantCulture) : 80;
            switch(protocol.ToLower()) {
                case "nettcp":
                    if(string.IsNullOrWhiteSpace(host)) {
                        return null;
                    }
                    return new LogClient(new Host(host, port, NetBinding.NetTcpBinding, null));
                case "wshttp":
                    if(string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(serviceName)) {
                        return null;
                    }
                    return new LogClient(new Host(host, port, NetBinding.WSHttpBinding, serviceName));
                case "basichttp":
                    if(string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(serviceName)) {
                        return null;
                    }
                    return new LogClient(new Host(host, port, NetBinding.BasicHttpBinding, serviceName));
                case "webapi":
                    if(string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(path)) {
                        return null;
                    }                   
                    return new WebApiLogClient(new Host(host, port, NetBinding.WebApi, path), login, password, token);
                case "namedpipes":
                    if(string.IsNullOrWhiteSpace(pipeName)) {
                        return null;
                    }
                    if(string.IsNullOrWhiteSpace(host)) {
                        host = ".";
                    }
                    return new NamedPipeLogClient(new Host(host, -1, NetBinding.NamedPipes, pipeName));
            }
            return null;
        }

        static void ReadLoop(ILogClient logReader) {
            const int bufferSize = 100;
            const int waitSmallInterval = 200;
            const int waitInterval = 2000;
            LogMessage[] buffer = new LogMessage[bufferSize];
            int lastMessageCounts = 1;
            while(true) {
                LogMessage[] lmArray = logReader.GetCompleteLog();
                if(lmArray != null && lmArray.Length == 1 && lmArray[0].MessageType == LogMessageType.LoggingEvent &&
                    lmArray[0].MessageText == "The maximum string content length quota has been exceeded") {
                    List<LogMessage> lmList = new List<LogMessage>();
                    LogMessage[] lmArrayChunk;
                    int lmCount = lastMessageCounts * 2;
                    do {
                        lmCount /= 2;
                        lmArrayChunk = logReader.GetMessages(lmCount);
                    } while(lmArrayChunk == null && lmCount > 0);
                    if(lmCount > 0) {
                        do {
                            lmList.AddRange(lmArrayChunk);
                            lmArrayChunk = logReader.GetMessages(lmCount);
                        } while(lmArrayChunk.Length > 0);
                        lmArray = lmList.ToArray();
                    }
                }
                if(lmArray != null && lmArray.Length != 0) {
                    lastMessageCounts = lmArray.GetLength(0);
                    if(lmArray.Length <= bufferSize) {
                        WriteMessagesToOutput(lmArray, lmArray.Length);
                    } else {
                        int position = 0;
                        while(position < lmArray.Length) {
                            int size = Math.Min(lmArray.Length - position, bufferSize);
                            Array.Copy(lmArray, position, buffer, 0, size);
                            WriteMessagesToOutput(buffer, size);
                            position += size;
                            Thread.Sleep(waitSmallInterval);
                        }
                    }
                }
                Thread.Sleep(waitInterval);
            }
        }

        static void WriteMessagesToOutput(LogMessage[] buffer, int bufferSize) {
            for(int i = 0; i < bufferSize; i++) {
                string serialized = SerializeLogMessage(buffer[i]);
                Console.WriteLine(serialized);
            }
        }

        static readonly DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(LogMessage));
        static string SerializeLogMessage(LogMessage message) {
            using(var stringWriter = new StringWriter()) {
                using(var xmlTextWriter = new XmlTextWriter(stringWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlSerializer.WriteObject(xmlTextWriter, message);
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }        
    }
}
