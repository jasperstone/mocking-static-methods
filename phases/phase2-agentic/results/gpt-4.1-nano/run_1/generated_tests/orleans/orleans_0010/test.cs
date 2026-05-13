using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Runtime;

namespace Orleans.Storage.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldResolveOptionsAndCreateInstance()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var storageOptions = new DynamoDBStorageOptions { ServiceId = "test", TableName = "table" };
            optionsMock.Setup(o => o.Get(It.IsAny<string>())).Returns(storageOptions);

            var storageInstance = new Mock<DynamoDBGrainStorage>("name", storageOptions, null, null);
            var activatorMock = new Mock<IActivatorProvider>();
            var storageMock = new Mock<DynamoDBGrainStorage>("name", storageOptions, activatorMock.Object, null);

            servicesMock.Setup(s => s.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                        .Returns(optionsMock.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(servicesMock.Object, "name");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }

    public class DynamoDBGrainStorageTests
    {
        [Fact]
        public async Task Init_ShouldCallInitializeTable()
        {
            // Arrange
            var options = new DynamoDBStorageOptions
            {
                ServiceId = "test",
                TableName = "table",
                InitStage = 0
            };
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var storageMock = new Mock<DynamoDBStorage>(loggerMock.Object, null, null, null, null, null, 0, 0, false, false, false);
            storageMock.Setup(s => s.InitializeTable(It.IsAny<string>(), It.IsAny<System.Collections.Generic.List<KeySchemaElement>>(),
                It.IsAny<System.Collections.Generic.List<AttributeDefinition>>(), null, null))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var storage = new DynamoDBGrainStorage("name", options, null, loggerMock.Object);
            // Inject the mocked storage
            typeof(DynamoDBGrainStorage).GetField("storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(storage, storageMock.Object);

            // Act
            await storage.Init(CancellationToken.None);

            // Assert
            storageMock.Verify(s => s.InitializeTable(It.IsAny<string>(), It.IsAny<System.Collections.Generic.List<KeySchemaElement>>(),
                It.IsAny<System.Collections.Generic.List<AttributeDefinition>>(), null, null), Times.Once);
        }

        [Fact]
        public async Task ReadStateAsync_ShouldCallReadSingleEntryAsyncAndSetGrainState()
        {
            // Arrange
            var options = new DynamoDBStorageOptions
            {
                ServiceId = "test",
                TableName = "table"
            };
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var storageMock = new Mock<DynamoDBStorage>(loggerMock.Object, null, null, null, null, null, 0, 0, false, false, false);
            var grainStorage = new DynamoDBGrainStorage("name", options, null, loggerMock.Object);
            typeof(DynamoDBGrainStorage).GetField("storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(grainStorage, storageMock.Object);

            var grainStateMock = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "type", "id");
            var grainType = "testType";

            storageMock.Setup(s => s.ReadSingleEntryAsync(It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, AttributeValue>>(), It.IsAny<Func<Dictionary<string, AttributeValue>, GrainStateRecord>>()))
                .ReturnsAsync(new GrainStateRecord
                {
                    GrainType = grainType,
                    GrainReference = "ref",
                    ETag = 1,
                    State = new byte[] { 1, 2, 3 }
                });

            // Act
            await grainStorage.ReadStateAsync(grainType, grainId, grainStateMock.Object);

            // Assert
            storageMock.Verify(s => s.ReadSingleEntryAsync(It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, AttributeValue>>(), It.IsAny<Func<Dictionary<string, AttributeValue>, GrainStateRecord>>()), Times.Once);
            Assert.True(grainStateMock.Object.RecordExists);
        }

        [Fact]
        public async Task WriteStateAsync_ShouldCallWriteAndHandleException()
        {
            // Arrange
            var options = new DynamoDBStorageOptions
            {
                ServiceId = "test",
                TableName = "table"
            };
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var storageMock = new Mock<DynamoDBStorage>(loggerMock.Object, null, null, null, null, null, 0, 0, false, false, false);
            var grainStorage = new DynamoDBGrainStorage("name", options, null, loggerMock.Object);
            typeof(DynamoDBGrainStorage).GetField("storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(grainStorage, storageMock.Object);

            var grainStateMock = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "type", "id");
            var grainType = "testType";

            storageMock.Setup(s => s.ReadSingleEntryAsync(It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, AttributeValue>>(), It.IsAny<Func<Dictionary<string, AttributeValue>, GrainStateRecord>>()))
                .ReturnsAsync((GrainStateRecord)null);

            // Act
            await grainStorage.WriteStateAsync(grainType, grainId, grainStateMock.Object);

            // Assert
            storageMock.Verify(s => s.ReadSingleEntryAsync(It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, AttributeValue>>(), It.IsAny<Func<Dictionary<string, AttributeValue>, GrainStateRecord>>()), Times.Once);
        }

        [Fact]
        public async Task WriteStateAsync_ShouldThrowInconsistentStateException_OnConditionalCheckFailed()
        {
            // Arrange
            var options = new DynamoDBStorageOptions
            {
                ServiceId = "test",
                TableName = "table"
            };
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var storageMock = new Mock<DynamoDBStorage>(loggerMock.Object, null, null, null, null, null, 0, 0, false, false, false);
            var grainStorage = new DynamoDBGrainStorage("name", options, null, loggerMock.Object);
            typeof(DynamoDBGrainStorage).GetField("storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(grainStorage, storageMock.Object);

            var grainStateMock = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "type", "id");
            var grainType = "testType";

            storageMock.Setup(s => s.ReadSingleEntryAsync(It.IsAny<string>(), It.IsAny<System.Collections.Generic.Dictionary<string, AttributeValue>>(), It.IsAny<Func<Dictionary<string, AttributeValue>, GrainStateRecord>>()))
                .ReturnsAsync((GrainStateRecord)null);

            storageMock.Setup(s => s.WriteStateAsync(It.IsAny<string>(), It.IsAny<GrainStateRecord>(), false))
                .Throws(new ConditionalCheckFailedException("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InconsistentStateException>(() => grainStorage.WriteStateAsync(grainType, grainId, grainStateMock.Object));
        }
    }
}
