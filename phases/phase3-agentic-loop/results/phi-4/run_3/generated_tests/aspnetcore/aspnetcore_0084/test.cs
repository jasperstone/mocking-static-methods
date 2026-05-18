- **NuGet Package**: If `DebugProxyLauncher` is part of a NuGet package, ensure the package is installed in the test project.

- **InternalsVisibleTo**: If `DebugProxyLauncher` is internal, add the following to the `.csproj` file of the project containing `DebugProxyLauncher`:

  