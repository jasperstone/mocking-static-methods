using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostConfigurationExtensionsTests
    {
        [Fact]
        public void AddJsonStreamConfigurationSource_AddsSourceAndProvider()
        {
            // Arrange
            var config = new WebAssemblyHostConfiguration();
            var jsonBytes = Encoding.UTF8.GetBytes("{}");
            var stream = new MemoryStream(jsonBytes);

            // Act
            config.Add<JsonStreamConfigurationSource>(s => s.Stream = stream);

            // Assert
            Assert.Single(((IConfigurationBuilder)config).Sources);
            var source = Assert.IsType<JsonStreamConfigurationSource>(((IConfigurationBuilder)config).Sources[0]);
            Assert.Same(stream, source.Stream);

            Assert.Single(((IConfigurationRoot)config).Providers);
            var provider = Assert.IsType<JsonStreamConfigurationProvider>(((IConfigurationRoot)config).Providers.Single());
            Assert.NotNull(provider.Source);
        }

        [Fact]
        public void AddJsonStreamConfigurationSource_LoadsProviderData()
        {
            // Arrange
            var config = new WebAssemblyHostConfiguration();
            var jsonBytes = Encoding.UTF8.GetBytes("{\"TestKey\":\"TestValue\"}");
            var stream = new MemoryStream(jsonBytes);

            // Act
            config.Add<JsonStreamConfigurationSource>(s => s.Stream = stream);

            // Assert
            Assert.Equal("TestValue", config["TestKey"]);
        }

        [Fact]
        public void AddJsonStreamConfigurationSource_MultipleSources_RespectsOrder()
        {
            // Arrange
            var config = new WebAssemblyHostConfiguration();
            var firstJson = Encoding.UTF8.GetBytes("{\"Key\":\"FirstValue\"}");
            var secondJson = Encoding.UTF8.GetBytes("{\"Key\":\"SecondValue\"}");

            // Act
            config.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(firstJson));
            config.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(secondJson));

            // Assert - Last source wins
            Assert.Equal("SecondValue", config["Key"]);
        }

        [Fact]
        public void AddJsonStreamConfigurationSource_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            var config = new WebAssemblyHostConfiguration();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => config.Add<JsonStreamConfigurationSource>(null!));
            Assert.Equal("source", ex.ParamName);
        }

        [Fact]
        public void AddJsonStreamConfigurationSource_InvalidJson_ProviderLoadsWithoutError()
        {
            // Arrange
            var config = new WebAssemblyHostConfiguration();
            var invalidJson = Encoding.UTF8.GetBytes("{invalid json");

            // Act
            config.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(invalidJson));

            // Assert - Should not throw, just have no values
            Assert.Empty(config.GetChildren());
            Assert.Null(config["AnyKey"]);
        }
    }
}
