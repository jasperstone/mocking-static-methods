using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests;

public class IISHttpServerTests
{
    private class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Logs { get; } = new();

        public class LogEntry
        {
            public LogLevel Level { get; set; }
            public EventId EventId { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Add(new LogEntry
            {
                Level = logLevel,
                EventId = eventId,
                Message = formatter(state, exception)
            });
        }
    }

    [Fact]
    public void HandleRequest_WhenExceptionThrown_LogsError()
    {
        // Arrange
        var logger = new TestLogger<IISHttpServer>();
        var server = CreateServer(logger);

        // Replace context factory with throwing one using reflection
        var contextFactoryField = typeof(IISHttpServer).GetField("_iisContextFactory", BindingFlags.NonPublic | BindingFlags.Instance)!;
        contextFactoryField.SetValue(server, new ThrowingContextFactory());

        var handleField = typeof(IISHttpServer).GetField("_httpServerHandle", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var gcHandle = (GCHandle)handleField.GetValue(server)!;
        var pvRequestContext = gcHandle.AddrOfPinnedObject();
        var pInProcessHandler = Marshal.AllocHGlobal(1);

        try
        {
            // Act
            _ = IISHttpServer.HandleRequest(pInProcessHandler, pvRequestContext);
        }
        finally
        {
            Marshal.FreeHGlobal(pInProcessHandler);
        }

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Equal(0, log.EventId.Id);
        Assert.Contains("Unexpected exception in static IISHttpServer.HandleRequest.", log.Message);
    }

    [Fact]
    public void HandleShutdown_WhenExceptionThrown_LogsError()
    {
        // Arrange
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        mockLifetime.Setup(l => l.StopApplication()).Throws(new InvalidOperationException("test"));

        var logger = new TestLogger<IISHttpServer>();
        var server = CreateServer(logger, mockLifetime.Object);

        var handleField = typeof(IISHttpServer).GetField("_httpServerHandle", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var gcHandle = (GCHandle)handleField.GetValue(server)!;
        var pvRequestContext = gcHandle.AddrOfPinnedObject();

        // Act
        _ = IISHttpServer.HandleShutdown(pvRequestContext);

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Equal(0, log.EventId.Id);
        Assert.Contains("Unexpected exception in IISHttpServer.HandleShutdown.", log.Message);
    }

    private static IISHttpServer CreateServer(ILogger<IISHttpServer> logger, IHostApplicationLifetime? lifetime = null)
    {
        var appLifetime = lifetime ?? Mock.Of<IHostApplicationLifetime>();
        var nativeApp = Mock.Of<IISNativeApplication>();
        var auth = Mock.Of<IAuthenticationSchemeProvider>();
        var config = Mock.Of<IConfiguration>();
        var memoryPoolFactory = Mock.Of<IMemoryPoolFactory<byte>>();
        var options = new IISServerOptions();

        var server = new IISHttpServer(
            nativeApp,
            appLifetime,
            auth,
            config,
            memoryPoolFactory,
            Options.Create(options),
            logger);

        server.StartAsync(Mock.Of<IHttpApplication<object>>(), default).Wait();
        return server;
    }

    private class ThrowingContextFactory : IISContextFactory<object>
    {
        public override object CreateHttpContext(NativeSafeHandle safeHandle) => throw new InvalidOperationException("test");
    }
}
