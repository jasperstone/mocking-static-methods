using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;

public class ApplicationHostTests
{
    [Fact]
    public void CreateInstanceSafe_DetectsDILoop_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ApplicationHost>>();
        var pluginManagerMock = new Mock<PluginManager>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var applicationHostMock = new ApplicationHostMock(null, null, null, null);
        applicationHostMock.Logger = loggerMock.Object;
        applicationHostMock._pluginManager = pluginManagerMock.Object;
        applicationHostMock.ServiceProvider = serviceProviderMock.Object;

        var type = typeof(ApplicationHostTests);

        // Act
        Assert.Throws<TypeLoadException>(() => applicationHostMock.CreateInstanceSafe(type));

        // Assert
        loggerMock.Verify(
            x => x.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
            Times.Once);

        loggerMock.Verify(
            x => x.LogError("Called from: {TypeName}", type.FullName),
            Times.Once);

        pluginManagerMock.Verify(
            x => x.FailPlugin(type.Assembly),
            Times.Once);
    }
}

public class ApplicationHostMock : ApplicationHost
{
    public ApplicationHostMock(
        IServerApplicationPaths applicationPaths,
        ILoggerFactory loggerFactory,
        IStartupOptions options,
        IConfiguration startupConfig)
        : base(applicationPaths, loggerFactory, options, startupConfig)
    {
    }

    public new object CreateInstanceSafe(Type type)
    {
        return base.CreateInstanceSafe(type);
    }

    protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
    {
        return new List<Assembly>();
    }

    public new ILogger<ApplicationHost> Logger { get; set; }
    public new PluginManager _pluginManager { get; set; }
    public new IServiceProvider ServiceProvider { get; set; }
}
