using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_UsesRequestServicesGetService_WhenOptionsNotProvided()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            jsonOptions.SerializerOptions = new JsonSerializerOptions { WriteIndented = true };

            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(jsonOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);
            httpContextMock.SetupGet(c => c.RequestAborted).Returns(CancellationToken.None);

            var pipe = new Pipe();
            var bodyWriter = pipe.Writer;

            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(r => r.HttpContext).Returns(httpContextMock.Object);
            responseMock.SetupProperty(r => r.ContentType);
            responseMock.SetupGet(r => r.BodyWriter).Returns(bodyWriter);

            var value = new { Name = "Test" };

            // Act
            var task = HttpResponseJsonExtensions.WriteAsJsonAsync(responseMock.Object, value, options: null, contentType: null, cancellationToken: CancellationToken.None);
            await task;

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
            Assert.Equal("application/json; charset=utf-8", responseMock.Object.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_UsesProvidedOptions_WhenOptionsProvided()
        {
            // Arrange
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns((IServiceProvider)null);
            httpContextMock.SetupGet(c => c.RequestAborted).Returns(CancellationToken.None);

            var pipe = new Pipe();
            var bodyWriter = pipe.Writer;

            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(r => r.HttpContext).Returns(httpContextMock.Object);
            responseMock.SetupProperty(r => r.ContentType);
            responseMock.SetupGet(r => r.BodyWriter).Returns(bodyWriter);

            var value = new { Name = "Test" };

            // Act
            var task = HttpResponseJsonExtensions.WriteAsJsonAsync(responseMock.Object, value, jsonOptions, contentType: null, cancellationToken: CancellationToken.None);
            await task;

            // Assert
            Assert.Equal("application/json; charset=utf-8", responseMock.Object.ContentType);
        }
    }
}
