using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_UsesResolvedSerializerOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var jsonOptionsMock = new Mock<IOptions<JsonOptions>>();
            var jsonOptions = new JsonOptions();
            jsonOptionsMock.Setup(o => o.Value).Returns(jsonOptions);
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(jsonOptionsMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(r => r.HttpContext).Returns(httpContextMock.Object);
            responseMock.Setup(r => r.BodyWriter).Returns(new PipeWriter(new Stream()));

            var value = new { Foo = "bar" };

            // Act
            await responseMock.Object.WriteAsJsonAsync(value);

            // Assert
            jsonOptionsMock.Verify(o => o.Value, Times.Once);
        }

        [Fact]
        public async Task WriteAsJsonAsync_UsesDefaultSerializerOptions_WhenNoOptionsAreResolved()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(r => r.HttpContext).Returns(httpContextMock.Object);
            responseMock.Setup(r => r.BodyWriter).Returns(new PipeWriter(new Stream()));

            var value = new { Foo = "bar" };

            // Act
            await responseMock.Object.WriteAsJsonAsync(value);

            // Assert
            serviceProviderMock.Verify(s => s.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }
    }
}
