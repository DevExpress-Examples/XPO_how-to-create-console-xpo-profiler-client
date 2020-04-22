Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Xml
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Threading
Imports DevExpress.Xpo.Logger
Imports DevExpress.Xpo.Logger.Transport

Namespace XpoProfilerConsole
	Friend Class Program

		Shared Sub Main(ByVal args() As String)
			Try
				Dim logClient As ILogClient = CreateLogClient(args)
				If logClient Is Nothing Then
					DisplayUsage()
					Return
				End If
				logClient.Start()
				ReadLoop(logClient)
			Catch ex As Exception
				Console.WriteLine(ex.Message)
			End Try
		End Sub

		Private Shared Sub DisplayUsage()
			Console.WriteLine("Usage:")
			Console.WriteLine("-protocol [nettcp -host hostName[-port portNumber]]")
			Console.WriteLine("  | [wshttp | basichttp -host hostName [-port portNumber] -servicename serviceName]")
			Console.WriteLine("  | [webapi -host hostName [-port portNumber] -path pathFromRoot [[-login username] [-password password] | [-token token]]]")
			Console.WriteLine("  | [namedpipes -host hostName -pipename -pipeName]")
		End Sub

		Private Shared Function GetCmdLineParameter(ByVal args() As String, ByVal paramName As String) As String
			For i As Integer = 0 To args.Length - 2 Step 2
				If String.Equals(args(i), paramName, StringComparison.CurrentCultureIgnoreCase) Then
					Return args(i + 1)
				End If
			Next i
			Return String.Empty
		End Function

		Private Shared Function CreateLogClient(ByVal args() As String) As ILogClient
			Dim protocol As String = GetCmdLineParameter(args, "-protocol")
			Dim host As String = GetCmdLineParameter(args, "-host")
			Dim serviceName As String = GetCmdLineParameter(args, "-serviceName")
			Dim portString As String = GetCmdLineParameter(args, "-port")
			Dim login As String = GetCmdLineParameter(args, "-login")
			Dim password As String = GetCmdLineParameter(args, "-password")
			Dim pipeName As String = GetCmdLineParameter(args, "-pipename")
			Dim token As String = GetCmdLineParameter(args, "-token")
			Dim path As String = GetCmdLineParameter(args, "-path")
			Dim port As Integer = If(Not String.IsNullOrWhiteSpace(portString), Integer.Parse(portString, CultureInfo.InvariantCulture), 80)
			Select Case protocol.ToLower()
				Case "nettcp"
					If String.IsNullOrWhiteSpace(host) Then
						Return Nothing
					End If
					Return New LogClient(New Host(host, port, NetBinding.NetTcpBinding, Nothing))
				Case "wshttp"
					If String.IsNullOrWhiteSpace(host) OrElse String.IsNullOrWhiteSpace(serviceName) Then
						Return Nothing
					End If
					Return New LogClient(New Host(host, port, NetBinding.WSHttpBinding, serviceName))
				Case "basichttp"
					If String.IsNullOrWhiteSpace(host) OrElse String.IsNullOrWhiteSpace(serviceName) Then
						Return Nothing
					End If
					Return New LogClient(New Host(host, port, NetBinding.BasicHttpBinding, serviceName))
				Case "webapi"
					If String.IsNullOrWhiteSpace(host) OrElse String.IsNullOrWhiteSpace(path) Then
						Return Nothing
					End If
					Return New WebApiLogClient(New Host(host, port, NetBinding.WebApi, path), login, password, token)
				Case "namedpipes"
					If String.IsNullOrWhiteSpace(pipeName) Then
						Return Nothing
					End If
					If String.IsNullOrWhiteSpace(host) Then
						host = "."
					End If
					Return New NamedPipeLogClient(New Host(host, -1, NetBinding.NamedPipes, pipeName))
			End Select
			Return Nothing
		End Function

		Private Shared Sub ReadLoop(ByVal logReader As ILogClient)
			Const bufferSize As Integer = 100
			Const waitSmallInterval As Integer = 200
			Const waitInterval As Integer = 2000
			Dim buffer(bufferSize - 1) As LogMessage
			Dim lastMessageCounts As Integer = 1
			Do
				Dim lmArray() As LogMessage = logReader.GetCompleteLog()
				If lmArray IsNot Nothing AndAlso lmArray.Length = 1 AndAlso lmArray(0).MessageType = LogMessageType.LoggingEvent AndAlso lmArray(0).MessageText = "The maximum string content length quota has been exceeded" Then
					Dim lmList As New List(Of LogMessage)()
					Dim lmArrayChunk() As LogMessage
					Dim lmCount As Integer = lastMessageCounts * 2
					Do
						lmCount \= 2
						lmArrayChunk = logReader.GetMessages(lmCount)
					Loop While lmArrayChunk Is Nothing AndAlso lmCount > 0
					If lmCount > 0 Then
						Do
							lmList.AddRange(lmArrayChunk)
							lmArrayChunk = logReader.GetMessages(lmCount)
						Loop While lmArrayChunk.Length > 0
						lmArray = lmList.ToArray()
					End If
				End If
				If lmArray IsNot Nothing AndAlso lmArray.Length <> 0 Then
					lastMessageCounts = lmArray.GetLength(0)
					If lmArray.Length <= bufferSize Then
						WriteMessagesToOutput(lmArray, lmArray.Length)
					Else
						Dim position As Integer = 0
						Do While position < lmArray.Length
							Dim size As Integer = Math.Min(lmArray.Length - position, bufferSize)
							Array.Copy(lmArray, position, buffer, 0, size)
							WriteMessagesToOutput(buffer, size)
							position += size
							Thread.Sleep(waitSmallInterval)
						Loop
					End If
				End If
				Thread.Sleep(waitInterval)
			Loop
		End Sub

		Private Shared Sub WriteMessagesToOutput(ByVal buffer() As LogMessage, ByVal bufferSize As Integer)
			For i As Integer = 0 To bufferSize - 1
				Dim serialized As String = SerializeLogMessage(buffer(i))
				Console.WriteLine(serialized)
			Next i
		End Sub

		Private Shared ReadOnly xmlSerializer As New DataContractSerializer(GetType(LogMessage))
		Private Shared Function SerializeLogMessage(ByVal message As LogMessage) As String
			Using stringWriter = New StringWriter()
				Using xmlTextWriter = New XmlTextWriter(stringWriter)
					xmlTextWriter.Formatting = Formatting.Indented
					xmlSerializer.WriteObject(xmlTextWriter, message)
					Return stringWriter.GetStringBuilder().ToString()
				End Using
			End Using
		End Function
	End Class
End Namespace
