using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_ResolvesSerializerOptionsFromDI()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<JsonOptions>()
                .Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions.PropertyNameCaseInsensitive = true;
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var response = new HttpResponse(new MemoryStream());
            response.HttpContext = httpContext;

            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            var json = await ReadResponseBody(response);
            Assert.Equal("{\"foo\":\"bar\"}", json);
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            var stream = (MemoryStream)response.Body;
            stream.Position = 0;
            var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
