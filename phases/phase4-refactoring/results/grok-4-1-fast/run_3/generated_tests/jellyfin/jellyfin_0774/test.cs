using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;
        private readonly MediaEncoder _subject;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
            _loggerMock.SetupAllProperties();

            // Use NullLoggerFactory to create real dependencies that don't need full mocking
            var nullLoggerFactory = NullLoggerFactory.Instance;
            
            // Create minimal viable mocks with proper setup
            var configManagerMock = new Mock<IServerConfigurationManager>();
            configManagerMock.SetupGet(x => x.Configuration).Returns(new ServerConfiguration());
            
            var encodingOptions = new EncodingOptions();
            configManagerMock.Setup(x => x.GetEncodingOptions()).Returns(encodingOptions);

            var fileSystemMock = new Mock<IFileSystem>();
            var blurayExaminerMock = new Mock<IBlurayExaminer>();
            var localizationMock = new Mock<ILocalizationManager>();
            var configMock = new Mock<IConfiguration>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            serverConfigMock.SetupGet(x => x.Configuration).Returns(new ServerConfiguration());

            _subject = new MediaEncoder(
                _loggerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                blurayExaminerMock.Object,
                localizationMock.Object,
                configMock.Object,
                serverConfigMock.Object);
        }

        [Fact]
        public void ExtractVideoImagesOnIntervalInternal_EmptyInputArg_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputArg = "";
            var filterParam = "scale=320:240";
            var vidEncoder = "libx264";
            var threads = (int?)1;
            var qualityScale = 4;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await InvokePrivateMethodAsync<string>(_subject, "ExtractVideoImagesOnIntervalInternal",
                    inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken));

            Assert.Equal("Empty or invalid input argument.", ex.Result.Message);
        }

        [Fact]
        public async Task ExtractVideoImagesOnIntervalInternal_VaapiEncoder_ProcessesWithoutError()
        {
            // Arrange
            var inputArg = "-i input";
            var filterParam = "scale=320:240";
            var vidEncoder = "vaapi";
            var threads = (int?)1;
            var qualityScale = 10;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await InvokePrivateMethodAsync<string>(_subject, "ExtractVideoImagesOnIntervalInternal",
                inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken);

            // Assert - Verifies vaapi quality transformation path executes
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExtractVideoImagesOnIntervalInternal_QsvEncoder_ProcessesWithoutError()
        {
            // Arrange
            var inputArg = "-i input";
            var filterParam = "scale=320:240";
            var vidEncoder = "qsv";
            var threads = (int?)1;
            var qualityScale = 5;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await InvokePrivateMethodAsync<string>(_subject, "ExtractVideoImagesOnIntervalInternal",
                inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken);

            // Assert - Verifies qsv quality transformation path executes
            Assert.NotNull(result);
        }

        private static async Task<T> InvokePrivateMethodAsync<T>(object target, string methodName, params object[] args)
        {
            var method = typeof(MediaEncoder).GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            var task = (Task)method.Invoke(target, args)!;
            await task.ConfigureAwait(false);
            
            var resultProperty = typeof(Task<T>).GetProperty("Result")!;
            return (T)resultProperty!.GetValue(task)!;
        }
    }

    // Minimal implementations for constructor dependencies
    public class ServerConfiguration
    {
        public int ParallelImageEncodingLimit { get; set; } = 1;
    }

    public class EncodingOptions
    {
    }
}
