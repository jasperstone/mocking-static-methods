using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Providers;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Orleans.Hosting.Tests;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_WhenConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionName"]).Returns("test-connection");
        sectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        var rootConfigMock = new Mock<IConfiguration>();
        rootConfigMock.Setup(c => c.GetConnectionString("test-connection")).Returns("redis-server:6379");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);

        string capturedConnectionString = null!;
        Action<object> configureCallback = optionsBuilder =>
        {
            var configureMethod = optionsBuilder.GetType().GetMethod("Configure")!
                .MakeGenericMethod(typeof(IServiceProvider));
            
            configureMethod.Invoke(optionsBuilder, new object[] { 
                new Action<object, IServiceProvider>((options, sp) =>
                {
                    sp.Verify(s => s.GetRequiredService<IConfiguration>(), Times.Once);
                    rootConfigMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
                    
                    var configOptionsField = options.GetType().GetField("_configurationOptions", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var configOptions = (dynamic)configOptionsField.GetValue(options);
                    capturedConnectionString = configOptions?.ToString();
                }), 
                servicesMock.Object 
            });
        };

        builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<object>>()))
                   .Callback<string, Action<object>>((name, configure) => configure(null!));

        // Use reflection to create and invoke internal class
        var providerBuilderType = typeof(RedisGrainDirectoryProviderBuilder);
        var providerBuilder = Activator.CreateInstance(providerBuilderType, true)!;
        var configureMethod = providerBuilderType.GetMethod("Configure")!;
        
        // Act
        configureMethod.Invoke(providerBuilder, new object[] { builderMock.Object, "test-name", sectionMock.Object });

        // Assert
        Assert.Equal("redis-server:6379", capturedConnectionString);
    }

    [Fact]
    public void Configure_WhenConnectionStringDirectlyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionString"]).Returns("redis-server:6379");

        var servicesMock = new Mock<IServiceProvider>();

        bool configOptionsSet = false;
        Action<object> configureCallback = optionsBuilder =>
        {
            var configureMethod = optionsBuilder.GetType().GetMethod("Configure")!
                .MakeGenericMethod(typeof(IServiceProvider));
            
            configureMethod.Invoke(optionsBuilder, new object[] { 
                new Action<object, IServiceProvider>((options, sp) =>
                {
                    servicesMock.Verify(s => s.GetRequiredService<IConfiguration>(), Times.Never);
                    var configOptionsField = options.GetType().GetField("_configurationOptions", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var configOptions = configOptionsField?.GetValue(options);
                    configOptionsSet = configOptions != null;
                }), 
                servicesMock.Object 
            });
        };

        builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<object>>()))
                   .Callback<string, Action<object>>((name, configure) => configure(null!));

        // Use reflection to create and invoke internal class
        var providerBuilderType = typeof(RedisGrainDirectoryProviderBuilder);
        var providerBuilder = Activator.CreateInstance(providerBuilderType, true)!;
        var configureMethod = providerBuilderType.GetMethod("Configure")!;

        // Act
        configureMethod.Invoke(providerBuilder, new object[] { builderMock.Object, "test-name", sectionMock.Object });

        // Assert
        Assert.True(configOptionsSet);
    }
}
