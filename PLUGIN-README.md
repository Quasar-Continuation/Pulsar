# Pulsar Plugin System

The Pulsar Plugin System provides a standardized, secure, and extensible framework for developing plugins that enhance the functionality of the Pulsar remote access tool.

## 🚀 Quick Start

1. **Read the [Quick Start Guide](Docs/Plugin-Quick-Start.md)** - Get your first plugin running in minutes
2. **Use the templates** in the `Templates/` folder
3. **Study the examples** in `Pulsar.Plugin.Client` and `Pulsar.Plugin.Server`
4. **Refer to the [API Reference](Docs/Plugin-API-Reference.md)** for detailed documentation

## 📋 Features

### ✅ Enhanced Security
- **Assembly Validation**: All DLLs are validated before loading to prevent crashes
- **Error Isolation**: Plugin errors don't affect the core system or other plugins
- **Safe Loading**: Built-in checks for malformed assemblies and dangerous APIs
- **Exception Handling**: Comprehensive error handling and reporting

### ✅ Developer-Friendly
- **Rich Documentation**: Complete guides, API reference, and examples
- **Plugin Templates**: Ready-to-use templates for quick development
- **Metadata Support**: Plugin information attributes for better management
- **IntelliSense Support**: Full IDE support with comprehensive XML documentation

### ✅ Robust Architecture
- **Interface-Based**: Clean separation between client and server plugins
- **Protocol Buffers**: Efficient serialization for plugin communication
- **Thread-Safe**: Plugins can be executed concurrently safely
- **Lifecycle Management**: Proper initialization and cleanup hooks

## 📁 Project Structure

```
Pulsar.Plugin.Common/          # Core plugin interfaces and utilities
├── IClientPlugin.cs           # Client plugin interface
├── IServerPlugin.cs           # Server plugin interface
├── PluginContext.cs           # Communication context
├── PluginMessages.cs          # Standard message types
├── Attributes/                # Plugin metadata attributes
├── Exceptions/                # Plugin-specific exceptions
└── Validation/                # Plugin validation utilities

Templates/                     # Plugin development templates
├── ClientPluginTemplate.cs   # Template for client plugins
├── ServerPluginTemplate.cs   # Template for server plugins
└── PluginTemplate.csproj     # Project template

Docs/                         # Documentation
├── Plugin-Development-Guide.md   # Comprehensive development guide
├── Plugin-Quick-Start.md         # Quick start tutorial
└── Plugin-API-Reference.md       # Complete API reference

Plugins/                      # Plugin deployment directories
├── Client/                   # Client plugin DLLs (.dll files)
└── Server/                   # Server plugin DLLs (.dll files)
```

## 🔧 Key Components

### Core Interfaces

- **`IClientPlugin`** - Interface for plugins that run on client machines
- **`IServerPlugin`** - Interface for plugins that run on the server
- **`PluginContext`** - Provides communication between server and client plugins

### Validation System

- **`PluginValidator`** - Validates plugin assemblies before loading
- **`PluginValidationResult`** - Contains validation results and messages
- Prevents crashes from malformed or incompatible DLLs

### Exception Handling

- **`PluginException`** - Base exception for all plugin errors
- **`PluginLoadException`** - Thrown when plugins fail to load
- **`PluginExecutionException`** - Thrown when plugins fail during execution
- **`PluginValidationException`** - Thrown when plugin validation fails

### Metadata System

- **`PluginInfoAttribute`** - Provides rich metadata about plugins
- Includes name, version, description, author, and compatibility information

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Plugin Development Guide](Docs/Plugin-Development-Guide.md) | Comprehensive guide covering all aspects of plugin development |
| [Quick Start Guide](Docs/Plugin-Quick-Start.md) | Get started with your first plugin in minutes |
| [API Reference](Docs/Plugin-API-Reference.md) | Complete API documentation with examples |

## 🛡️ Security Features

