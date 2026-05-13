using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Extensions.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_CallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            services.AddSingleton<IOptions<JsonOptions>>(new OptionsWrapper<JsonOptions>(new JsonOptions { SerializerOptions = options }));
            var serviceProvider = services.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;
            var httpContext = context;

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(options, result);
        }
    }
}
