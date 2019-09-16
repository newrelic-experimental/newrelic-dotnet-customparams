# Asp .Net - Adding Custom Transaction Parameters

A custom extension for the New Relic .Net Framework agent to add custom transaction parameters from configured ASP .Net Http Request query string and form parameters, headers and cookies.

## Installation

1. Drop the extension dll in the newrelic agent's "extensions" folder.

```cmd
   copy Custom.Providers.Wrapper.AspNet.dll C:\Program Files\New Relic\.NET Agent\Extensions\
```

2. Drop the extension xml in the newrelic agent's "extensions" folder.

```cmd
   copy Custom.Providers.Wrapper.AspNet.xml C:\ProgramData\New Relic\.NET Agent\Extensions
```

***
**Note: The XML file must be dropped into ProgramData's extension folder whereas DLL file must be dropped into Program Files's extension folder**
***

3. Edit the newrelic agent's configuration file (`newrelic.config`) and add the following properties as applicable to the `appSettings` element:

```xml
  # To collect custom request headers:
  <add key="requestHeaders" value="ContentPref" />
  # To collect custom request parameters:
  <add key="requestParams" value="city, country" />
  #To collect custom cookies
  <add key="requestCookies" value="SSOUSER, SSOSESSIONID" />
  # To set a prefix for the collected attributes
  # Leave blank or set to "blank" to have no prefix.
  # Default: ''
  <add key="prefix" value="" />
```

An Example snippet of newrelic.config with the above configuration looks like this

```xml
<?xml version="1.0"?>
<!-- Copyright (c) 2008-2017 New Relic, Inc.  All rights reserved. -->
<!-- For more information see: https://newrelic.com/docs/dotnet/dotnet-agent-configuration -->
<configuration xmlns="urn:newrelic-config" agentEnabled="true">
  <service licenseKey="???" />
  <application>
    <name>My Application</name>
  </application>
  <appSettings>
    <add key="requestHeaders" value="ContentPref" />
    <add key="requestParams" value="city, country" />
	<add key="requestCookies" value="SSOUSER, SSOSESSIONID" />
  </appSettings>
  <log level="info" />
...
```
4. Restart your application after adding the extension files and configurations.
3. Check your [results](#results)!

## Results

The instrumentation will add the extracted headers and/or parameters as custom transaction parameters, which are found in these places:

- APM Transaction Traces (both distributed traces and classic) in the "Attributes" section
- Transaction events in Insights




