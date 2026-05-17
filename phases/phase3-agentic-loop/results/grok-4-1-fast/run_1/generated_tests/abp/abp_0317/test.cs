using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IO;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly Mock<object> _jsonSerializerMock;
    private readonly Mock<object> _remoteServiceExceptionHandlerMock;
    private readonly Mock<object> _cancellationTokenProviderMock;
    private readonly Mock<object> _cliHttpClientFactoryMock;
    private readonly Mock<object> _cliVersionServiceMock;
    private readonly AbpCliOptions _options;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _jsonSerializerMock = new Mock<object>();
        _remoteServiceExceptionHandlerMock = new Mock<object>();
        _cancellationTokenProviderMock = new Mock<object>();
        _cliHttpClientFactoryMock = new Mock<object>();
        _cliVersionServiceMock = new Mock<object>();
        _options = new AbpCliOptions();
    }

    [Fact]
    public async Task GetAsync_ShouldLogWarningAndThrowException_WhenLatestVersionIsNull()
    {
        // Arrange
        var store = CreateStore();
        
        // Mock the GetLatestSourceCodeVersionAsync call - since it's internal/protected, we override behavior
        // by making the store think latestVersion is null when version is null
        store.GetType().GetProperty("CliVersionService")?.SetValue(store, _cliVersionServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => store.GetAsync("test", "template")
        );
        
        _loggerMock.Verify(
            x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
            Times.Once
        );
        _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.AtLeast(4));
        Assert.Contains("abp new", exception.Message);
    }

    [Fact]
    public async Task GetAsync_ShouldNotLogSpecificWarning_WhenLatestVersionIsAvailable()
    {
        // Arrange
        var store = CreateStore();

        // Act
        await store.GetAsync("test", "template", version: "1.0.0");

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
            Times.Never
        );
    }

    private AbpIoSourceCodeStore CreateStore()
    {
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            (dynamic)_jsonSerializerMock.Object,
            (dynamic)_remoteServiceExceptionHandlerMock.Object,
            (dynamic)_cancellationTokenProviderMock.Object,
            (dynamic)_cliHttpClientFactoryMock.Object,
            (dynamic)_cliVersionServiceMock.Object
        );
        store.Logger = _loggerMock.Object;
        return store;
    }
}
