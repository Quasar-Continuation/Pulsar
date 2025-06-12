# Pulsar Plugin API Reference

This document provides a comprehensive reference for the Pulsar Plugin API.

## Core Interfaces

### IClientPlugin

Client plugins must implement this interface to run on client machines.

```csharp
public interface IClientPlugin
{
    string Name { get; }
    string Version { get; }
    byte[] Execute(byte[] input);
    void Initialize();
    void Cleanup();
}
```

#### Properties

**Name** : `string`
- Gets the unique plugin name
- Must be consistent across plugin versions
- Used for plugin identification and routing

**Version** : `string`
- Gets the plugin version
- Should follow semantic versioning (MAJOR.MINOR.PATCH)
- Used for compatibility checking

#### Methods

**Initialize()** : `void`
- Called once when the plugin is loaded
- Use for resource allocation and setup
- Throws `PluginLoadException` on failure

**Execute(byte[] input)** : `byte[]`
- Main plugin execution method
- Receives serialized input data
- Returns serialized response data
- Throws `PluginExecutionException` on failure

**Cleanup()** : `void`
- Called when the plugin is unloaded
- Use for resource cleanup and disposal
- Should not throw exceptions

### IServerPlugin

Server plugins implement this interface to process client responses.

```csharp
public interface IServerPlugin
{
    string Name { get; }
    string Version { get; }
    void ProcessResponse(string clientId, string workId, byte[] response);
    void ProcessError(string clientId, string workId, byte[] error);
    void Initialize();
    void Cleanup();
}
```

#### Properties

**Name** : `string`
- Gets the unique plugin name
- Should match corresponding client plugin for automatic routing
- Used for plugin identification

**Version** : `string`
- Gets the plugin version
- Should follow semantic versioning
- Used for compatibility checking

#### Methods

**Initialize()** : `void`
- Called once when the plugin is loaded
- Use for resource allocation and setup
- Throws `PluginLoadException` on failure

**ProcessResponse(string clientId, string workId, byte[] response)** : `void`
- Processes successful responses from client plugins
- `clientId`: Unique identifier of the responding client
- `workId`: Unique identifier for tracking this operation
- `response`: Serialized response data from client plugin

**ProcessError(string clientId, string workId, byte[] error)** : `void`
- Processes error responses from client plugins
- `clientId`: Unique identifier of the client that errored
- `workId`: Unique identifier for tracking this operation
- `error`: Serialized error data from client plugin

**Cleanup()** : `void`
- Called when the plugin is unloaded
- Use for resource cleanup and disposal
- Should not throw exceptions

## Attributes

### PluginInfoAttribute

Provides metadata about a plugin for discovery and management.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PluginInfoAttribute : Attribute
{
    public PluginInfoAttribute(string name, string version, string description = "", 
                              string author = "", string minimumPulsarVersion = "1.0.0");
    
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Author { get; }
    public string MinimumPulsarVersion { get; }
}
```

#### Usage

```csharp
[PluginInfo("MyPlugin", "1.2.0", "Does something useful", "John Doe", "1.0.0")]
public class MyPlugin : IClientPlugin
{
    // Plugin implementation
}
```

#### Parameters

- **name**: Unique plugin identifier
- **version**: Plugin version (semantic versioning)
- **description**: Brief description of plugin functionality
- **author**: Plugin author/developer name
- **minimumPulsarVersion**: Minimum required Pulsar version

## Plugin Context

### PluginContext

Provides communication capabilities for server plugins.

```csharp
public static class PluginContext
{
    public static void ExecuteClientPlugin(string pluginName, string workId, byte[] input);
    public static bool IsExecutorAvailable { get; }
}
```

#### Methods

**ExecuteClientPlugin(string pluginName, string workId, byte[] input)** : `void`
- Executes a client plugin from a server plugin context
- `pluginName`: Name of the client plugin to execute
- `workId`: Unique identifier for tracking this operation
- `input`: Serialized input data for the client plugin
- Throws `InvalidOperationException` if no executor is available
- Throws `ArgumentNullException` if parameters are null

#### Properties

**IsExecutorAvailable** : `bool`
- Gets whether a plugin executor is available in the current context
- Returns `true` when called from within a server plugin context
- Returns `false` when called outside plugin execution context

#### Usage Example

```csharp
public void ProcessResponse(string clientId, string workId, byte[] response)
{
    // Process the response...
    
    // Trigger another client plugin
    var request = new AnotherRequest { /* data */ };
    using (var stream = new MemoryStream())
    {
        Serializer.Serialize(stream, request);
        PluginContext.ExecuteClientPlugin("AnotherPlugin", 
            Guid.NewGuid().ToString(), stream.ToArray());
    }
}
```

## Exception Types

### PluginException

Base class for all plugin-related exceptions.

```csharp
public class PluginException : Exception
{
    public string PluginName { get; }
    public PluginException(string pluginName, string message);
    public PluginException(string pluginName, string message, Exception innerException);
}
```

### PluginLoadException

Thrown when a plugin fails to load or initialize.

```csharp
public class PluginLoadException : PluginException
{
    public PluginLoadException(string pluginName, string message);
    public PluginLoadException(string pluginName, string message, Exception innerException);
}
```

### PluginExecutionException

Thrown when a plugin fails during execution.

```csharp
public class PluginExecutionException : PluginException
{
    public string WorkId { get; }
    public PluginExecutionException(string pluginName, string workId, string message);
    public PluginExecutionException(string pluginName, string workId, string message, Exception innerException);
}
```

### PluginValidationException

Thrown when plugin validation fails.

```csharp
public class PluginValidationException : PluginException
{
    public PluginValidationException(string pluginName, string message);
    public PluginValidationException(string pluginName, string message, Exception innerException);
}
```

## Validation

### PluginValidator

Provides validation capabilities for plugin assemblies.

```csharp
public static class PluginValidator
{
    public static PluginValidationResult ValidatePluginFile(string pluginPath);
    public static PluginValidationResult ValidatePluginBytes(byte[] pluginBytes, string pluginName);
}
```

#### Methods

**ValidatePluginFile(string pluginPath)** : `PluginValidationResult`
- Validates a plugin assembly from file path
- Checks file existence, PE format, and .NET assembly validity
- Returns validation result with success/failure status

**ValidatePluginBytes(byte[] pluginBytes, string pluginName)** : `PluginValidationResult`
- Validates a plugin assembly from byte array
- Performs comprehensive validation including interface checks
- Returns validation result with detailed information

### PluginValidationResult

Contains the result of plugin validation.

```csharp
public class PluginValidationResult
{
    public bool IsValid { get; }
    public bool HasWarnings { get; }
    public string Message { get; }
    
