using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ReturnsCorrectValue_WhenKeyExists()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:probesize", "10M" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Equal("10M", result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsCorrectValue_WhenKeyExists()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:analyzeduration", "30" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Equal("30", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenKeyIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:novalidation", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyIsFalse()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:novalidation", "false" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeyIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:imgExtractPerfTradeoff", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyIsFalse()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "FFmpeg:imgExtractPerfTradeoff", "false" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenKeyIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "kestrel:socket", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyIsFalse()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "kestrel:socket", "false" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsCorrectValue_WhenKeyExists()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "kestrel:socketPath", "/tmp/jellyfin.sock" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Equal("/tmp/jellyfin.sock", result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsCorrectValue_WhenKeyExists()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "kestrel:socketPermissions", "0777" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Equal("0777", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsCorrectValue_WhenKeyExists()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "sqlite:cacheSize", "10000" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.GetSqliteCacheSize();

            // Assert
            Assert.Equal(10000, result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var result = configuration.GetSqliteCacheSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void HostWebClient_ReturnsTrue_WhenKeyIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "hostwebclient", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.HostWebClient();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HostWebClient_ReturnsFalse_WhenKeyIsFalse()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "hostwebclient", "false" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = configuration.HostWebClient();

            // Assert
            Assert.False(result);
        }
    }
}
