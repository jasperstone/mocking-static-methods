using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Microsoft.Plugins.Manifest;

namespace Functions.OpenApi.Extensions.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWhenNoFunctionsFoundInRuntimeObject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var runtime = new Runtime { Type = RuntimeType.OpenApi, RunForFunctions = new List<string>() };
            var document = new PluginManifestDocument { Functions = new List<PluginFunction>() };

            // Act
            var functions = new List<KernelFunction>();
            var documentWalker = new OpenApiWalker(new OperationIdNormalizationOpenApiVisitor());
            foreach (var r in new[] { runtime })
            {
                var manifestFunctions = document?.Functions?.Where(f => r.RunForFunctions.Contains(f.Name)).ToList();
                if (manifestFunctions is null || manifestFunctions.Count == 0)
                {
                    loggerMock.Object.LogWarning("No functions found in the runtime object.");
                    continue;
                }
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning("No functions found in the runtime object."), Times.Once);
        }

        [Fact]
        public void LogWarning_CalledWhenNoApiDescriptionUrlFoundInRuntimeObject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var runtime = new Runtime { Type = RuntimeType.OpenApi, Spec = new OpenApiSpec { Url = string.Empty } };
            var document = new PluginManifestDocument { Functions = new List<PluginFunction>() };

            // Act
            var functions = new List<KernelFunction>();
            var documentWalker = new OpenApiWalker(new OperationIdNormalizationOpenApiVisitor());
            foreach (var r in new[] { runtime })
            {
                var manifestFunctions = document?.Functions?.Where(f => r.RunForFunctions.Contains(f.Name)).ToList();
                if (manifestFunctions is null || manifestFunctions.Count == 0)
                {
                    loggerMock.Object.LogWarning("No functions found in the runtime object.");
                    continue;
                }

                var openApiRuntime = r as Runtime;
                var apiDescriptionUrl = openApiRuntime?.Spec?.Url ?? string.Empty;
                if (apiDescriptionUrl.Length == 0)
                {
                    loggerMock.Object.LogWarning("No API description URL found in the runtime object.");
                    continue;
                }
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }
}
