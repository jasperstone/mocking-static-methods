### Key Adjustments:

- **Access Level**: Ensure `ConfigureBuilder` is accessible. If you have control over the production code, change its access level to `public` temporarily for testing.

- **InternalsVisibleTo**: If you can modify the production code, add the following line to the `AssemblyInfo.cs` or the top of the relevant file:

  