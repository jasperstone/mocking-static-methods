using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpRequestJsonExtensionsTests
    {
        private class DummyOptions
        {
            public JsonSerializerOptions Value { get; set; } = new JsonSerializerOptions();
        }

        private class DummyService
        {
        }

        private class DummyServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IOptions<JsonOptions>))
                {
                    return Options;
                }
                if (serviceType == typeof(DummyService))
                {
                    return new DummyService();
                }
                return null;
            }

            public IOptions<JsonOptions> Options { get; } = Options.Create(new JsonOptions());
        }

        private class TestHttpContext : HttpContext
        {
            public override IServiceProvider RequestServices { get; set; } = new DummyServiceProvider();

            // Other members can throw NotImplementedException as they are not used in tests
            public override HttpRequest Request { get; } = new DefaultHttpContext().Request;
            public override HttpResponse Response { get; } = new DefaultHttpContext().Response;
            public override ConnectionInfo Connection => throw new NotImplementedException();
            public override WebSocketManager WebSockets => throw new NotImplementedException();
            public override ClaimsPrincipal User { get; set; } = new System.Security.Claims.ClaimsPrincipal();
            public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
            public override IServiceProvider RequestServices { get; set; }
            public override CancellationToken RequestAborted { get; set; }
            public override string TraceIdentifier { get; set; } = Guid.NewGuid().ToString();
            public override ISession Session { get; set; } = new DummySession();

            // ... other members omitted for brevity
        }

        private class DummySession : ISession
        {
            public bool IsAvailable => true;
            public string Id => "dummy";
            public IEnumerable<string> Keys => Enumerable.Empty<string>();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Clear() { }
            public void Remove(string key) { }
            public void Set(string key, byte[] value) { }
            public bool TryGetValue(string key, out byte[] value)
            {
                value = null!;
                return false;
            }
        }

        [Fact]
        public async Task ResolveSerializerOptions_ReturnsOptions_FromRequestServices()
        {
            // Arrange
            var context = new TestHttpContext();
            var request = context.Request;

            // Act
            var options = HttpRequestJsonExtensions.ResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(options);
            Assert.IsType<JsonSerializerOptions>(options);
        }

        [Fact]
        public async Task GetService_CalledOnRequest_ReturnsExpectedService()
        {
            // Arrange
            var context = new TestHttpContext();
            var request = context.Request;

            // Act
            var service = request.HttpContext.RequestServices.GetService(typeof(DummyService));

            // Assert
            Assert.NotNull(service);
            Assert.IsType<DummyService>(service);
        }
    }
}
