using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _optionsMock = new Mock<IOptions<AbpCliOptions>>();
    }

    [Fact]
    public async Task GetAsync_Should_LogWarning_When_LatestVersion_Is_Null_And_No_Version_Specified()
    {
        // Arrange
        _cancellationTokenProviderMock.Setup(x => x.Token).Returns(CancellationToken.None);
        
        var store = new AbpIoSourceCodeStoreTestDouble(
            _optionsMock.Object,
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cancellationTokenProviderMock.Object);

        store.Logger = _loggerMock.Object;
        store.SetLatestVersionResult(null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => store.GetAsync("test-template", SourceCodeTypes.Template, version: null));

        // Verify the specific LogWarning call was made
        _loggerMock.Verify(
            x => x.LogWarning(
                "The remote service is currently unavailable, please specify the version."),
            Times.Once);
    }

    [Fact]
    public void Constructor_Should_Set_Logger_To_NullLogger_By_Default()
    {
        // Arrange & Act
        var store = new AbpIoSourceCodeStoreTestDouble(
            _optionsMock.Object,
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cancellationTokenProviderMock.Object);

        // Assert
        Assert.IsType<NullLogger<AbpIoSourceCodeStore>>(store.Logger);
    }
}

public class AbpIoSourceCodeStoreTestDouble : AbpIoSourceCodeStore
{
    private string _latestVersionResult;

    public AbpIoSourceCodeStoreTestDouble(
        IOptions<AbpCliOptions> options,
        IJsonSerializer jsonSerializer,
        IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, 
               new DummyCliHttpClientFactory(), new DummyCliVersionService())
    {
    }

    public void SetLatestVersionResult(string result)
    {
        _latestVersionResult = result;
    }

    protected new Task<string> GetLatestSourceCodeVersionAsync(
        string name, string type, string templateSource, bool includePreReleases)
    {
        return Task.FromResult(_latestVersionResult);
    }

    protected new List<TemplateFile> GetLocalTemplates()
    {
        return new List<TemplateFile>();
    }
}

public class DummyCliHttpClientFactory
{
}

public class DummyCliVersionService
{
    public Task<object> GetCurrentCliVersionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object>(null);
    }
}
