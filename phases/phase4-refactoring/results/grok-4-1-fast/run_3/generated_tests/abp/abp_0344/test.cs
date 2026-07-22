using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class NpmPackageInfoProviderTests
{
    private readonly Mock<IJsonSerializer> _mockJsonSerializer;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<IRemoteServiceExceptionHandler> _mockRemoteServiceExceptionHandler;
    private readonly Mock<CliHttpClientFactory> _mockCliHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _mockHttpClient;
    private readonly NpmPackageInfoProvider _provider;

    public NpmPackageInfoProviderTests()
    {
        _mockJsonSerializer = new Mock<IJsonSerializer>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        _mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();

        _mockCancellationTokenProvider.Setup(x => x.Token).Returns(CancellationToken.None);

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockCliHttpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(_mockHttpClient);

        _provider = new NpmPackageInfoProvider(
            _mockJsonSerializer.Object,
            _mockCancellationTokenProvider.Object,
            _mockRemoteServiceExceptionHandler.Object,
            _mockCliHttpClientFactory.Object);
    }

    [Fact]
    public async Task GetPackageListAsync_ShouldReturnDeserializedList()
    {
        // Arrange
        var packageList = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "abp.core", Version = "1.0.0" }
        };
        var jsonContent = "[{\"name\":\"abp.core\",\"version\":\"1.0.0\"}]";

        SetupHttpResponse(jsonContent, HttpStatusCode.OK);

        _mockJsonSerializer
            .Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent))
            .Returns(packageList);

        _mockRemoteServiceExceptionHandler
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _provider.GetPackageListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("abp.core", result[0].Name);
        _mockJsonSerializer.Verify(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent), Times.Once);
        _mockRemoteServiceExceptionHandler.Verify(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPackage_WhenFound()
    {
        // Arrange
        var packageList = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "abp.core", Version = "1.0.0" }
        };
        var jsonContent = "[{\"name\":\"abp.core\",\"version\":\"1.0.0\"}]";

        SetupHttpResponse(jsonContent, HttpStatusCode.OK);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(packageList);
        _mockRemoteServiceExceptionHandler
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _provider.GetAsync("abp.core");

        // Assert
        Assert.Equal("abp.core", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenPackageNotFound()
    {
        // Arrange
        var packageList = new List<NpmPackageInfo>();
        var jsonContent = "[]";

        SetupHttpResponse(jsonContent, HttpStatusCode.OK);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(packageList);
        _mockRemoteServiceExceptionHandler
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _provider.GetAsync("nonexistent"));
        Assert.Equal("Package is not found or downloadable!", exception.Message);
    }

    private void SetupHttpResponse(string content, HttpStatusCode statusCode)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
    }
}

public class NpmPackageInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
