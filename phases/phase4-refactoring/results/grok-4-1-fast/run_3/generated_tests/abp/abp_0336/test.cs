using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<IOptions<AbpCliOptions>>());
        services.AddSingleton(_jsonSerializerMock.Object);
        services.AddSingleton(_remoteServiceExceptionHandlerMock.Object);
        services.AddSingleton(_cliHttpClientFactoryMock.Object);

        _serviceProvider = services.BuildServiceProvider();
        _sourceCodeStore = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_WhenLeptonXVersionFound()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}")
        };
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(httpClient);
        _cliHttpClientFactoryMock
            .Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(cancellationToken);
        _remoteServiceExceptionHandlerMock
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);
        _jsonSerializerMock
            .Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(new GithubReleaseVersions 
            { 
                LeptonXVersions = new List<GithubReleaseVersion> { new GithubReleaseVersion { Name = "1.0.0" } }
            });

        // Act & Assert
        var result = await _sourceCodeStore.IsVersionExists("leptonx-template", "1.0.0");
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_WhenFrameworkVersionFound()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}")
        };
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(httpClient);
        _cliHttpClientFactoryMock
            .Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(cancellationToken);
        _remoteServiceExceptionHandlerMock
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);
        _jsonSerializerMock
            .Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(new GithubReleaseVersions 
            { 
                FrameworkAndCommercialVersions = new List<GithubReleaseVersion> { new GithubReleaseVersion { Name = "1.0.0" } }
            });

        // Act & Assert
        var result = await _sourceCodeStore.IsVersionExists("basic-template", "1.0.0");
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_OnException()
    {
        // Arrange
        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        var result = await _sourceCodeStore.IsVersionExists("any-template", "1.0.0");
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnFalse_WhenVersionNotFound()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(httpClient);
        _cliHttpClientFactoryMock
            .Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(cancellationToken);
        _remoteServiceExceptionHandlerMock
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);
        _jsonSerializerMock
            .Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(new GithubReleaseVersions());

        // Act & Assert
        var result = await _sourceCodeStore.IsVersionExists("basic-template", "1.0.0");
        Assert.False(result);
    }
}
