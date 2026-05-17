using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class NpmPackageInfoProviderTests
{
    [Fact]
    public async Task GetAsync_PackageFound_ReturnsPackage()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var packages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "test-package" }
        };
        var json = "[{\"name\":\"test-package\"}]";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

        mockJsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(json)).Returns(packages);
        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act
        var result = await provider.GetAsync("test-package");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-package", result.Name);
    }

    [Fact]
    public async Task GetAsync_PackageNotFound_ThrowsException()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var packages = new List<NpmPackageInfo>();
        var json = "[]";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

        mockJsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(json)).Returns(packages);
        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("non-existent-package"));
    }

    [Fact]
    public async Task GetPackageListAsync_SuccessfulHttpCall_ReturnsDeserializedList()
    {
        // Arrange
        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();

        var expectedPackages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "package1" },
            new NpmPackageInfo { Name = "package2" }
        };
        var json = "[{\"name\":\"package1\"},{\"name\":\"package2\"}]";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

        mockJsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(json)).Returns(expectedPackages);
        mockCancellationTokenProvider.Setup(p => p.Token).Returns(CancellationToken.None);
        mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()));

        var provider = new NpmPackageInfoProvider(
            mockJsonSerializer.Object,
            mockCancellationTokenProvider.Object,
            mockRemoteServiceExceptionHandler.Object,
            mockHttpClientFactory.Object);

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("package1", result[0].Name);
    }
}
