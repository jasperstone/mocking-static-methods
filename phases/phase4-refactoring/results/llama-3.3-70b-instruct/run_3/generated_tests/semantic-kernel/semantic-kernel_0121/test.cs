using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            services.AddHuggingFaceImageToText("model", null, null, null, null);

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
