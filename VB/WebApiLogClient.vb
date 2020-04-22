Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports DevExpress.Utils
Imports DevExpress.Xpo.Logger
Imports DevExpress.Xpo.Logger.Transport
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace XpoProfilerConsole
	Friend Class WebApiLogClient
		Implements ILogClient

		Private host As Host
		Private Shared ReadOnly timeSpanConverter As New TimeSpanJsonConverter()
		Public Property Login() As String
		Public Property Password() As String
		Public Property AuthToken() As String
		Public Sub New(ByVal host As Host)
			Me.host = host
		End Sub
		Public Sub New(ByVal host As Host, ByVal login As String, ByVal password As String, ByVal token As String)
			Me.host = host
			Me.Login = login
			Me.Password = password
			Me.AuthToken = token
		End Sub
		Private Function GetServiceUrl() As String
			Dim protocol As String = ""
			If Not host.HostAddress.StartsWithInvariantCultureIgnoreCase("http://") AndAlso Not host.HostAddress.StartsWithInvariantCultureIgnoreCase("https://") Then
				protocol = "http://"
			End If
			Return String.Format(CultureInfo.InvariantCulture, "{0}{1}:{2}/{3}/", protocol, host.HostAddress, host.Port, host.ServiceName)
		End Function
		Private Function GetHttpResponse(ByVal url As String) As String
			Dim request As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)
			If Not String.IsNullOrEmpty(Login) Then
				request.Credentials = New NetworkCredential(Login, Password)
			End If
			If Not String.IsNullOrEmpty(AuthToken) Then
				request.Headers.Add("Authorization", String.Concat("XpoProfilerToken ", AuthToken))
			End If
			Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
			Using str As New StreamReader(response.GetResponseStream())
				Return str.ReadToEnd()
			End Using
		End Function
		Public Function GetCompleteLog() As LogMessage() Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetCompleteLog
			Dim url As String = String.Concat(GetServiceUrl(), "GetCompleteLog")
			Dim json As String = GetHttpResponse(url)
			Return JsonConvert.DeserializeObject(Of LogMessage())(json, timeSpanConverter)
		End Function
		Public Function GetMessage() As LogMessage Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetMessage
			Dim url As String = String.Concat(GetServiceUrl(), "GetMessage")
			Dim json As String = GetHttpResponse(url)
			Return JsonConvert.DeserializeObject(Of LogMessage)(json, timeSpanConverter)
		End Function
		Public Function GetMessages(ByVal messagesAmount As Integer) As LogMessage() Implements DevExpress.Xpo.Logger.Transport.ILogSource.GetMessages
			Dim url As String = String.Concat(GetServiceUrl(), "GetCompleteLog?messagesAmount=", messagesAmount.ToString())
			Dim json As String = GetHttpResponse(url)
			Return JsonConvert.DeserializeObject(Of LogMessage())(json, timeSpanConverter)
		End Function
		Public Function Start() As Boolean Implements ILogClient.Start
			Return True
		End Function
		Public Function [Stop]() As Boolean Implements ILogClient.Stop
			Return True
		End Function
		Public Sub Dispose() Implements System.IDisposable.Dispose
		End Sub
	End Class

	Friend Class TimeSpanJsonConverter
		Inherits JsonConverter

		Public Overrides Function CanConvert(ByVal objectType As Type) As Boolean
			Return objectType Is GetType(TimeSpan)
		End Function
		Public Overrides Function ReadJson(ByVal reader As JsonReader, ByVal objectType As Type, ByVal existingValue As Object, ByVal serializer As JsonSerializer) As Object
			Select Case reader.TokenType
				Case JsonToken.String
					Dim val As String = reader.Value.ToString()
					Dim ts As TimeSpan = Nothing
					If TimeSpan.TryParse(val, CultureInfo.InvariantCulture, ts) Then
						Return ts
					End If
					Throw New InvalidOperationException(String.Format("Cannot deserialize TimeSpan from string '{0}'.", val))
				Case JsonToken.StartObject
					Dim jObject As JObject = JObject.Load(reader)
					Dim ticks As JToken = jObject("ticks")
					If ticks IsNot Nothing Then
						Return New TimeSpan(ticks.ToObject(Of Integer)())
					End If
					Throw New InvalidOperationException(String.Format("Cannot deserialize TimeSpan from JSON '{0}'.", jObject))
			End Select
			Throw New InvalidOperationException(String.Format("Cannot deserialize TimeSpan from token '{0}'", reader.TokenType))
		End Function
		Public Overrides Sub WriteJson(ByVal writer As JsonWriter, ByVal value As Object, ByVal serializer As JsonSerializer)
			Throw New NotImplementedException()
		End Sub
	End Class
End Namespace
