using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_Should_LogInformation_When_CreatingFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var generator = new CSharpServiceProxyGenerator(mockHttpClientFactory.Object, mockJsonSerializer.Object);
            generator.Logger = mockLogger.Object;

            var args = new GenerateProxyArgs
            {
                WorkDirectory = Path.GetTempPath(),
                Folder = "TestFolder",
                CommandName = "Generate",
                WithoutContracts = false
            };

            // Setup a minimal ApplicationApiDescriptionModel with one controller and interface
            var apiDescription = new ApplicationApiDescriptionModel
            {
                Modules = new()
                {
                    ["TestModule"] = new ModuleApiDescription
                    {
                        Controllers = new()
                        {
                            ["TestController"] = new ControllerApiDescription
                            {
                                Interfaces = new[]
                                {
                                    new InterfaceApiDescription
                                    {
                                        Type = "Volo.Abp.Application.Services.IApplicationService",
                                        Methods = Array.Empty<MethodApiDescription>()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Mock GetApplicationApiDescriptionModelAsync to return our minimal model
            async Task<ApplicationApiDescriptionModel> MockGetApplicationApiDescriptionModelAsync(GenerateProxyArgs a, object b)
            {
                return await Task.FromResult(apiDescription);
            }

            // Use reflection to set the private method (or alternatively, we can make it internal for testing)
            // For simplicity, assume we can override or inject dependencies; here, we will just call the method directly
            // and simulate the part that calls Logger.LogInformation

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