    public static PluginValidationResult Success();
    public static PluginValidationResult Warning(string message);
    public static PluginValidationResult Failure(string message);
}
```

#### Properties

- **IsValid**: `true` if validation passed, `false` if failed
- **HasWarnings**: `true` if validation passed with warnings
- **Message**: Descriptive message about validation result

## Message Classes

### SystemInfoRequest

Protocol Buffer message for requesting system information.

```csharp
[ProtoContract]
public class SystemInfoRequest
{
    [ProtoMember(1)]
    public bool IncludeSoftware { get; set; } = true;
    
    [ProtoMember(2)]
    public bool IncludeProcesses { get; set; } = true;
    
    [ProtoMember(3)]
    public bool IncludeUptime { get; set; } = true;
}
```

### SystemInfoResponse

Protocol Buffer message for system information responses.

```csharp
[ProtoContract]
public class SystemInfoResponse
{
    [ProtoMember(1)]
    public string ComputerName { get; set; }
    
    [ProtoMember(2)]
    public string UserName { get; set; }
    
    [ProtoMember(3)]
    public string OSVersion { get; set; }
    
    [ProtoMember(4)]
    public int ProcessorCount { get; set; }
    
    [ProtoMember(5)]
    public long WorkingSet { get; set; }
    
    [ProtoMember(6)]
    public string[] InstalledSoftware { get; set; }
    
    [ProtoMember(7)]
    public string[] RunningProcesses { get; set; }
    
    [ProtoMember(8)]
    public long SystemUpticksTicks { get; set; }
    
    [ProtoMember(9)]
    public long TimestampTicks { get; set; }
    
    [ProtoIgnore]
    public TimeSpan SystemUptime { get; set; }
    
    [ProtoIgnore]
    public DateTime Timestamp { get; set; }
}
```

### PluginErrorResponse

Protocol Buffer message for plugin error responses.

```csharp
[ProtoContract]
public class PluginErrorResponse
{
    [ProtoMember(1)]
    public string Error { get; set; }
    
    [ProtoMember(2)]
    public string StackTrace { get; set; }
}
```

## Best Practices

### Serialization

Use Protocol Buffers for all plugin communication:

```csharp
// Serialization
using (var stream = new MemoryStream())
{
    Serializer.Serialize(stream, myObject);
    return stream.ToArray();
}

// Deserialization
using (var stream = new MemoryStream(data))
{
    return Serializer.Deserialize<MyType>(stream);
}
```

### Error Handling

Always use appropriate exception types:

```csharp
try
{
    // Plugin logic
}
catch (Exception ex)
{
    throw new PluginExecutionException(Name, workId, 
        "Operation failed", ex);
}
```

### Logging

Use console output for debugging:

```csharp
Console.WriteLine($"[{Name}] Processing request for {clientId}");
```

### Resource Management

Always implement proper cleanup:

```csharp
public void Cleanup()
{
    try
    {
        _resource?.Dispose();
        _timer?.Stop();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Cleanup error: {ex.Message}");
    }
}
```

## Common Patterns

### Request-Response Pattern

```csharp
// Client Plugin
public byte[] Execute(byte[] input)
{
    var request = Serializer.Deserialize<MyRequest>(new MemoryStream(input));
    var response = ProcessRequest(request);
    
    using (var stream = new MemoryStream())
    {
        Serializer.Serialize(stream, response);
        return stream.ToArray();
    }
}

// Server Plugin
public void ProcessResponse(string clientId, string workId, byte[] response)
{
    var data = Serializer.Deserialize<MyResponse>(new MemoryStream(response));
    ProcessClientData(clientId, data);
}
```

### Chained Plugin Execution

```csharp
// Server plugin triggering client plugin
public void ProcessResponse(string clientId, string workId, byte[] response)
{
    // Process initial response
    var data = ProcessResponse(response);
    
    // Trigger follow-up plugin
    var followUpRequest = new FollowUpRequest { Data = data };
    using (var stream = new MemoryStream())
    {
        Serializer.Serialize(stream, followUpRequest);
        PluginContext.ExecuteClientPlugin("FollowUpPlugin", 
            Guid.NewGuid().ToString(), stream.ToArray());
    }
}
```

### Error Propagation

```csharp
// Client plugin error handling
public byte[] Execute(byte[] input)
{
    try
    {
        return ProcessRequest(input);
    }
    catch (Exception ex)
    {
        var error = new PluginErrorResponse
        {
            Error = ex.Message,
            StackTrace = ex.StackTrace
        };
        
        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, error);
            return stream.ToArray();
        }
    }
}

// Server plugin error handling
public void ProcessError(string clientId, string workId, byte[] error)
{
    var errorData = Serializer.Deserialize<PluginErrorResponse>(
        new MemoryStream(error));
    
    LogError(clientId, errorData.Error, errorData.StackTrace);
}
```
