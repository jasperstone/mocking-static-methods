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
        public async Task ResolveSerializerOptions_ResolvesOptionsFromDI()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<JsonOptions>()
                .Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(serializerOptions);
            Assert.True(serializerOptions.WriteIndented);
        }

        [Fact]
        public async Task ResolveSerializerOptions_ReturnsDefaultOptionsIfNotResolvedFromDI()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // Act
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(serializerOptions);
            Assert.Equal(JsonSerializerDefaults.Web, serializerOptions);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WritesJsonToResponse()
        {
            // Arrange
            var response = new HttpResponse(new Stream());
            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            // Verify that the response body contains the expected JSON
            var responseBody = await response.BodyReader.ReadAsync();
            var json = Encoding.UTF8.GetString(responseBody.Buffer.FirstSpan);
            Assert.Equal("{\"Foo\":\"bar\"}", json);
        }

        private class Stream : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => 0;
            public override long Position { get => 0; set => throw new NotImplementedException(); }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                BodyReader = new PipeReader(new MemoryStream(buffer));
            }

            public PipeWriter BodyWriter { get; } = new PipeWriter(new MemoryStream());
            public PipeReader BodyReader { get; private set; }
        }
    }
}
