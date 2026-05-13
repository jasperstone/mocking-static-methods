using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockResponse = new Mock<HttpResponseMessage>();
            var mockContent = new Mock<HttpContent>();
            var mockSerializer = new Mock<IJsonSerializer>();
            var mockRemoteHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockVersionService = new Mock<CliVersionService>();
            var store = new AbpIoSourceCodeStore(
                Options.Create(new AbpCliOptions()),
                mockSerializer.Object,
                mockRemoteHandler.Object,
                mockTokenProvider.Object,
                mockHttpClientFactory.Object,
                mockVersionService.Object);

            var testUrl = "https://testurl.com/api/download/all-versions?includePreReleases=true";

            var mockHttpClientHandler = new Mock<HttpMessageHandler>();
            mockHttpClientHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"FrameworkAndCommercialVersions\":[],\"LeptonXVersions\":[]}")
                });

            var client = new HttpClient(mockHttpClientHandler.Object);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<TimeSpan>())).Returns(client);

            // Act
            var result = await store.InvokePrivateMethodAsync<bool>("IsVersionExists", "TestTemplate", "1.0.0");

            // Assert
            mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<TimeSpan>()), Times.Once);
            Assert.True(result);
        }
    }

    // Extension method to invoke private methods for testing
    public static class TestExtensions
    {
        public static async Task<T> InvokePrivateMethodAsync<T>(this object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(obj, args);
            if (result is Task<T> task)
            {
                return await task;
            }
            else if (result is Task taskResult)
            {
                await taskResult;
                return default(T);
            }
            else
            {
                return (T)result;
            }
        }
    }
}
