<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/257919049/24.2.1%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T882761)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
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
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XPO_how-to-create-console-xpo-profiler-client&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XPO_how-to-create-console-xpo-profiler-client&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
