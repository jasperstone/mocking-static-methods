using Xunit;
using Microsoft.Extensions.Configuration;
using System;

namespace MediaBrowser.Controller.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:probesize", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsNull_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:analyzeduration", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:novalidation", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:imgExtractPerfTradeoff", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socket", null) })
                .Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPath", null) })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsNull_WhenKeyIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPermissions", null) })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:probesize", "12345") })
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
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:analyzeduration", "12345") })
                .Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Equal("12345", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:novalidation", "true") })
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:imgExtractPerfTradeoff", "true") })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenKeyIsPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socket", "true") })
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
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPath", "/path/to/socket") })
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
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPermissions", "permissions") })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Equal("permissions", result);
        }
    }
}
