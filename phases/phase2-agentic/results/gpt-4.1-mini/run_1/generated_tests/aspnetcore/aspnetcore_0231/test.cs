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

namespace Microsoft.AspNetCore.Http
{
    public class HttpResponseJsonExtensionsTests
    {
        private class DummyHttpResponse : HttpResponse
        {
            private readonly HttpContext _context;
            private string _contentType = null!;
            private readonly PipeWriter _pipeWriter = new Pipe().Writer;

            public DummyHttpResponse(HttpContext context)
            {
                _context = context;
            }

            public override HttpContext HttpContext => _context;
            public override int StatusCode { get; set; }
            public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
            public override Stream Body { get; set; } = new System.IO.MemoryStream();
            public override long? ContentLength { get; set; }
            public override string ContentType
            {
                get => _contentType;
                set => _contentType = value;
            }
            public override IResponseCookies Cookies => throw new NotImplementedException();
            public override bool HasStarted => false;
            public override PipeWriter BodyWriter => _pipeWriter;

            public override void OnStarting(Func<object, Task> callback, object state) => throw new NotImplementedException();
            public override void OnCompleted(Func<object, Task> callback, object state) => throw new NotImplementedException();
            public override void Redirect(string location, bool permanent) => throw new NotImplementedException();
        }

        private class DummyHttpContext : HttpContext
        {
            private readonly IServiceProvider _serviceProvider;

            public DummyHttpContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override IServiceProvider RequestServices
            {
                get => _serviceProvider;
                set => throw new NotImplementedException();
            }

            public override IFeatureCollection Features => throw new NotImplementedException();
            public override HttpRequest Request => throw new NotImplementedException();
            public override HttpResponse Response => throw new NotImplementedException();
            public override ConnectionInfo Connection => throw new NotImplementedException();
            public override WebSocketManager WebSockets => throw new NotImplementedException();
            public override ClaimsPrincipal User { get; set; } = null!;
            public override IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();
            public override CancellationToken RequestAborted { get; set; }
            public override string TraceIdentifier { get; set; } = null!;
            public override ISession Session { get; set; } = null!;
            public override void Abort() => throw new NotImplementedException();
        }

        [Fact]
        public void WriteAsJsonAsync_UsesResolvedSerializerOptions_WhenOptionsIsNull()
        {
            // Arrange
            var expectedOptions = new JsonSerializerOptions();
            var jsonOptionsMock = new Mock<IOptions<JsonOptions>>();
            jsonOptionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = expectedOptions });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(jsonOptionsMock.Object);

            var httpContext = new DummyHttpContext(serviceProviderMock.Object)
            {
                RequestAborted = CancellationToken.None
            };

            var response = new DummyHttpResponse(httpContext);

            var value = new { Name = "Test" };

            // Act
            var task = HttpResponseJsonExtensions.WriteAsJsonAsync(response, value, options: null, contentType: null, cancellationToken: new CancellationToken(true));

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            // The returned task should not be null
            Assert.NotNull(task);
            // Verify that GetService was called for IOptions<JsonOptions>
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }

        [Fact]
        public void WriteAsJsonAsync_SetsContentTypeToProvidedValue()
        {
            // Arrange
            var options = new JsonSerializerOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContext = new DummyHttpContext(serviceProviderMock.Object)
            {
                RequestAborted = CancellationToken.None
            };
            var response = new DummyHttpResponse(httpContext);
            var value = new { Name = "Test" };
            var contentType = "application/custom+json";

            // Act
            var task = HttpResponseJsonExtensions.WriteAsJsonAsync(response, value, options, contentType, cancellationToken: new CancellationToken(true));

            // Assert
            Assert.Equal(contentType, response.ContentType);
            Assert.NotNull(task);
        }

        [Fact]
        public void WriteAsJsonAsync_ThrowsArgumentNullException_WhenResponseIsNull()
        {
            // Arrange
            HttpResponse? response = null;
            var value = new { Name = "Test" };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => HttpResponseJsonExtensions.WriteAsJsonAsync(response!, value));
        }
    }
}
