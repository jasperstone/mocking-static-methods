using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using System.Threading;
using System.Web;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        private class TestRemoteAccessPolicyResult
        {
            public static readonly TestRemoteAccessPolicyResult Allow = new TestRemoteAccessPolicyResult("Allow");
            public static readonly TestRemoteAccessPolicyResult Deny = new TestRemoteAccessPolicyResult("Deny");

            private readonly string _name;

            private TestRemoteAccessPolicyResult(string name)
            {
                _name = name;
            }

            public override string ToString() => _name;

            public static bool operator !=(TestRemoteAccessPolicyResult left, TestRemoteAccessPolicyResult right) => !(left == right);
            public static bool operator ==(TestRemoteAccessPolicyResult left, TestRemoteAccessPolicyResult right) => ReferenceEquals(left, right);
            public override bool Equals(object obj) => ReferenceEquals(this, obj);
            public override int GetHashCode() => _name.GetHashCode();
        }

        private interface ITestNetworkManager
        {
            TestRemoteAccessPolicyResult ShouldAllowServerAccess(string remoteIP);
        }

        [Fact]
        public async Task Invoke_BlocksAccess_LogsWarningAndSets503()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/testpath";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

            // Setup extension methods IsLocal and GetNormalizedRemoteIP via Moq or manual override
            // Since these are extension methods, we simulate their behavior by setting up the context accordingly
            // We simulate IsLocal() returning false to trigger the blocking logic
            // We simulate GetNormalizedRemoteIP() returning the remote IP string

            // We will create a wrapper for INetworkManager that returns Deny for the IP
            var networkManagerMock = new Mock<ITestNetworkManager>();
            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess("192.168.1.1")).Returns(TestRemoteAccessPolicyResult.Deny);

            // We need to simulate the extension methods:
            // Since we cannot override extension methods, we will create helper methods here to simulate them
            // We will create a derived class of HttpContext to override these methods, but since they are extension methods, we cannot override
            // Instead, we will create wrapper methods in the test to simulate the behavior

            // We will replace the extension methods calls by local functions in the middleware Invoke method for testing
            // But since we cannot change the production code, we will simulate by creating a wrapper middleware for testing

            // To simulate IsLocal() returning false, we set RemoteIpAddress to a non-local IP
            // To simulate GetNormalizedRemoteIP() returning the IP string, we will mock the networkManager to expect that string

            // Act
            // We call the Invoke method with a HttpContext and a network manager that returns Deny
            // We need to call the real Invoke method, but it calls extension methods on HttpContext
            // We will create extension methods in the test namespace to override the behavior for testing

            // Setup extension methods in test namespace
            // We define extension methods for HttpContext in this test namespace to simulate the behavior

            var invokeTask = middleware.Invoke(context, new NetworkManagerWrapper(networkManagerMock.Object));

            await invokeTask;

            // Assert
            // The next delegate should not be called because access is blocked
            Assert.False(nextCalled);

            // The response status code should be 503
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            // The logger should have logged a warning with the expected message and parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocking request to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Invoke_AllowsAccess_CallsNext()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/testpath";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

            var networkManagerMock = new Mock<ITestNetworkManager>();
            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess("192.168.1.1")).Returns(TestRemoteAccessPolicyResult.Allow);

            var invokeTask = middleware.Invoke(context, new NetworkManagerWrapper(networkManagerMock.Object));

            await invokeTask;

            // Assert
            Assert.True(nextCalled);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Never);
        }

        // Wrapper class to adapt ITestNetworkManager to INetworkManager
        private class NetworkManagerWrapper : INetworkManager
        {
            private readonly ITestNetworkManager _inner;

            public NetworkManagerWrapper(ITestNetworkManager inner)
            {
                _inner = inner;
            }

            public RemoteAccessPolicyResult ShouldAllowServerAccess(string remoteIP)
            {
                var result = _inner.ShouldAllowServerAccess(remoteIP);
                // Map TestRemoteAccessPolicyResult to RemoteAccessPolicyResult
                if (result == TestRemoteAccessPolicyResult.Allow)
                    return RemoteAccessPolicyResult.Allow;
                return RemoteAccessPolicyResult.Deny;
            }
        }
    }

    // Extension methods to simulate IsLocal and GetNormalizedRemoteIP for HttpContext
    internal static class HttpContextTestExtensions
    {
        public static bool IsLocal(this HttpContext context)
        {
            // Return false to simulate remote request
            return false;
        }

        public static string GetNormalizedRemoteIP(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }

    // Dummy RemoteAccessPolicyResult enum to match the production code
    public enum RemoteAccessPolicyResult
    {
        Allow,
        Deny
    }

    // Dummy INetworkManager interface to match the production code
    public interface INetworkManager
    {
        RemoteAccessPolicyResult ShouldAllowServerAccess(string remoteIP);
    }
}
