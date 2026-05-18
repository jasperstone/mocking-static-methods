using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.DynamoDB; // Ensure this namespace is included

// Mock version of DynamoDBStorageOptions
public class DynamoDBStorageOptions
{
    public string ServiceId { get; set; }
    public string TableName { get; set; }
    public int ReadCapacityUnits { get; set; }
    public int WriteCapacityUnits { get; set; }
    public bool UseProvisionedThroughput { get; set; }
    public bool CreateIfNotExists { get; set; }
    public bool UpdateIfExists { get; set; }
    public bool DeleteStateOnClear { get; set; }
}

// Mock version of DynamoDBGrainStorageFactory
public static class DynamoDBGrainStorageFactory
{
    public static object Create(IServiceProvider services, string name)
    {
        var optionsMonitor = services.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
        return new object(); // Return a dummy object for testing purposes
    }
}

public class DynamoDBGrainStorageFactoryTests
{
    [Fact]
    public void Create_ShouldCallGetRequiredServiceWithCorrectType()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
        var options = new DynamoDBStorageOptions
        {
            ServiceId = "default",
            TableName = "default",
            ReadCapacityUnits = 5,
            WriteCapacityUnits = 5,
            UseProvisionedThroughput = true,
            CreateIfNotExists = true,
            UpdateIfExists = false,
            DeleteStateOnClear = false
        };
        mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

        // Use a delegate to capture the call
        Func<IOptionsMonitor<DynamoDBStorageOptions>> getOptionsMonitor = null;
        mockServiceProvider
            .Setup(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
            .Returns(() =>
            {
                getOptionsMonitor = () => mockOptionsMonitor.Object;
                return mockOptionsMonitor.Object;
            });

        // Act
        var result = DynamoDBGrainStorageFactory.Create(mockServiceProvider.Object, "testName");

        // Assert
        Assert.NotNull(getOptionsMonitor);
        getOptionsMonitor.Invoke();
        mockServiceProvider.Verify(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
        Assert.NotNull(result);
    }
}
