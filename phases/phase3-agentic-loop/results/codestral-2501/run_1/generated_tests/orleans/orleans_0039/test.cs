using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetQueueOptionBuilder_ShouldSetConnectionStringFromConfiguration()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var mockRootConfiguration = new Mock<IConfiguration>();
        mockRootConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockRootConfiguration.Object);

        var services = new ServiceCollection();
        services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(services, "Test");

        // Act
        var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(mockConfigurationSection.Object);
        optionBuilder(optionsBuilder);

        // Assert
        var options = optionsBuilder.Value;
        Assert.NotNull(options.QueueServiceClient);
        Assert.Equal("TestConnectionString", options.QueueServiceClient.AccountName);
    }

    public class AzureQueueOptions
    {
        public QueueServiceClient QueueServiceClient { get; set; }
    }
}
