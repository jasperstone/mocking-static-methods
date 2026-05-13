using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebAssemblyHostBuilderTests
{
    public class ConfigurationExtensionTests
    {
        [Fact]
        public void AddJsonStream_ShouldAddConfigurationSource()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();

            // Act
            configurationBuilder.Add<JsonStreamConfigurationSource>(s =>
            {
                var jsonBytes = System.Text.Encoding.UTF8.GetBytes("{\"Key\":\"Value\"}");
                s.Stream = new MemoryStream(jsonBytes);
            });

            var configurationRoot = configurationBuilder.Build();

            // Assert
            Assert.NotNull(configurationRoot);
            Assert.Equal("Value", configurationRoot["Key"]);
        }
    }
}
