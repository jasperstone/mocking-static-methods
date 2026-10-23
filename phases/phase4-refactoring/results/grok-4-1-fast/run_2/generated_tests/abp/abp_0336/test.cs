using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;

    public AbpIoSourceCodeStoreTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object);

        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>()))
            .Returns(_httpClientMock.Object);
        _cliHttpClientFactoryMock
            .Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(new CancellationToken());

        services.AddSingleton(Mock.Of<IOptions<AbpCliOptions>>());
        services.AddSingleton(_jsonSerializerMock.Object);
        services.AddSingleton(_remoteServiceExceptionHandlerMock.Object);
        services.AddSingleton(_cliHttpClientFactoryMock.Object);

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task GetAsync_Should_Call_IsVersionExists_And_Use_HttpClient_GetAsync()
    {
        // Arrange
        var templateName = "app";
        var version = "1.0.0";
        var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
        var responseContent = "{\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}";

        SetupHttpClientSuccess(url, responseContent);

        _jsonSerializerMock
            .Setup(x => x.Deserialize<GithubReleaseVersions>(responseContent))
            .Returns(new GithubReleaseVersions
            {
                FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = version } }
            });

        _remoteServiceExceptionHandlerMock
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var sourceCodeStore = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();

        // Act
        await sourceCodeStore.GetAsync(templateName, "template", version, trustUserVersion: true);

        // Assert - Verify HttpClient.GetAsync was called (line 295)
        _httpMessageHandlerMock.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("all-versions")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_Handle_IsVersionExists_Exception()
    {
        // Arrange - Setup HttpClient to throw, which will cause IsVersionExists to return true
        var templateName = "app";
        var version = "1.0.0";
        var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";

        SetupHttpClientThrows(url);

        _remoteServiceExceptionHandlerMock
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var sourceCodeStore = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();

        // Act & Assert - Should not throw since IsVersionExists returns true on exception
        await sourceCodeStore.GetAsync(templateName, "template", version, trustUserVersion: true);
    }

    private void SetupHttpClientSuccess(string requestUri, string responseContent)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(requestUri)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            });
    }

    private void SetupHttpClientThrows(string requestUri)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(requestUri)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test exception"));
    }
}

// DTOs needed for deserialization
public class GithubReleaseVersions
{
    public GithubReleaseVersion[] LeptonXVersions { get; set; } = Array.Empty<GithubReleaseVersion>();
    public GithubReleaseVersion[] FrameworkAndCommercialVersions { get; set; } = Array.Empty<GithubReleaseVersion>();
}

public class GithubReleaseVersion
{
    public string Name { get; set; } = string.Empty;
}
