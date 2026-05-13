using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        private class DummyHttpResponse : HttpResponse
        {
            private readonly HttpContext _context;
            private string _contentType = null!;
            private readonly PipeWriter _pipeWriter;

            public DummyHttpResponse(HttpContext context)
            {
                _context = context;
                var pipe = new Pipe();
                _pipeWriter = pipe.Writer;
            }

            public override HttpContext HttpContext => _context;

            public override string ContentType
            {
                get => _contentType;
                set => _contentType = value;
            }

            public override PipeWriter BodyWriter => _pipeWriter;

            #region NotImplementedMembers
            public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override IHeaderDictionary Headers => throw new NotImplementedException();
            public override Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override bool HasStarted => throw new NotImplementedException();
            public override void OnCompleted(Func<object, Task> callback, object state) => throw new NotImplementedException();
            public override void OnStarting(Func<object, Task> callback, object state) => throw new NotImplementedException();
            public override void Redirect(string location, bool permanent) => throw new NotImplementedException();
            #endregion
        }

        private class DummyHttpContext : HttpContext
        {
            private readonly IServiceProvider _serviceProvider;

            public DummyHttpContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
                RequestAborted = CancellationToken.None;
            }

            public override IServiceProvider RequestServices
            {
                get => _serviceProvider;
                set => throw new NotImplementedException();
            }

            public override CancellationToken RequestAborted { get; set; }

            #region NotImplementedMembers
            public override IFeatureCollection Features => throw new NotImplementedException();
            public override HttpRequest Request => throw new NotImplementedException();
            public override HttpResponse Response => throw new NotImplementedException();
            public override ConnectionInfo Connection => throw new NotImplementedException();
            public override WebSocketManager WebSockets => throw new NotImplementedException();
            public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override IDictionary<object, object?> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override void Abort() => throw new NotImplementedException();
            #endregion
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsOptionsFromDI_WhenAvailable()
        {
            // Arrange
            var expectedOptions = new JsonSerializerOptions();
            var jsonOptions = new JsonOptions { SerializerOptions = expectedOptions };
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(jsonOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var actualOptions = InvokeResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(expectedOptions, actualOptions);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptions_WhenDIIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var actualOptions = InvokeResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, actualOptions);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptions_WhenRequestServicesIsNull()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns((IServiceProvider?)null);

            // Act
            var actualOptions = InvokeResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, actualOptions);
        }

        private static JsonSerializerOptions InvokeResolveSerializerOptions(HttpContext httpContext)
        {
            // Use reflection to invoke the private static method ResolveSerializerOptions
            var method = typeof(HttpResponseJsonExtensions).GetMethod("ResolveSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            var result = method.Invoke(null, new object[] { httpContext });
            return (JsonSerializerOptions)result!;
        }
    }
}
