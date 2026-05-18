using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_CallsGetServiceAndSetsContentType()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var context = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            response.HttpContext = context;

            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal(ContentTypeConstants.JsonContentTypeWithCharset, response.ContentType);
        }
    }
}
