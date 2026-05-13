using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenExceptionThrown()
        {
            // Arrange
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict, null, null);

            var sut = new AbpIoSourceCodeStore(
                Options.Create(new AbpCliOptions()),
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            var templateName = "TestTemplate";
            var version = "1.0.0";

            // Setup CreateClient to throw exception to simulate failure
            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<TimeSpan?>()))
                .Throws(new HttpRequestException());

            // Act
            var result = await sut.InvokeIsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnCorrectResult_WhenResponseIsValid()
        {
            // Arrange
            var templateName = "LeptonX";
            var version = "1.0.0";

            var versionsJson = @"{
                ""LeptonXVersions"": [{ ""Name"": ""1.0.0"" }],
                ""FrameworkAndCommercialVersions"": [{ ""Name"": ""2.0.0"" }]
            }";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(versionsJson)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<TimeSpan?>()))
                .Returns(httpClient);

            cliHttpClientFactoryMock
                .Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>()))
                .Returns(CancellationToken.None);

            var jsonSerializer = new Volo.Abp.Json.SystemTextJson.AbpSystemTextJsonSerializer();

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict, null, null);

            var sut = new AbpIoSourceCodeStore(
                Options.Create(new AbpCliOptions()),
                jsonSerializer,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            // Act
            var result = await sut.InvokeIsVersionExists(templateName, version);

            // Assert
            Assert.True(result);

            // Test with a templateName that does not contain "LeptonX"
            var nonLeptonTemplateName = "OtherTemplate";
            var result2 = await sut.InvokeIsVersionExists(nonLeptonTemplateName, "2.0.0");
            Assert.True(result2);

            var result3 = await sut.InvokeIsVersionExists(nonLeptonTemplateName, "1.0.0");
            Assert.False(result3);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
    }

    // Extension class to expose the private method for testing
    public static class AbpIoSourceCodeStoreTestExtensions
    {
        public static Task<bool> InvokeIsVersionExists(this AbpIoSourceCodeStore store, string templateName, string version)
        {
            var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("Method IsVersionExists not found");
            return (Task<bool>)method.Invoke(store, new object[] { templateName, version });
        }
    }
}
