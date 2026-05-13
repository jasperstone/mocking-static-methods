using System;
using System.Collections.Generic;
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
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly Mock<CliHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly CSharpServiceProxyGenerator _generator;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _httpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _generator = new CSharpServiceProxyGenerator(_httpClientFactoryMock.Object, _jsonSerializerMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingFile()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                WorkDirectory = "testDir",
                Folder = "subFolder",
                CommandName = "generate",
                WithoutContracts = false
            };

            var apiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationModuleApiDescription>
                {
                    {
                        "module1", new ApplicationModuleApiDescription
                        {
                            Controllers = new Dictionary<string, ApplicationControllerApiDescription>
                            {
                                {
                                    "controller1", new ApplicationControllerApiDescription
                                    {
                                        Interfaces = new List<ApplicationInterfaceApiDescription>
                                        {
                                            new ApplicationInterfaceApiDescription
                                            {
                                                Type = "Volo.Abp.Application.Services.IApplicationService",
                                                Methods = new List<ApplicationMethodApiDescription>()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Mock the method to return our apiDescriptionModel
            var getApiDescriptionModelMethod = typeof(CSharpServiceProxyGenerator).GetMethod("GetApplicationApiDescriptionModelAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var taskCompletionSource = new TaskCompletionSource<ApplicationApiDescriptionModel>();
            taskCompletionSource.SetResult(apiDescriptionModel);
            var mockGenerator = new Mock<CSharpServiceProxyGenerator>(_httpClientFactoryMock.Object, _jsonSerializerMock.Object);
            mockGenerator.CallBase = true;
            mockGenerator.Setup(x => x.GetApplicationApiDescriptionModelAsync(It.IsAny<GenerateProxyArgs>(), It.IsAny<ApplicationApiDescriptionModelRequestDto>()))
                         .Returns(taskCompletionSource.Task);
            var generator = mockGenerator.Object;

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
