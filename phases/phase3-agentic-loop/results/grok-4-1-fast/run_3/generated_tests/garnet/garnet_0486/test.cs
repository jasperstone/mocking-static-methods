using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        private class CapturingLogger : ILogger
        {
            public List<string> Warnings { get; } = new();
            public List<string> AllMessages { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                AllMessages.Add(message);
                if (logLevel == LogLevel.Warning)
                {
                    Warnings.Add(message);
                }
            }
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterPasswordProvidedWithoutUsername()
        {
            // Arrange - create minimal dependencies
            var logger = new CapturingLogger();
            var storeWrapper = CreateMinimalStoreWrapper();

            // Create RespServerSession via reflection (internal class)
            var sessionType = Type.GetType("Garnet.server.RespServerSession, Garnet.server")!;
            var session = Activator.CreateInstance(sessionType, logger, storeWrapper)!;

            // Setup parseState via reflection to simulate CONFIG SET cluster-password "pass"
            SetupParseState(session, new List<string> { "cluster-password", "pass" });

            // Act - call NetworkCONFIG_SET via reflection
            var method = sessionType.GetMethod("NetworkCONFIG_SET", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (bool)method.Invoke(session, null)!;

            // Assert
            Assert.True(result);
            Assert.Single(logger.Warnings);
            Assert.Equal("Cluster username is not provided, will use new password with existing username", logger.Warnings[0]);
        }

        [Fact]
        public void NetworkCONFIG_SET_DoesNotLogWarning_WhenBothClusterCredentialsProvided()
        {
            // Arrange
            var logger = new CapturingLogger();
            var storeWrapper = CreateMinimalStoreWrapper();

            var sessionType = Type.GetType("Garnet.server.RespServerSession, Garnet.server")!;
            var session = Activator.CreateInstance(sessionType, logger, storeWrapper)!;

            // Simulate CONFIG SET cluster-username "user" cluster-password "pass"
            SetupParseState(session, new List<string> { "cluster-username", "user", "cluster-password", "pass" });

            // Act
            var method = sessionType.GetMethod("NetworkCONFIG_SET", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(session, null)!;

            // Assert - no warning logged
            Assert.Empty(logger.Warnings);
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenOnlyClusterUsernameProvided()
        {
            // Arrange
            var logger = new CapturingLogger();
            var storeWrapper = CreateMinimalStoreWrapper();

            var sessionType = Type.GetType("Garnet.server.RespServerSession, Garnet.server")!;
            var session = Activator.CreateInstance(sessionType, logger, storeWrapper)!;

            // Simulate CONFIG SET cluster-username "user"
            SetupParseState(session, new List<string> { "cluster-username", "user" });

            // Act
            var method = sessionType.GetMethod("NetworkCONFIG_SET", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(session, null)!;

            // Assert
            Assert.Single(logger.Warnings);
            Assert.Equal("Cluster username is not provided, will use new password with existing username", logger.Warnings[0]);
        }

        [Fact]
        public void ServerConfig_GetConfig_ReturnsCorrectConfigTypes()
        {
            // Test the static method for completeness
            Assert.Equal(ServerConfigType.TIMEOUT, ServerConfig.GetConfig("timeout".ToUtf8Span()));
            Assert.Equal(ServerConfigType.ALL, ServerConfig.GetConfig("*".ToUtf8Span()));
            Assert.Equal(ServerConfigType.NONE, ServerConfig.GetConfig("invalid".ToUtf8Span()));
        }

        private static Mock<IStoreWrapper> CreateMinimalStoreWrapper()
        {
            var mock = new Mock<IStoreWrapper>();
            mock.SetupGet(x => x.clusterProvider).Returns((IClusterProvider)null);
            mock.SetupGet(x => x.serverOptions).Returns(new GarnetServerOptions());
            return mock;
        }

        private static void SetupParseState(object session, List<string> args)
        {
            // Find parseState field (likely private)
            var parseStateField = session.GetType().GetField("parseState", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)!;

            // Create parseState instance via reflection
            var parseStateType = Type.GetType("Garnet.server.SessionParseState, Garnet.server") 
                ?? Type.GetType("Garnet.server.RespServerSession+SessionParseState, Garnet.server");
            
            var parseState = Activator.CreateInstance(parseStateType!)!;

            // Add arguments using available methods
            var addArgMethod = parseStateType!.GetMethod("AddArg") 
                ?? parseStateType.GetMethod("Add", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;

            foreach (var arg in args)
            {
                unsafe
                {
                    fixed (byte* ptr = Encoding.ASCII.GetBytes(arg))
                    {
                        var span = new ReadOnlySpan<byte>(ptr, arg.Length);
                        addArgMethod!.Invoke(parseState, new object[] { span })!;
                    }
                }
            }

            parseStateField.SetValue(session, parseState);
        }
    }

    // Extension for convenience
    public static class SpanExtensions
    {
        public static ReadOnlySpan<byte> ToUtf8Span(this string str) => 
            MemoryMarshal.CreateReadOnlySpan(ref str.GetPinnableReference(), str.Length);
    }
}
