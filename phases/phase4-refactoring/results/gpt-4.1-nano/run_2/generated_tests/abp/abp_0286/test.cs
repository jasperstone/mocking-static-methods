using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task GenerateCrudPageAsync_Should_LogError_When_ResponseContainsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var handler = new class : HttpMessageHandler
            {
                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
                {
                    var response = new HttpResponseMessage
                    {
                        Content = new StringContent("Error: Something went wrong")
                    };
                    return Task.FromResult(response);
                }
            };

            var httpClient = new HttpClient(handler);

            var cliHttpClientFactoryMock = new Mock<IVirtualHttpClientFactory>();
            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<bool>()))
                .Returns(httpClient);

            // Instantiate SuiteCommand with minimal dependencies
            var suiteCommand = new SuiteCommand(
                new Mock<AbpNuGetIndexUrlService>().Object,
                new Mock<PackageVersionCheckerService>().Object,
                new Mock<ICmdHelper>().Object,
                new Mock<AuthService>().Object,
                cliHttpClientFactoryMock.Object,
                new Mock<SuiteAppSettingsService>().Object
            )
            {
                Logger = loggerMock.Object
            };

            // Create dummy file
            var tempEntityFile = Path.GetTempFileName();
            File.WriteAllText(tempEntityFile, "{}");
            var args = new CommandLineArgs
            {
                Options = new System.Collections.Generic.Dictionary<string, string>
                {
                    {Options.Crud.Entity.Short, tempEntityFile},
                    {Options.Crud.Solution.Short, "dummy.sln"}
                }
            };

            // Use reflection to invoke private method
            var methodInfo = typeof(SuiteCommand).GetMethod("GenerateCrudPageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)methodInfo.Invoke(suiteCommand, new object[] { args });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error: Something went wrong")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
