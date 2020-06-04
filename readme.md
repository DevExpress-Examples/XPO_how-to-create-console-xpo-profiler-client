# How to create a console XPO Profiler to detect performance bottlenecks and code issues

This example demonstrates how to implement a console version of the [XPO Profiler](https://docs.devexpress.com/XpoProfiler/10646/xpo-profiler) tool.

Below are usage examples for various service bindings. For more information, see [Specify Connection Parameters](https://docs.devexpress.com/XpoProfiler/10659/set-up-the-profiler#specify-connection-parameters).

## NetTcpBinding

`XpoProfilerConsole.exe -protocol nettcp -host <hostname> -port <portumber>`

## WsHttpBinding

`XpoProfilerConsole.exe -protocol wshttp -host <hostname> -port <portumber> -servicename <serviceName>`

## BasicHttpBinding

`XpoProfilerConsole.exe -protocol basichttp -host <hostname> -port <portumber> -servicename <serviceName>`

## Web API

`XpoProfilerConsole.exe -protocol webapi -host <hostname> -port <portumber> -path <path-to-api-controller>`

## Named Pipes

`XpoProfilerConsole.exe -protocol namedpipes -host <hostName> -pipename <pipeName>`
