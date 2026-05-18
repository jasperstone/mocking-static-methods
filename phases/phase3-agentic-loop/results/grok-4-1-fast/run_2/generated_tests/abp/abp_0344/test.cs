using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Threading;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class NpmPackageInfoProviderTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnPackage_WhenPackageExists()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var expectedPackageList = new List<object>
        {
            new { Name = "test-package" }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(expectedPackageList);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(httpClient);

        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        mockJsonSerializer.Setup(s => s.Deserialize<List<object>>(It.IsAny<string>()))
            .Returns(expectedPackageList);

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act
        var result = await provider.GetAsync("test-package");

        // Assert
        Assert.NotNull(result);
        mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenPackageNotFound()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var emptyPackageList = new List<object>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(httpClient);

        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        mockJsonSerializer.Setup(s => s.Deserialize<List<object>>(It.IsAny<string>()))
            .Returns(emptyPackageList);

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("non-existent-package"));
    }

    [Fact]
    public async Task GetPackageListAsync_ShouldHandleSuccessfulHttpResponse()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var expectedPackageList = new List<object>
        {
            new { Name = "package1" },
            new { Name = "package2" }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(expectedPackageList);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(httpClient);

        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        mockJsonSerializer.Setup(s => s.Deserialize<List<object>>(It.IsAny<string>()))
            .Returns(expectedPackageList);

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.NotNull(result);
        mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()), Times.Once);
        mockRemoteServiceExceptionHandler.Verify(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
        mockJsonSerializer.Verify(s => s.Deserialize<List<object>>(It.IsAny<string>()), Times.Once);
    }
}
