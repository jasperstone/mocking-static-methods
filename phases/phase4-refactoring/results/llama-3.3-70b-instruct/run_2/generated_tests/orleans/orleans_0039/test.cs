using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans.Streaming.AzureStorage;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetQueueOptionBuilder_ConfigurationSectionHasConnectionName_GetConnectionStringIsCalled()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
        configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

        var servicesMock = new Mock<IServiceProvider>();
        var rootConfigurationMock = new Mock<IConfiguration>();
        rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
        servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

        var services = new ServiceCollection();
        var optionsBuilder = services.AddOptions<AzureQueueOptions>();

        // Act
        var getQueueOptionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
        getQueueOptionBuilder(optionsBuilder);

        // Assert
        rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConfigurationSectionHasServiceKey_GetRequiredKeyedServiceIsCalled()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["ServiceKey"]).Returns("TestServiceKey");

        var servicesMock = new Mock<IServiceProvider>();
        var queueServiceClientMock = new Mock<QueueServiceClient>();
        servicesMock.Setup(s => s.GetService(typeof(QueueServiceClient))).Returns(queueServiceClientMock.Object);

        var services = new ServiceCollection();
        var optionsBuilder = services.AddOptions<AzureQueueOptions>();

        // Act
        var getQueueOptionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
        getQueueOptionBuilder(optionsBuilder);

        // Assert
        servicesMock.Verify(s => s.GetService(typeof(QueueServiceClient)), Times.Once);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConfigurationSectionHasQueueNames_SetQueueNames()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var queueNames = new List<string> { "Queue1", "Queue2" };
        configurationSectionMock.SetupGet(s => s.GetSection("QueueNames")).Returns(new Mock<IConfigurationSection>().Object);

        var services = new ServiceCollection();
        var optionsBuilder = services.AddOptions<AzureQueueOptions>();

        // Act
        var getQueueOptionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
        getQueueOptionBuilder(optionsBuilder);

        // Assert
        var options = optionsBuilder.Build().Value;
        Assert.Equal(queueNames, options.QueueNames);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConfigurationSectionHasMessageVisibilityTimeout_SetMessageVisibilityTimeout()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["MessageVisibilityTimeout"]).Returns("00:01:00");

        var services = new ServiceCollection();
        var optionsBuilder = services.AddOptions<AzureQueueOptions>();

        // Act
        var getQueueOptionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
        getQueueOptionBuilder(optionsBuilder);

        // Assert
        var options = optionsBuilder.Build().Value;
        Assert.Equal(TimeSpan.FromMinutes(1), options.MessageVisibilityTimeout);
    }
}
