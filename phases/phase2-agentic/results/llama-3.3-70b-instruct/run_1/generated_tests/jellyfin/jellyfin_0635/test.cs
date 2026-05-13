using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsNull_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsNull_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenKeyIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetSqliteCacheSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("FFmpeg:probesize", "12345") })
                .Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Equal("12345", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("FFmpeg:analyzeduration", "12345") })
                .Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Equal("12345", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenKeyIsPresentAndTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("FFmpeg:novalidation", "true") })
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeyIsPresentAndTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("FFmpeg:imgExtractPerfTradeoff", "true") })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenKeyIsPresentAndTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("kestrel:socket", "true") })
                .Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("kestrel:socketPath", "/path/to/socket") })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Equal("/path/to/socket", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("kestrel:socketPermissions", "permissions") })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Equal("permissions", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("sqlite:cacheSize", "12345") })
                .Build();

            // Act
            var result = configuration.GetSqliteCacheSize();

            // Assert
            Assert.Equal(12345, result);
        }
    }
}
