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

    public NpmPackageInfoProviderTests()
    {
        _mockJsonSerializer = new Mock<IJsonSerializer>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        _mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPackage_WhenPackageExists()
    {
        // Arrange
        var packageList = new List<dynamic> { new { Name = "test-package" } };
        var json = "[{\"Name\":\"test-package\"}]";
        
        SetupMocks(packageList, json);

        var provider = CreateSut();

        // Act
        var result = await provider.GetAsync("test-package");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenPackageNotFound()
    {
        // Arrange
        var packageList = new List<dynamic>();
        var json = "[]";
        
        SetupMocks(packageList, json);

        var provider = CreateSut();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("non-existent-package"));
        Assert.Equal("Package is not found or downloadable!", exception.Message);
    }

    [Fact]
    public async Task GetPackageListAsync_ShouldReturnPackageList_WhenHttpCallSucceeds()
    {
        // Arrange
        var expectedList = new List<dynamic>
        {
            new { Name = "package1" },
            new { Name = "package2" }
        };
        var json = "[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]";
        
        SetupMocks(expectedList, json);

        var provider = CreateSut();

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    private NpmPackageInfoProvider CreateSut()
    {
        _mockCliHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(_mockHttpClient);
        return new NpmPackageInfoProvider(
            _mockJsonSerializer.Object,
            _mockCancellationTokenProvider.Object,
            _mockRemoteServiceExceptionHandler.Object,
            _mockCliHttpClientFactory.Object);
    }

    private void SetupMocks(List<dynamic> packageList, string json)
    {
        _mockCancellationTokenProvider.Setup(x => x.Token).Returns(CancellationToken.None);
        _mockRemoteServiceExceptionHandler.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<dynamic>>(json)).Returns(packageList);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });
    }
}
