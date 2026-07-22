using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterPasswordProvidedWithoutUsername()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RespServerSession>>();
            
            // Find RespServerSession type via reflection (it's internal)
            var sessionType = Assembly.LoadFrom("Garnet.server.dll").GetType("Garnet.server.RespServerSession");
            if (sessionType == null) 
            {
                Assert.True(false, "Could not find RespServerSession type");
                return;
            }

            // Create instance via reflection (assuming constructor takes ILogger and other params)
            var session = (dynamic)Activator.CreateInstance(sessionType, mockLogger.Object);

            // Set up parseState to simulate CONFIG SET cluster-password "pass"
            var parseStateField = sessionType.GetField("parseState", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            
            // Create ArgSlice instances matching the expected constructor
            var keyBytes = Encoding.ASCII.GetBytes("cluster-password");
            var valueBytes = Encoding.ASCII.GetBytes("pass");
            var parseState = new List<object>
            {
                Activator.CreateInstance(typeof(ArgSlice), keyBytes, 0, keyBytes.Length),
                Activator.CreateInstance(typeof(ArgSlice), valueBytes, 0, valueBytes.Length)
            };
            
            parseStateField?.SetValue(session, parseState);

            // Set storeWrapper.clusterProvider to null via reflection to hit warning path
            var storeWrapperField = sessionType.GetField("storeWrapper", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var mockStoreWrapper = new Mock<object>();
            storeWrapperField?.SetValue(session, mockStoreWrapper.Object);

            // Act - invoke private NetworkCONFIG_SET method
            var method = sessionType.GetMethod("NetworkCONFIG_SET", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            method?.Invoke(session, null);

            // Assert - verify LogWarning was called with expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
