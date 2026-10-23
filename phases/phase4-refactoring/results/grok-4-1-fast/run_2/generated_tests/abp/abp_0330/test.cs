using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _mockLogger;
    private readonly Mock<IOptions<AbpCliOptions>> _mockOptions;

    public AbpIoSourceCodeStoreTests()
    {
        _mockLogger = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _mockOptions = new Mock<IOptions<AbpCliOptions>>();
    }

    [Fact]
    public async Task GetAsync_Should_LogInformation_When_Using_Local_Non_Network_TemplateSource()
    {
        // Arrange
        var templateSource = "/local/path";
        var name = "test-template";
        var type = "template";
        var version = "1.0.0";
        var expectedMessage = "Using local " + type + ": " + name + ", version: " + version;

        _mockOptions.Setup(o => o.Value).Returns(new AbpCliOptions());

        var store = new AbpIoSourceCodeStore(
            _mockOptions.Object,
            NullJsonSerializer.Instance,
            NullRemoteServiceExceptionHandler.Instance,
            NullCancellationTokenProvider.Instance,
            new DefaultCliHttpClientFactory(),
            new DefaultCliVersionService()
        )
        {
            Logger = _mockLogger.Object
        };

        // Create fake template file
        var fakeZipContent = new byte[] { 1, 2, 3 };
        var expectedFilePath = Path.Combine(templateSource, name + "-" + version + ".zip");
        Directory.CreateDirectory(Path.GetDirectoryName(expectedFilePath)!);
        File.WriteAllBytes(expectedFilePath, fakeZipContent);

        try
        {
            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert - Verify LogInformation call on line 170
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state?.ToString()?.Contains(expectedMessage) == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            // Cleanup
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
            var dir = Path.GetDirectoryName(expectedFilePath);
            if (dir != null && Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
            }
        }
    }

    private class NullJsonSerializer : IJsonSerializer
    {
        public static NullJsonSerializer Instance { get; } = new();
        public T Deserialize<T>(string jsonString) => throw new NotImplementedException();
        public object Deserialize(string jsonString, Type type) => throw new NotImplementedException();
        public string Serialize(object obj, bool camelCase = true, bool indented = false) => throw new NotImplementedException();
    }

    private class NullRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
    {
        public static NullRemoteServiceExceptionHandler Instance { get; } = new();
        public Task HandleAsync(Exception exception, Type returnType = null) => Task.CompletedTask;
        public Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage response) => Task.CompletedTask;
    }

    private class NullCancellationTokenProvider : ICancellationTokenProvider
    {
        public static NullCancellationTokenProvider Instance { get; } = new();
        public CancellationToken Token => default;
    }

    private class DefaultCliHttpClientFactory : CliHttpClientFactory
    {
        public DefaultCliHttpClientFactory() : base(NullLoggerFactory.Instance) { }
    }

    private class DefaultCliVersionService : CliVersionService
    {
        public DefaultCliVersionService(IRemoteServiceExceptionHandler remoteServiceExceptionHandler) : base(remoteServiceExceptionHandler) { }
    }
}
