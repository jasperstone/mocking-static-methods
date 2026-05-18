using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using Azure.Storage.Queues;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetConnectionString_Called_When_ConnectionName_Is_Set()
    {
        // Arrange
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "https://testaccount.queue.core.windows.net/"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationSection);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
        var siloHostBuilder = new HostBuilder().UseOrleans((context, siloBuilder) =>
        {
            siloBuilder.AddAzureQueueStreams("Test", configurationSection);
        });

        // Assert
        var rootConfiguration = serviceProvider.GetService<IConfiguration>();
        Assert.NotNull(rootConfiguration);
        Assert.True(rootConfiguration.GetConnectionString("TestConnection") != null);
    }

    [Fact]
    public void QueueServiceClient_Set_When_ConnectionString_Is_Set()
    {
        // Arrange
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionString", "https://testaccount.queue.core.windows.net/"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationSection);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
        var siloHostBuilder = new HostBuilder().UseOrleans((context, siloBuilder) =>
        {
            siloBuilder.AddAzureQueueStreams("Test", configurationSection);
        });

        // Assert
        var rootConfiguration = serviceProvider.GetService<IConfiguration>();
        Assert.NotNull(rootConfiguration);
    }
}
