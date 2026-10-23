<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.4.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.3" />
    <PackageReference Include="Moq" Version="4.16.1" />
    <PackageReference Include="FluentValidation" Version="10.3.6" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="5.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ordering.API\Ordering.API.csproj" />
  </ItemGroup>

</Project>
