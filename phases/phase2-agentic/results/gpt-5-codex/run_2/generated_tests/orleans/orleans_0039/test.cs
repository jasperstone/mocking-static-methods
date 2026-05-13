using System;
using System.Collections.Generic;
using System.Reflection;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void Configure_UsesConnectionStringFromRootConfigurationWhenConnectionNameProvided()
    {
        // Arrange
        var connectionString = "https://storageaccount.queue.core.windows.net?sig=dummysig";
        var rootConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MyConnection"] = connectionString,
                ["AzureQueue:ConnectionName"] = "MyConnection"
            })
            .Build();
        var configurationSection = rootConfiguration.GetSection("AzureQueue");

        var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var queueOptionsAction = (Action<OptionsBuilder<AzureQueueOptions>>)method!.Invoke(null, new object[] { configurationSection })!;

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration);
        services.AddOptions();

        var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(services, Options.DefaultName);

        // Act
        queueOptionsAction(optionsBuilder);
        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<AzureQueueOptions>>().Get(Options.DefaultName);

        // Assert
        var expectedUri = new Uri(connectionString);
        Assert.NotNull(options.QueueServiceClient);
        Assert.Equal(expectedUri, options.QueueServiceClient.Uri);
    }
}
