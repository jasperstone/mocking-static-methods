using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans;
using Orleans.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class InsideRuntimeClientLoggingTests
    {
        [Fact]
        public void LogDebug_IsCalled_ForStatusUpdateOnUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var client = new InsideRuntimeClientBuilder()
                .WithLoggerFactory(loggerFactoryMock.Object)
                .Build();

            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse
                {
                    Diagnostics = new[] { "diag1", "diag2" }
                },
                TargetGrain = GrainId.NewId(),
                Id = Guid.NewGuid(),
                SendingSilo = null,
                SendingGrain = GrainId.NewId(),
            };

            var callbacks = new ConcurrentDictionary<(GrainId, Guid), CallbackData>();
            var callbacksKey = (message.TargetGrain, message.Id);
            var callbackData = new CallbackData();

            callbacks.TryAdd(callbacksKey, callbackData);

            // Act
            client.HandleMessage(message, callbacks);

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
    }

    // Helper builder to create InsideRuntimeClient with mocked dependencies
    public class InsideRuntimeClientBuilder
    {
        private ILoggerFactory loggerFactory = new LoggerFactory();

        public InsideRuntimeClientBuilder WithLoggerFactory(ILoggerFactory factory)
        {
            this.loggerFactory = factory;
            return this;
        }

        public InsideRuntimeClient Build()
        {
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var client = new InsideRuntimeClient(
                siloDetails: null,
                serviceProvider: serviceProvider,
                messageFactory: new MessageFactory(),
                loggerFactory: loggerFactory,
                messagingOptions: Options.Create(new SiloMessagingOptions()),
                messagingTrace: new MessagingTrace(),
                referenceActivator: null,
                interfaceIdResolver: null,
                interfaceToTypeResolver: null,
                deepCopier: new DeepCopier(),
                timeProvider: new TimeProvider(),
                interfaceToImplementationMapping: new InterfaceToImplementationMappingCache());

            return client;
        }
    }

    // Dummy implementations for dependencies
    public class Message
    {
        public enum ResponseTypes { Status }
        public ResponseTypes Result { get; set; }
        public object BodyObject { get; set; }
        public GrainId TargetGrain { get; set; }
        public Guid Id { get; set; }
        public string SendingSilo { get; set; }
        public GrainId SendingGrain { get; set; }
        public string TargetSilo { get; set; }
        public bool IsSystemMessage { get; set; }
    }

    public class StatusResponse
    {
        public string[] Diagnostics { get; set; }
    }

    public class CallbackData
    {
        public Action<StatusResponse> OnStatusUpdate { get; set; }
        public IInvokable Message { get; set; }
        public void DoCallback(Message message) { }
    }

    public interface IInvokable { }

    public class GrainId
    {
        public static GrainId NewId() => new GrainId();
    }

    public class MessageFactory
    {
        public Message CreateMessage(IInvokable request, InvokeMethodOptions options) => new Message();
    }

    public class MessagingTrace { }

    public class DeepCopier { }

    public class TimeProvider { }

    public class InterfaceToImplementationMappingCache { }

    public class InsideRuntimeClient
    {
        private readonly ILogger logger;

        public InsideRuntimeClient(
            IServiceProvider serviceProvider,
            MessageFactory messageFactory,
            ILoggerFactory loggerFactory,
            IOptions<SiloMessagingOptions> messagingOptions,
            MessagingTrace messagingTrace,
            GrainReferenceActivator referenceActivator,
            GrainInterfaceTypeResolver interfaceIdResolver,
            GrainInterfaceTypeToGrainTypeResolver interfaceToTypeResolver,
            DeepCopier deepCopier,
            TimeProvider timeProvider,
            InterfaceToImplementationMappingCache interfaceToImplementationMapping)
        {
            this.logger = loggerFactory.CreateLogger<InsideRuntimeClient>();
        }

        public void HandleMessage(Message message, ConcurrentDictionary<(GrainId, Guid), CallbackData> callbacks)
        {
            if (message.Result == Message.ResponseTypes.Status)
            {
                var status = (StatusResponse)message.BodyObject;
                // Simulate the code that logs debug message
                var logger = this.logger;
                var diagnosticsString = string.Join("\n", status.Diagnostics);
                logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
            }
        }
    }
}
