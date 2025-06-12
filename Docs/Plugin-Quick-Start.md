# Pulsar Plugin Quick Start Guide

This guide will help you create your first Pulsar plugin in just a few minutes.

## Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8 or later
- Pulsar source code

## Step 1: Create Your Plugin Project

1. Open Visual Studio
2. Create a new **Class Library (.NET Framework)** project
3. Target **.NET Framework 4.8**
4. Name your project (e.g., `MyAwesome.Plugin.Client`)

## Step 2: Configure Your Project

Replace your `.csproj` file content with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <OutputPath>..\..\Pulsar\bin\Debug\Plugins\Client\</OutputPath>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\Pulsar\Pulsar.Plugin.Common\Pulsar.Plugin.Common.csproj" />
    <PackageReference Include="protobuf-net" Version="3.0.101" />
  </ItemGroup>
</Project>
```

## Step 3: Create Your Client Plugin

Replace the default `Class1.cs` with:

```csharp
using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using System;

[PluginInfo("MyPlugin", "1.0.0", "My first Pulsar plugin", "Your Name")]
public class MyPlugin : IClientPlugin
{
    public string Name => "MyPlugin";
    public string Version => "1.0.0";

    public void Initialize()
    {
        Console.WriteLine("MyPlugin initialized!");
    }

    public byte[] Execute(byte[] input)
    {
        // Your plugin logic here
        string message = $"Plugin executed at {DateTime.Now}";
        return System.Text.Encoding.UTF8.GetBytes(message);
    }

    public void Cleanup()
    {
        Console.WriteLine("MyPlugin cleaned up!");
    }
}
```

## Step 4: Build and Test

1. **Build your project** (Ctrl+Shift+B)
2. **Run Pulsar Server** and check the console output
3. You should see: `Loaded server plugin: MyPlugin`

## Step 5: Create a Server Plugin (Optional)

If you want a server plugin to handle responses:

1. Create another project named `MyAwesome.Plugin.Server`
2. Change the output path to `Plugins\Server\`
3. Create a server plugin:

```csharp
using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using System;

[PluginInfo("MyServerPlugin", "1.0.0", "Server side of my plugin", "Your Name")]
public class MyServerPlugin : IServerPlugin
{
    public string Name => "MyServerPlugin";
    public string Version => "1.0.0";

    public void Initialize()
    {
        Console.WriteLine("MyServerPlugin initialized!");
    }

    public void ProcessResponse(string clientId, string workId, byte[] response)
    {
        string message = System.Text.Encoding.UTF8.GetString(response);
        Console.WriteLine($"Received from {clientId}: {message}");
    }

    public void ProcessError(string clientId, string workId, byte[] error)
    {
        Console.WriteLine($"Error from {clientId}: {System.Text.Encoding.UTF8.GetString(error)}");
    }

    public void Cleanup()
    {
        Console.WriteLine("MyServerPlugin cleaned up!");
    }
}
```

## Step 6: Test Plugin Execution

1. Start Pulsar Server
2. Connect a client
3. Use the server interface to execute your plugin
4. Check console output for plugin messages

## Next Steps

- Read the [Full Plugin Development Guide](Plugin-Development-Guide.md)
- Check out the example plugins in the source code
- Explore Protocol Buffer serialization for complex data
- Learn about plugin security best practices

## Troubleshooting

**Plugin not loading?**
- Check the console output for error messages
- Verify the plugin is in the correct directory
- Ensure it implements the correct interface

**Build errors?**
- Make sure you're targeting .NET Framework 4.8
- Check that all references are correctly added
- Verify the Pulsar.Plugin.Common reference path

**Plugin not executing?**
- Check plugin names match between client and server
- Verify the plugin loaded successfully
- Look for runtime error messages in console

For more detailed information, see the complete [Plugin Development Guide](Plugin-Development-Guide.md).
