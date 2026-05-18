using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using MediaBrowser.Controller.Extensions;
using System.Collections.Generic;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        private class TestConfiguration : IConfiguration
        {
            private readonly Dictionary<string, string> _data = new Dictionary<string, string>();

            public TestConfiguration(Dictionary<string, string> data)
            {
                _data = data;
            }

            public string this[string key]
            {
                get => _data.ContainsKey(key) ? _data[key] : null;
                set => _data[key] = value;
            }

            public IEnumerable<IConfigurationSection> GetChildren()
            {
                throw new System.NotImplementedException();
            }

            public IChangeToken GetReloadToken()
            {
                return new CancellationChangeToken(System.Threading.CancellationToken.None);
            }

            public IConfigurationSection GetSection(string key)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public void GetFFmpegProbeSize_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegProbeSizeKey, "12345" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetFFmpegProbeSize(configuration);

            // Assert
            Assert.Equal("12345", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegAnalyzeDurationKey, "60" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetFFmpegAnalyzeDuration(configuration);

            // Assert
            Assert.Equal("60", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegSkipValidationKey, "true" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetFFmpegSkipValidation(configuration);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(configuration);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.BindToUnixSocketKey, "true" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.UseUnixSocket(configuration);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetUnixSocketPath_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPathKey, "/var/run/jellyfin.sock" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetUnixSocketPath(configuration);

            // Assert
            Assert.Equal("/var/run/jellyfin.sock", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPermissionsKey, "0777" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetUnixSocketPermissions(configuration);

            // Assert
            Assert.Equal("0777", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ShouldReturnCorrectValue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { ConfigurationExtensions.SqliteCacheSizeKey, "2000" }
            };
            var configuration = new TestConfiguration(data);

            // Act
            var result = ConfigurationExtensions.GetSqliteCacheSize(configuration);

            // Assert
            Assert.Equal(2000, result);
        }
    }
}
