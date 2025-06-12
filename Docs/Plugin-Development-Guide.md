# Pulsar Plugin Development Guide

This guide provides comprehensive information on developing plugins for the Pulsar remote access tool.

## Table of Contents

1. [Plugin System Overview](#plugin-system-overview)
2. [Getting Started](#getting-started)
3. [Client Plugin Development](#client-plugin-development)
4. [Server Plugin Development](#server-plugin-development)
5. [Plugin Architecture](#plugin-architecture)
6. [Best Practices](#best-practices)
7. [Error Handling](#error-handling)
8. [Examples](#examples)
9. [Troubleshooting](#troubleshooting)

## Plugin System Overview

The Pulsar plugin system allows developers to extend the functionality of both the client and server components. Plugins are .NET assemblies that implement specific interfaces and can be dynamically loaded at runtime.

### Key Features

- **Dynamic Loading**: Plugins are loaded at runtime without requiring recompilation
- **Safe Execution**: Built-in validation prevents malformed DLLs from crashing the system
- **Error Isolation**: Plugin errors don't affect the core system or other plugins
- **Bi-directional Communication**: Server plugins can trigger client plugin execution
- **Metadata Support**: Rich plugin information using attributes

### Plugin Types

1. **Client Plugins** (`IClientPlugin`): Run on client machines to perform tasks like system information gathering, file operations, etc.
2. **Server Plugins** (`IServerPlugin`): Run on the server to process responses from client plugins and coordinate operations

## Getting Started

### Prerequisites

- .NET Framework 4.8 or later
- Visual Studio 2019 or later (recommended)
- Basic understanding of C# and .NET development

### Setting Up Your Development Environment

1. Clone or download the Pulsar source code
2. Open `Pulsar.sln` in Visual Studio
3. Build the solution to ensure all dependencies are available
4. Reference the `Pulsar.Plugin.Common` project in your plugin projects

### Creating Your First Plugin

#### Step 1: Create a New Class Library Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\\Pulsar.Plugin.Common\\Pulsar.Plugin.Common.csproj" />
    <PackageReference Include="protobuf-net" Version="3.0.101" />
  </ItemGroup>
</Project>
```

#### Step 2: Implement the Plugin Interface

Choose either `IClientPlugin` for client-side functionality or `IServerPlugin` for server-side functionality.

## Client Plugin Development

Client plugins implement the `IClientPlugin` interface and run on client machines.

### Basic Client Plugin Structure

```csharp
using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using Pulsar.Plugin.Common.Exceptions;
using System;

[PluginInfo("MyPlugin", "1.0.0", "Description of my plugin", "Your Name")]
public class MyClientPlugin : IClientPlugin
{
    public string Name => "MyPlugin";
    public string Version => "1.0.0";

    public void Initialize()
    {
        // Plugin initialization code
        // Set up resources, configuration, etc.
    }

    public byte[] Execute(byte[] input)
    {
        try
        {
            // Your plugin logic here
            // Process the input data
            // Return result as byte array
            
            return result;
        }
        catch (Exception ex)
        {
            // Handle errors appropriately
            throw new PluginExecutionException(Name, "workId", 
                $"Plugin execution failed: {ex.Message}", ex);
        }
    }

    public void Cleanup()
    {
        // Clean up resources
        // Dispose of any objects, close connections, etc.
    }
}
```

### Input/Output Handling

Client plugins receive input as byte arrays and must return byte arrays. Use Protocol Buffers (protobuf-net) for serialization:

```csharp
// Deserializing input
MyRequest request;
using (var stream = new MemoryStream(input))
{
    request = Serializer.Deserialize<MyRequest>(stream);
}

// Serializing output
using (var stream = new MemoryStream())
{
    Serializer.Serialize(stream, response);
    return stream.ToArray();
}
```

### Protocol Buffer Message Example

```csharp
[ProtoContract]
public class MyRequest
{
    [ProtoMember(1)]
    public string Parameter1 { get; set; }
    
    [ProtoMember(2)]
    public bool Parameter2 { get; set; }
}

[ProtoContract]
public class MyResponse
{
    [ProtoMember(1)]
    public string Result { get; set; }
    
    [ProtoMember(2)]
    public DateTime Timestamp { get; set; }
}
```

## Server Plugin Development

Server plugins implement the `IServerPlugin` interface and run on the server to process client responses.

### Basic Server Plugin Structure

```csharp
using Pulsar.Plugin.Common;
using Pulsar.Plugin.Common.Attributes;
using System;

[PluginInfo("MyServerPlugin", "1.0.0", "Server-side processing plugin", "Your Name")]
public class MyServerPlugin : IServerPlugin
{
    public string Name => "MyServerPlugin";
    public string Version => "1.0.0";

    public void Initialize()
    {
        // Server plugin initialization
    }

    public void ProcessResponse(string clientId, string workId, byte[] response)
    {
        try
        {
            // Deserialize response from client
            MyResponse clientResponse;
            using (var stream = new MemoryStream(response))
            {
                clientResponse = Serializer.Deserialize<MyResponse>(stream);
            }

            // Process the response
            ProcessClientData(clientId, clientResponse);
            
            // Optionally trigger another client plugin
            TriggerAnotherPlugin(clientId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing response: {ex.Message}");
        }
    }

    public void ProcessError(string clientId, string workId, byte[] error)
    {
        // Handle client plugin errors
        string errorMessage = Encoding.UTF8.GetString(error);
        Console.WriteLine($"Client {clientId} reported error: {errorMessage}");
    }

    private void TriggerAnotherPlugin(string clientId)
    {
        // Example of triggering a client plugin
        var request = new AnotherRequest { /* initialize */ };
        
        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, request);
            PluginContext.ExecuteClientPlugin("AnotherClientPlugin", 
                Guid.NewGuid().ToString(), stream.ToArray());
        }
    }

    public void Cleanup()
    {
        // Server plugin cleanup
    }
}
```

### Client Plugin Communication

Server plugins can trigger client plugin execution using the `PluginContext`:

```csharp
// Execute a client plugin
PluginContext.ExecuteClientPlugin(
    pluginName: "MyClientPlugin",
    workId: Guid.NewGuid().ToString(),
    input: serializedData
);
```

## Plugin Architecture

### Plugin Lifecycle

1. **Discovery**: Plugins are discovered by scanning DLL files in plugin directories
2. **Validation**: Each plugin DLL is validated for safety and compatibility
3. **Loading**: Valid plugins are loaded into memory
4. **Initialization**: The `Initialize()` method is called
5. **Execution**: Plugins are executed as needed
6. **Cleanup**: The `Cleanup()` method is called when unloading

### Directory Structure

```
Pulsar/
├── Plugins/
│   ├── Client/          # Client plugin DLLs
│   │   ├── MyClientPlugin.dll
│   │   └── AnotherClientPlugin.dll
│   └── Server/          # Server plugin DLLs
│       ├── MyServerPlugin.dll
│       └── AnotherServerPlugin.dll
```

### Plugin Naming Conventions

- Client plugins should end with the namespace containing "Client" (e.g., `MyPlugin.Client`)
- Server plugins should end with the namespace containing "Server" (e.g., `MyPlugin.Server`)
- Plugin names should be descriptive and unique

## Best Practices

### Security

- **Validate all inputs**: Never trust data received from plugins
- **Use safe APIs**: Avoid file system operations, network calls, and registry access unless necessary
- **Handle exceptions**: Always wrap plugin operations in try-catch blocks
- **Minimize permissions**: Run with the least privileges necessary

### Performance

- **Avoid blocking operations**: Use async patterns where possible
- **Minimize memory usage**: Dispose of resources properly
- **Cache expensive operations**: Store results of expensive computations
- **Use efficient serialization**: Protocol Buffers are recommended

### Reliability

- **Implement proper cleanup**: Always implement the `Cleanup()` method
- **Handle null inputs**: Check for null or empty input data
- **Log important events**: Use console output for debugging
- **Test thoroughly**: Test with various input scenarios

### Code Quality

- **Follow naming conventions**: Use descriptive names for methods and variables
- **Document your code**: Add XML documentation comments
- **Use the plugin attribute**: Always add `[PluginInfo]` to your plugin classes
- **Keep plugins focused**: Each plugin should have a single, well-defined purpose

## Error Handling

The plugin system provides structured error handling through custom exceptions:

### Plugin-Specific Exceptions

```csharp
// Plugin loading errors
throw new PluginLoadException(pluginName, "Failed to initialize database connection");

// Plugin execution errors
throw new PluginExecutionException(pluginName, workId, "Invalid input format", innerException);

// Plugin validation errors
throw new PluginValidationException(pluginName, "Plugin requires .NET Framework 4.8 or later");
```

### Error Propagation

- Client plugin errors are sent back to the server plugin
- Server plugins receive errors via the `ProcessError()` method
- All plugin operations are wrapped in error handling to prevent system crashes

### Logging

```csharp
// Use Console.WriteLine for debug output
Console.WriteLine($"[{Name}] Processing request for client {clientId}");

// Log errors with context
Console.WriteLine($"[{Name}] Error: {ex.Message}");
Console.WriteLine($"[{Name}] Stack trace: {ex.StackTrace}");
```

## Examples

### Example 1: Simple File Information Plugin

**Client Plugin:**
```csharp
[PluginInfo("FileInfo", "1.0.0", "Gets file information", "Pulsar Team")]
public class FileInfoPlugin : IClientPlugin
{
    public string Name => "FileInfo";
    public string Version => "1.0.0";

    public void Initialize() { }

    public byte[] Execute(byte[] input)
    {
        var request = Serializer.Deserialize<FileInfoRequest>(new MemoryStream(input));
        
        var fileInfo = new FileInfo(request.FilePath);
        var response = new FileInfoResponse
        {
            Exists = fileInfo.Exists,
            Size = fileInfo.Exists ? fileInfo.Length : 0,
            LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue
        };

        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, response);
            return stream.ToArray();
        }
    }

    public void Cleanup() { }
}
```

### Example 2: System Monitoring Server Plugin

**Server Plugin:**
```csharp
[PluginInfo("SystemMonitor", "1.0.0", "Monitors system health", "Pulsar Team")]
public class SystemMonitorPlugin : IServerPlugin
{
    private Timer _monitoringTimer;

    public string Name => "SystemMonitor";
    public string Version => "1.0.0";

    public void Initialize()
    {
        // Start monitoring every 5 minutes
        _monitoringTimer = new Timer(CheckSystemHealth, null, 
            TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    private void CheckSystemHealth(object state)
    {
        // Trigger system info collection on all clients
        var request = new SystemInfoRequest
        {
            IncludeSoftware = false,
            IncludeProcesses = true,
            IncludeUptime = true
        };

        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, request);
            PluginContext.ExecuteClientPlugin("SystemInfo", 
                Guid.NewGuid().ToString(), stream.ToArray());
        }
    }

    public void ProcessResponse(string clientId, string workId, byte[] response)
    {
        var systemInfo = Serializer.Deserialize<SystemInfoResponse>(new MemoryStream(response));
        
        // Analyze system health
        if (systemInfo.ProcessorCount > 8 && systemInfo.WorkingSet > 1_000_000_000)
        {
            Console.WriteLine($"High resource usage detected on {clientId}");
        }
    }

    public void ProcessError(string clientId, string workId, byte[] error)
    {
        Console.WriteLine($"System monitoring failed for {clientId}");
    }

    public void Cleanup()
    {
        _monitoringTimer?.Dispose();
    }
}
```

## Troubleshooting

### Common Issues

**Plugin Not Loading**
- Check that the DLL is in the correct directory (`Plugins/Client` or `Plugins/Server`)
- Verify the plugin implements the correct interface (`IClientPlugin` or `IServerPlugin`)
- Ensure all dependencies are available
- Check the console output for validation errors

**Plugin Execution Errors**
- Verify input data format and serialization
- Check for null reference exceptions
- Ensure proper error handling in plugin code
- Review console logs for detailed error messages

**Communication Issues**
- Verify plugin names match between client and server
- Check that `PluginContext.ExecuteClientPlugin()` is called correctly
- Ensure serialization/deserialization is symmetric

### Debugging Tips

1. **Use Console Output**: Add debug messages throughout your plugin
2. **Test Incrementally**: Start with simple functionality and add complexity
3. **Validate Input**: Always check input data before processing
4. **Check Dependencies**: Ensure all required assemblies are available
5. **Review Logs**: Check both client and server console output

### Validation Errors

The plugin system validates DLLs before loading:

- **Invalid PE File**: The file is not a valid executable
- **Not a .NET Assembly**: The file is not a valid .NET assembly
- **No Plugin Interface**: The assembly doesn't implement required interfaces
- **Security Warning**: The plugin uses potentially dangerous APIs

### Performance Issues

- **Large Input/Output**: Minimize data size in plugin communications
- **Blocking Operations**: Use asynchronous patterns for I/O operations
- **Memory Leaks**: Ensure proper disposal of resources in `Cleanup()`
- **CPU Usage**: Optimize algorithms and avoid tight loops

## Advanced Topics

### Plugin Dependencies

Plugins can reference other assemblies, but be aware of:
- Version conflicts with core Pulsar assemblies
- Missing dependencies on target machines
- Security implications of third-party libraries

### Custom Serialization

While Protocol Buffers are recommended, you can use other serialization methods:
- JSON (System.Text.Json or Newtonsoft.Json)
- XML Serialization
- Binary Serialization (not recommended for security reasons)

### Plugin Versioning

Use semantic versioning (MAJOR.MINOR.PATCH) for plugins:
- MAJOR: Breaking changes
- MINOR: New features, backwards compatible
- PATCH: Bug fixes

### Security Considerations

- **Code Signing**: Consider signing plugin assemblies
- **Sandboxing**: Plugins run in the same security context as the host
- **Input Validation**: Always validate data from external sources
- **API Restrictions**: Some Windows APIs may be restricted in certain environments

---

For additional support or questions, please refer to the Pulsar project documentation or open an issue on the project repository.