### Assembly Validation
- Validates PE file format before loading
- Checks for valid .NET assemblies
- Verifies required interfaces are implemented
- Scans for potentially dangerous API usage

### Error Isolation
- Plugin errors don't crash the main application
- Failed plugins are safely unloaded
- Comprehensive logging for troubleshooting
- Graceful degradation when plugins fail

### Safe Execution
- All plugin operations are wrapped in error handling
- Thread-safe execution model
- Resource cleanup is enforced
- Memory leak prevention

## 🔨 Development Workflow

1. **Create Project**: Use the provided templates or create from scratch
2. **Implement Interface**: Choose `IClientPlugin` or `IServerPlugin`
3. **Add Metadata**: Use `[PluginInfo]` attribute for rich information
4. **Build & Deploy**: DLLs automatically go to the correct directory
5. **Test & Debug**: Use console output and built-in error handling

## 📝 Example Usage

### Simple Client Plugin
```csharp
[PluginInfo("FileReader", "1.0.0", "Reads file contents", "Pulsar Team")]
public class FileReaderPlugin : IClientPlugin
{
    public string Name => "FileReader";
    public string Version => "1.0.0";

    public void Initialize() { }

    public byte[] Execute(byte[] input)
    {
        var request = Deserialize<FileRequest>(input);
        var content = File.ReadAllText(request.FilePath);
        var response = new FileResponse { Content = content };
        return Serialize(response);
    }

    public void Cleanup() { }
}
```

### Corresponding Server Plugin
```csharp
[PluginInfo("FileManager", "1.0.0", "Manages file operations", "Pulsar Team")]
public class FileManagerPlugin : IServerPlugin
{
    public string Name => "FileManager";
    public string Version => "1.0.0";

    public void Initialize() { }

    public void ProcessResponse(string clientId, string workId, byte[] response)
    {
        var fileResponse = Deserialize<FileResponse>(response);
        Console.WriteLine($"File content from {clientId}: {fileResponse.Content}");
        
        // Optionally trigger another plugin
        var followUp = new ProcessRequest { Data = fileResponse.Content };
        PluginContext.ExecuteClientPlugin("DataProcessor", 
            Guid.NewGuid().ToString(), Serialize(followUp));
    }

    public void ProcessError(string clientId, string workId, byte[] error) { }
    public void Cleanup() { }
}
```

## 🚨 Breaking Changes from Previous Version

### ⚠️ Important Updates

1. **Enhanced Error Handling**: All plugins must handle `PluginException` types
2. **Plugin Validation**: All DLLs are now validated before loading
3. **Metadata Attributes**: `[PluginInfo]` attribute is strongly recommended
4. **Improved Context**: `PluginContext.ExecuteClientPlugin()` replaces old methods

### 🔄 Migration Guide

**Old Code:**
```csharp
public class MyPlugin : IClientPlugin
{
    public byte[] Execute(byte[] input)
    {
        // Direct execution without error handling
        return ProcessData(input);
    }
}
```

**New Code:**
```csharp
[PluginInfo("MyPlugin", "1.0.0", "Plugin description", "Author")]
public class MyPlugin : IClientPlugin
{
    public byte[] Execute(byte[] input)
    {
        try
        {
            return ProcessData(input);
        }
        catch (Exception ex)
        {
            throw new PluginExecutionException(Name, "workId", 
                $"Processing failed: {ex.Message}", ex);
        }
    }
}
```

## 🤝 Contributing

1. Follow the established patterns in existing plugins
2. Add comprehensive error handling
3. Include XML documentation comments
4. Test your plugins thoroughly
5. Use the `[PluginInfo]` attribute with meaningful information

## 📞 Support

- **Documentation**: Check the comprehensive guides in the `Docs/` folder
- **Examples**: Study the example plugins in the source code
- **Templates**: Use the templates in `Templates/` folder for quick start
- **Issues**: Report bugs and request features through the project repository

---

**⚡ Get started with the [Quick Start Guide](Docs/Plugin-Quick-Start.md) and have your first plugin running in minutes!**
