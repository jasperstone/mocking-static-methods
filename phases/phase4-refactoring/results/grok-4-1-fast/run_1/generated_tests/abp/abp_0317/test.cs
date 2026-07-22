using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _mockLogger;
    private readonly Mock<IOptions<AbpCliOptions>> _mockOptions;
    private readonly Mock<IJsonSerializer> _mockJsonSerializer;
    private readonly Mock<IRemoteServiceExceptionHandler> _mockRemoteServiceExceptionHandler;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<object> _mockCliHttpClientFactory;
    private readonly Mock<object> _mockCliVersionService;

    public AbpIoSourceCodeStoreTests()
    {
        _mockLogger = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _mockOptions = new Mock<IOptions<AbpCliOptions>>();
        _mockJsonSerializer = new Mock<IJsonSerializer>();
        _mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockCliHttpClientFactory = new Mock<object>();
        _mockCliVersionService = new Mock<object>();
    }

    [Fact]
    public async Task GetAsync_Should_LogWarning_When_LatestVersion_Is_Null_And_No_Version_Specified()
    {
        // Arrange
        SetupMocks();

        var mockStore = CreateMockStore();
        mockStore.Protected()
                 .Setup<Task<string>>("GetLatestSourceCodeVersionAsync", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>())
                 .ReturnsAsync((string name, string type, string version, bool includePreReleases) => null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => mockStore.Object.GetAsync("test", SourceCodeTypes.Template, version: null));

        _mockLogger.Verify(
            x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_LogMultipleWarnings_When_LatestVersion_Is_Null_And_Cache_Exists()
    {
        // Arrange
        SetupMocks();

        // Mock GetLocalTemplates to return fake data
        var fakeTemplates = new List<TemplateDefinitionCacheItem>
        {
            new() { TemplateName = "test", Version = "1.0.0" }
        };

        var mockStore = CreateMockStore();
        mockStore.Protected()
                 .Setup<Task<string>>("GetLatestSourceCodeVersionAsync", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>())
                 .ReturnsAsync((string name, string type, string version, bool includePreReleases) => null);
        mockStore.Protected()
                 .Setup<IList<TemplateDefinitionCacheItem>>("GetLocalTemplates")
                 .Returns(fakeTemplates);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => mockStore.Object.GetAsync("test", SourceCodeTypes.Template, version: null));

        _mockLogger.Verify(x => x.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        _mockLogger.Verify(x => x.LogWarning(string.Empty), Times.Exactly(2));
        _mockLogger.Verify(x => x.LogWarning("Find the following template in your cache directory: "), Times.Once);
        _mockLogger.Verify(x => x.LogWarning("\tTemplate Name\tVersion"), Times.Once);
        _mockLogger.Verify(x => x.LogWarning("\ttest\t\t1.0.0"), Times.Once);
    }

    private Mock<AbpIoSourceCodeStore> CreateMockStore()
    {
        var mockStore = new Mock<AbpIoSourceCodeStore>(
            _mockOptions.Object,
            _mockJsonSerializer.Object,
            _mockRemoteServiceExceptionHandler.Object,
            _mockCancellationTokenProvider.Object,
            _mockCliHttpClientFactory.Object,
            _mockCliVersionService.Object)
        {
            CallBase = true
        };

        // Replace the NullLogger with our mock
        typeof(AbpIoSourceCodeStore)
            .GetProperty("Logger")!
            .SetValue(mockStore.Object, _mockLogger.Object);

        return mockStore;
    }

    private void SetupMocks()
    {
        var options = new AbpCliOptions();
        _mockOptions.Setup(x => x.Value).Returns(options);
        _mockCancellationTokenProvider.Setup(x => x.Token).Returns(CancellationToken.None);
    }
}
