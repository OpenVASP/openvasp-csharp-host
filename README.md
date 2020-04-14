
# OpenVASP C# Host

This is a reference implementation for a C# host for the OpenVASP standard.

### How to build

```
dotnet build --configuration Release "OpenVASP.Host.sln"
```

### How to set up

Provide <b>appsettings.json/appsettings.{env}.json</b> (for instance <b>appsettings.Development.json</b>) according to the <b>appsettings.json</b> from the solution root directory (field names are self-descriptive).

### How to use

See <b>http://{host_url}/swagger/index.html</b> for the controllers and models description.