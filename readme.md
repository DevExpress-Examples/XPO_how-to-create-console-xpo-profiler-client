<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/257919049/20.1.2%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T882761)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
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
