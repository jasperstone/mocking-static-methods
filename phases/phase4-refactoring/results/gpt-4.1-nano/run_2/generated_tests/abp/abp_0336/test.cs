using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_Should_Call_GetAsync_And_Return_Correct_Result()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"leptonXVersions\":[],\"frameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}") // sample JSON
                    };
                    return await Task.FromResult(response);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            mockHttpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<TimeSpan>()))
                .Returns(httpClient);

            var store = new AbpIoSourceCodeStore(
                Options.Create(new AbpCliOptions()), 
                new DummyJsonSerializer(), 
                new DummyRemoteServiceExceptionHandler(), 
                new DummyCancellationTokenProvider(), 
                mockHttpClientFactory.Object,
                new DummyCliVersionService());

            // Act
            var result = await store.InvokePrivateMethod<bool>("IsVersionExists", "templateName", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }

    // Dummy implementations for dependencies
    public class DummyJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json) => default;
        public string Serialize(object obj) => "";
    }

    public class DummyRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
    {
        public Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage response) => Task.CompletedTask;
        public Task GetAbpRemoteServiceErrorAsync(HttpResponseMessage response) => Task.CompletedTask;
    }

    public class DummyCancellationTokenProvider : ICancellationTokenProvider
    {
        public CancellationToken GetCancellationToken(TimeSpan timeout) => CancellationToken.None;
    }

    public class DummyCliVersionService : CliVersionService
    {
        public override Task<SemanticVersion> GetCurrentCliVersionAsync() => Task.FromResult(SemanticVersion.Parse("1.0.0"));
    }
}
