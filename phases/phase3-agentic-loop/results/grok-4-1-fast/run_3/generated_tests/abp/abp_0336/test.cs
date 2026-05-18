using System;
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
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ServiceProvider _serviceProvider;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJsonSerializer>(new DefaultJsonSerializer());
        services.AddSingleton<IRemoteServiceExceptionHandler>(new MockRemoteServiceExceptionHandler());
        services.AddSingleton<ICancellationTokenProvider>(new MockCancellationTokenProvider());
        services.AddSingleton(s => new CliHttpClientFactory(
            Mock.Of<IHttpClientFactory>(),
            s.GetRequiredService<ICancellationTokenProvider>()
        )
        {
            HttpClient = _httpClient
        });
        services.AddSingleton<CliVersionService>(Mock.Of<CliVersionService>());
        services.AddOptions<AbpCliOptions>()
            .Configure(o => { });
        
        services.AddTransient<AbpIoSourceCodeStore>();
        
        _serviceProvider = services.BuildServiceProvider();
        _sourceCodeStore = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();
    }

    [Fact]
    public async Task IsVersionExists_ShouldCallGetAsync_OnHttpClient()
    {
        // Arrange
        var expectedUrl = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"FrameworkAndCommercialVersions\":[],\"LeptonXVersions\":[]}")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage)
            .Verifiable();

        // Act
        var result = await _sourceCodeStore.IsVersionExists("test-template", "1.0.0");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
        
        Assert.False(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_OnException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test exception"));

        // Act & Assert
        var result = await _sourceCodeStore.IsVersionExists("test-template", "1.0.0");
        Assert.True(result);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _httpClient?.Dispose();
    }
}

public class MockRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
{
    public Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage response)
    {
        return Task.CompletedTask;
    }

    public Task<RemoteServiceErrorResponse> GetAbpRemoteServiceErrorAsync(HttpResponseMessage response)
    {
        return Task.FromResult(new RemoteServiceErrorResponse());
    }
}

public class MockCancellationTokenProvider : ICancellationTokenProvider
{
    public CancellationToken Token => default;
}
