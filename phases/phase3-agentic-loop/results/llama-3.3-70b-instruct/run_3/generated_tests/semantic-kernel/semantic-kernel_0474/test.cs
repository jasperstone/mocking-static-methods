using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedQdrantCollection_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceKey = "test";
            var name = "test";
            var host = "test";
            var port = 6334;
            var https = true;
            var apiKey = "test";
            var options = new QdrantCollectionOptions();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => QdrantServiceCollectionExtensions.AddKeyedQdrantCollection<string, object>(null, serviceKey, name, host, port, https, apiKey, options));
        }

        [Fact]
        public void AddKeyedQdrantCollection_ServiceKeyIsNull_ServiceProviderIsNotNull_AddsQdrantCollectionToServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test";
            var host = "test";
            var port = 6334;
            var https = true;
            var apiKey = "test";
            var options = new QdrantCollectionOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.AddKeyedQdrantCollection<string, object>(services, null, name, host, port, https, apiKey, options);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 1);
        }
    }
}
