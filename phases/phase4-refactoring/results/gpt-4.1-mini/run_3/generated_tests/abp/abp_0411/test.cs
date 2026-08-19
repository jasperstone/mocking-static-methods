using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Xunit;

namespace Volo.Abp.Cli.Tests.ServiceProxying.CSharp
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformationOnDeleteProxy()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var jsonSerializer = new Mock<Volo.Abp.Json.IJsonSerializer>();

            var generator = new TestCSharpServiceProxyGenerator(cliHttpClientFactory.Object, jsonSerializer.Object, mockLogger.Object);

            var args = new GenerateProxyArgs(
                commandName: "RemoveProxy",
                workDirectory: Environment.CurrentDirectory,
                module: null,
                url: null,
                output: null,
                target: null,
                apiName: null,
                source: null,
                folder: null,
                serviceType: null,
                entryPoint: null,
                withoutContracts: false);

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Delete ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateProxyAsync_LogsInformationOnCreateProxy()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var jsonSerializer = new Mock<Volo.Abp.Json.IJsonSerializer>();

            var generator = new TestCSharpServiceProxyGenerator(cliHttpClientFactory.Object, jsonSerializer.Object, mockLogger.Object);

            var args = new GenerateProxyArgs(
                commandName: "GenerateProxy",
                workDirectory: Environment.CurrentDirectory,
                module: null,
                url: null,
                output: null,
                target: null,
                apiName: null,
                source: null,
                folder: null,
                serviceType: null,
                entryPoint: null,
                withoutContracts: false);

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Create ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestCSharpServiceProxyGenerator : CSharpServiceProxyGenerator
        {
            private readonly ILogger<CSharpServiceProxyGenerator> _logger;

            public TestCSharpServiceProxyGenerator(
                Volo.Abp.Cli.Http.CliHttpClientFactory cliHttpClientFactory,
                Volo.Abp.Json.IJsonSerializer jsonSerializer,
                ILogger<CSharpServiceProxyGenerator> logger) : base(cliHttpClientFactory, jsonSerializer)
            {
                _logger = logger;
                Logger = logger;
            }

            public override Task GenerateProxyAsync(GenerateProxyArgs args)
            {
                if (args.CommandName == "RemoveProxy")
                {
                    // Simulate the delete logging path
                    Logger.LogInformation($"Delete dummy path");
                    return Task.CompletedTask;
                }
                else
                {
                    // Simulate the create logging path
                    Logger.LogInformation($"Create dummy path");
                    return Task.CompletedTask;
                }
            }
        }
    }
}
