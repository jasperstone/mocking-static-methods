using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _optionsMock = new Mock<IOptions<AbpCliOptions>>();
    }

    [Fact]
    public async Task GetAsync_Should_LogInformation_For_Local_Non_Network_TemplateSource()
    {
        // Arrange
        var templateSource = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var name = "test-template";
        var type = "template";
        var version = "1.0.0";
        var fakeZipContent = new byte[] { 1, 2, 3 };

        Directory.CreateDirectory(templateSource);
        var expectedFilePath = Path.Combine(templateSource, name + "-" + version + ".zip");
        File.WriteAllBytes(expectedFilePath, fakeZipContent);

        var options = new AbpCliOptions();
        _optionsMock.Setup(o => o.Value).Returns(options);

        // Use NullLogger instances and minimal mocks that won't throw
        var store = new AbpIoSourceCodeStore(
            _optionsMock.Object,
            new MockJsonSerializer(),
            new MockRemoteServiceExceptionHandler(),
            new MockCancellationTokenProvider(),
            new MockCliHttpClientFactory(),
            new MockCliVersionService()
        );
        store.Logger = _loggerMock.Object;

        // Act
        var result = await store.GetAsync(name, type, version, templateSource);

        // Assert - Verify LogInformation was called for local non-network source (line ~170)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state?.ToString().Contains("Using local template: test-template, version: 1.0.0") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.Equal(version, result.Version);
        Assert.Equal(fakeZipContent.Length, result.FileContent.Length);

        // Cleanup
        try { Directory.Delete(templateSource, true); } catch { }
    }
}

// Minimal implementations to satisfy constructor requirements without external dependencies
public class MockJsonSerializer : IJsonSerializer
{
    public string Serialize(object obj, bool camelCase = true, bool indented = false) => "{}";
    public T Deserialize<T>(string json, bool camelCase = true) => default!;
    public string SerializeToJsonString(object obj, bool camelCase = true, bool indented = false) => "{}";
    public T DeserializeToObject<T>(string json, bool camelCase = true) => default!;
}

public class MockRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
{
    public Task Handle(Exception exception) => Task.CompletedTask;
    public Task EnsureSuccessfulHttpResponseAsync(System.Net.Http.HttpResponseMessage responseMessage) => Task.CompletedTask;
    public Task<RemoteServiceErrorInfo> GetAbpRemoteServiceErrorAsync(System.Net.Http.HttpResponseMessage responseMessage) 
        => Task.FromResult(new RemoteServiceErrorInfo());
}

public class MockCancellationTokenProvider : ICancellationTokenProvider
{
    public CancellationToken Token => CancellationToken.None;
}

public class MockCliHttpClientFactory
{
    // Minimal implementation - constructor doesn't call methods
}

public class MockCliVersionService
{
    // Minimal implementation - we bypass version checks with provided version
}

// Minimal types for RemoteServiceErrorInfo
public class RemoteServiceErrorInfo
{
    public string Message { get; set; } = "";
    public string Code { get; set; } = "";
    public string Details { get; set; } = "";
    public Dictionary<string, object> Data { get; set; } = new();
}
