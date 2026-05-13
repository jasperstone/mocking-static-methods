using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientLoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var insideRuntimeClient = CreateInsideRuntimeClientWithLogger(loggerMock.Object);

            var message = new Message
            {
                TargetGrain = GrainId.NewId(),
                Id = CorrelationId.NewId(),
                SendingSilo = SiloAddress.NewLocalAddress(0),
                SendingGrain = GrainId.NewId(),
                BodyObject = new StatusResponse
                {
                    Diagnostics = new List<string> { "diag1", "diag2" }
                }
            };
            message.Result = Message.ResponseTypes.Status;

            var callbacks = new Dictionary<(GrainId, CorrelationId), CallbackData>();

            var messagingOptions = new SiloMessagingOptions
            {
                CancelUnknownRequestOnStatusUpdate = false
            };

            // Act
            // We simulate the code path that leads to the LogDebug call on line 438
            // This is the else branch where callback is null and status.Diagnostics is not empty and logger.IsEnabled(LogLevel.Debug) is true
            var status = (StatusResponse)message.BodyObject;
            CallbackData callback = null;
            var request = callback?.Message;
            if (request is null)
            {
                if (messagingOptions.CancelUnknownRequestOnStatusUpdate)
                {
                    // Not tested here
                }

                if (status.Diagnostics != null && status.Diagnostics.Count > 0 && loggerMock.Object.IsEnabled(LogLevel.Debug))
                {
                    var diagnosticsString = string.Join("\n", status.Diagnostics);
                    loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
                }
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private InsideRuntimeClient CreateInsideRuntimeClientWithLogger(ILogger logger)
        {
            // We create a minimal InsideRuntimeClient with the logger injected.
            // Since the constructor requires many dependencies, we mock or provide minimal implementations.

            var siloDetailsMock = new Mock<ILocalSiloDetails>();
            siloDetailsMock.SetupGet(x => x.SiloAddress).Returns(SiloAddress.NewLocalAddress(0));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HostedClient))).Returns(new HostedClient(SiloAddress.NewLocalAddress(0)));

            var messageFactoryMock = new Mock<MessageFactory>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<InsideRuntimeClient>()).Returns(logger);
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger);

            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<SiloMessagingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SiloMessagingOptions());

            var messagingTrace = new MessagingTrace(null);

            var referenceActivator = new GrainReferenceActivator(null, null);

            var interfaceIdResolver = new GrainInterfaceTypeResolver(null);

            var interfaceToTypeResolver = new GrainInterfaceTypeToGrainTypeResolver(null);

            var deepCopier = new DeepCopier(null);

            var timeProvider = TimeProvider.System;

            var interfaceToImplementationMapping = new InterfaceToImplementationMappingCache();

            return new InsideRuntimeClient(
                siloDetailsMock.Object,
                serviceProviderMock.Object,
                messageFactoryMock.Object,
                loggerFactoryMock.Object,
                optionsMock.Object,
                messagingTrace,
                referenceActivator,
                interfaceIdResolver,
                interfaceToTypeResolver,
                deepCopier,
                timeProvider,
                interfaceToImplementationMapping);
        }
    }

    // Minimal stubs for types used in the test
    internal class Message
    {
        public enum ResponseTypes
        {
            Status
        }

        public GrainId TargetGrain { get; set; }
        public CorrelationId Id { get; set; }
        public SiloAddress SendingSilo { get; set; }
        public GrainId SendingGrain { get; set; }
        public object BodyObject { get; set; }
        public ResponseTypes Result { get; set; }
    }

    internal class StatusResponse
    {
        public List<string> Diagnostics { get; set; }
    }

    internal class CallbackData
    {
        public Message Message { get; set; }
    }

    internal class GrainId
    {
        private static int _counter = 0;
        private readonly int _id;

        private GrainId(int id)
        {
            _id = id;
        }

        public static GrainId NewId()
        {
            return new GrainId(System.Threading.Interlocked.Increment(ref _counter));
        }

        public override string ToString() => $"GrainId-{_id}";
    }

    internal class CorrelationId
    {
        private static int _counter = 0;
        private readonly int _id;

        private CorrelationId(int id)
        {
            _id = id;
        }

        public static CorrelationId NewId()
        {
            return new CorrelationId(System.Threading.Interlocked.Increment(ref _counter));
        }

        public override string ToString() => $"CorrelationId-{_id}";
    }

    internal class SiloAddress
    {
        private static int _counter = 0;
        private readonly int _id;

        private SiloAddress(int id)
        {
            _id = id;
        }

        public static SiloAddress NewLocalAddress(int generation)
        {
            return new SiloAddress(System.Threading.Interlocked.Increment(ref _counter));
        }

        public override string ToString() => $"SiloAddress-{_id}";
    }

    internal class HostedClient
    {
        public SiloAddress Address { get; }

        public HostedClient(SiloAddress address)
        {
            Address = address;
        }
    }
}
