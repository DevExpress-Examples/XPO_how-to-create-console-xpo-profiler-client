Imports DevExpress.Xpo.Logger
Imports DevExpress.Xpo.Logger.Transport
Imports System
Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.Serialization

Namespace XpoProfilerConsole
	Friend Class NamedPipeLogClient
		Implements ILogClient, IDisposable

		Private Const NamedPipeConnectTimeout As Integer = 5000
		Private ReadOnly host As Host
		Private namedPipeClient As NamedPipeClientStream

		Public Sub New(ByVal host As Host)
			Me.host = host
		End Sub

		Public Function GetCompleteLog() As LogMessage() Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetCompleteLog
			Try
				EnsureCreateNamedPipe()
				namedPipeClient.WriteByte(1)
				Return GetResponseFromNamedPipe()
			Catch
				DisposeNamedPipe()
				Throw
			End Try
		End Function

		Public Function GetMessage() As LogMessage Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetMessage
			Try
				EnsureCreateNamedPipe()
				namedPipeClient.WriteByte(2)
				Dim messages() As LogMessage = GetResponseFromNamedPipe()
				If messages.Length = 1 Then
					Return messages(0)
				ElseIf messages.Length = 0 Then
					Return Nothing
				End If
				Throw New InvalidOperationException("Invalid response from server.")
			Catch
				DisposeNamedPipe()
				Throw
			End Try
		End Function

		Public Function GetMessages(ByVal messageAmount As Integer) As LogMessage() Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetMessages
			Try
				EnsureCreateNamedPipe()
				namedPipeClient.WriteByte(3)
				namedPipeClient.Write(BitConverter.GetBytes(messageAmount), 0, 4)
				Return GetResponseFromNamedPipe()
			Catch
				DisposeNamedPipe()
				Throw
			End Try
		End Function

		Private Function GetResponseFromNamedPipe() As LogMessage()
			Dim buf(4095) As Byte
			Dim bytesRead As Integer = namedPipeClient.Read(buf, 0, 4)
			If bytesRead <> 4 Then
				Throw New InvalidOperationException("Invalid response from server.")
			End If
			Dim dataLength As Integer = BitConverter.ToInt32(buf, 0)
			If dataLength < 0 Then
				Throw New InvalidOperationException("Invalid response from server.")
			End If
			If dataLength = 0 Then
				Return New LogMessage(){}
			End If
			Dim serializer = New DataContractSerializer(GetType(LogMessage()))
			bytesRead = 0
			Using stream = New MemoryStream()
				Do While bytesRead <> dataLength
					Dim partLength As Integer = namedPipeClient.Read(buf, 0, Math.Min(dataLength - bytesRead, buf.Length))
					If partLength = 0 OrElse Not namedPipeClient.IsConnected Then
						Throw New InvalidOperationException("Invalid response from server.")
					End If
					stream.Write(buf, 0, partLength)
					bytesRead += partLength
				Loop
				stream.Position = 0
				Return DirectCast(serializer.ReadObject(stream), LogMessage())
			End Using
		End Function

		Private Sub EnsureCreateNamedPipe()
			If namedPipeClient Is Nothing Then
				namedPipeClient = New NamedPipeClientStream(host.HostAddress, host.ServiceName, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation)
				namedPipeClient.Connect(NamedPipeConnectTimeout)
			End If
		End Sub
		Private Sub DisposeNamedPipe()
			If namedPipeClient IsNot Nothing Then
				namedPipeClient.Dispose()
				namedPipeClient = Nothing
			End If
		End Sub
		Public Function Start() As Boolean Implements ILogClient.Start
			Try
				EnsureCreateNamedPipe()
				Return True
			Catch
				Return False
			End Try
		End Function
		Public Function [Stop]() As Boolean Implements ILogClient.Stop
			DisposeNamedPipe()
			Return True
		End Function
		Public Sub Dispose() Implements System.IDisposable.Dispose
			DisposeNamedPipe()
		End Sub
	End Class
End Namespace
