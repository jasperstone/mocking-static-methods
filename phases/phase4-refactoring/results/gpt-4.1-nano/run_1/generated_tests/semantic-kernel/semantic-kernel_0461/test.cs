using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Moq;
using System;
using Microsoft.Extensions.VectorData; // For VectorStore
using Microsoft.SemanticKernel.Connectors.MongoDB; // For MongoVectorStore
using Microsoft.SemanticKernel; // For IEmbeddingGenerator and options

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddMongoVectorStore_Should_Register_Services_And_Resolve_Correctly()
        {
            var services = new ServiceCollection();

            // Mock IMongoDatabase
            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton(mockDatabase.Object);

            // Build provider
            var provider = services.BuildServiceProvider();

            // Register the database in the provider for testing
            services.AddSingleton<IMongoDatabase>(mockDatabase.Object);
            provider = services.BuildServiceProvider();

            // Call extension method
            services.AddMongoVectorStore();

            var sp = services.BuildServiceProvider();

            // Resolve VectorStore
            var vectorStore = sp.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
            Assert.IsType<MongoVectorStore>(vectorStore);

            // Resolve MongoVectorStore
            var mongoStore = sp.GetService<MongoVectorStore>();
            Assert.NotNull(mongoStore);
            Assert.Equal(mockDatabase.Object, mongoStore.Database);
        }

        [Fact]
        public void GetStoreOptions_Should_Return_Options_With_EmbeddingGenerator_When_Present()
        {
            var services = new ServiceCollection();

            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object };

            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Call private method via reflection or make it internal for testing
            var result = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { provider, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => null) });

            Assert.NotNull(result);
            var optionsResult = result as MongoVectorStoreOptions;
            Assert.NotNull(optionsResult);
            Assert.Equal(mockGenerator.Object, optionsResult.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_Should_Return_NewOptions_When_EmbeddingGenerator_Is_Null()
        {
            var services = new ServiceCollection();

            var options = new MongoVectorStoreOptions { EmbeddingGenerator = null };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            var result = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { provider, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            Assert.NotNull(result);
            var optionsResult = result as MongoVectorStoreOptions;
            Assert.NotNull(optionsResult);
            Assert.Null(optionsResult.EmbeddingGenerator);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_Should_Register_Services_Correctly()
        {
            var services = new ServiceCollection();

            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton(mockDatabase.Object);

            services.AddKeyedMongoVectorStore("key");

            var provider = services.BuildServiceProvider();

            var vectorStore = provider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
            Assert.IsType<MongoVectorStore>(vectorStore);
        }
    }
}
