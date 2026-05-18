using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void Logger_LogDebug_IsCalled_ForUnknownRequestWithStatusDiagnostics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(InsideRuntimeClient).FullName))
                .Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<InsideRuntimeClient>())
                .Returns(loggerMock.Object);

            var messagingOptions = new SiloMessagingOptions
            {
                CancelUnknownRequestOnStatusUpdate = true
            };

            var messagingOptionsMock = new Mock<Microsoft.Extensions.Options.IOptions<SiloMessagingOptions>>();
            messagingOptionsMock.Setup(m => m.Value).Returns(messagingOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HostedClient))).Returns(new HostedClient(null, null, null, null));

            var insideRuntimeClient = new InsideRuntimeClient(
                siloDetails: new TestLocalSiloDetails(),
                serviceProvider: serviceProviderMock.Object,
                messageFactory: new MessageFactory(),
                loggerFactory: loggerFactoryMock.Object,
                messagingOptions: messagingOptionsMock.Object,
                messagingTrace: null,
                referenceActivator: null,
                interfaceIdResolver: null,
                interfaceToTypeResolver: null,
                deepCopier: null,
                timeProvider: null,
                interfaceToImplementationMapping: null);

            // Setup logger to enable Debug level
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            // Prepare message with unknown callback and status diagnostics
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse
                {
                    Diagnostics = new List<string> { "diag1", "diag2" }
                },
                TargetGrain = GrainId.NewId(),
                Id = CorrelationId.NewId(),
                SendingSilo = null,
                SendingGrain = GrainId.NewId()
            };

            // Setup callbacks dictionary to be empty (unknown request)
            var callbacksField = typeof(InsideRuntimeClient).GetField("callbacks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var callbacks = new System.Collections.Concurrent.ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
            callbacksField.SetValue(insideRuntimeClient, callbacks);

            // Act
            // We need to invoke the method containing the code snippet.
            // The snippet is inside a method that processes messages.
            // The method is not fully visible, but likely named something like ReceiveMessage or similar.
            // We will try to find a method that takes a Message and processes it.

            var method = typeof(InsideRuntimeClient).GetMethod("ReceiveMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null)
            {
                // If no such method, fallback to a method named "ReceiveMessage" with any signature
                foreach (var m in typeof(InsideRuntimeClient).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (m.Name.Contains("Receive") && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Message))
                    {
                        method = m;
                        break;
                    }
                }
            }

            Assert.NotNull(method);

            method.Invoke(insideRuntimeClient, new object[] { message });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Minimal stub classes to satisfy dependencies
        private class TestLocalSiloDetails : ILocalSiloDetails
        {
            public SiloAddress SiloAddress { get; } = SiloAddress.NewLocalAddress(0);
            public string Name { get; } = "TestSilo";
            public string ClusterId { get; } = "TestCluster";
            public string ServiceId { get; } = "TestService";
        }

        private class MessageFactory
        {
            public Message CreateMessage(IInvokable request, InvokeMethodOptions options) => new Message();
        }

        private class HostedClient
        {
            public GrainAddress Address { get; }
            public HostedClient(object a, object b, object c, object d)
            {
                Address = new GrainAddress(GrainId.NewId());
            }
        }

        private class GrainAddress
        {
            public GrainId GrainId { get; }
            public GrainAddress(GrainId grainId) => GrainId = grainId;
        }

        private class Message
        {
            public enum ResponseTypes { Status, Other }
            public ResponseTypes Result { get; set; }
            public object BodyObject { get; set; }
            public GrainId TargetGrain { get; set; }
            public CorrelationId Id { get; set; }
            public SiloAddress SendingSilo { get; set; }
            public GrainId SendingGrain { get; set; }
            public bool IsSystemMessage { get; set; }
            public int InterfaceType { get; set; }
            public int InterfaceVersion { get; set; }
            public SiloAddress TargetSilo { get; set; }
            public bool IsExpirableMessage() => false;
            public TimeSpan TimeToLive { get; set; }
        }

        private class StatusResponse
        {
            public List<string> Diagnostics { get; set; }
        }

        private class GrainId
        {
            private static int _counter = 0;
            private readonly int _id;
            private GrainId(int id) { _id = id; }
            public static GrainId NewId() => new GrainId(System.Threading.Interlocked.Increment(ref _counter));
        }

        private class CorrelationId
        {
            private static int _counter = 0;
            private readonly int _id;
            private CorrelationId(int id) { _id = id; }
            public static CorrelationId NewId() => new CorrelationId(System.Threading.Interlocked.Increment(ref _counter));
        }

        private class SiloAddress
        {
            public static SiloAddress NewLocalAddress(int port) => new SiloAddress();
        }

        private class CallbackData
        {
            public Message Message { get; set; }
            public void OnStatusUpdate(StatusResponse status) { }
        }

        private interface IInvokable
        {
            CancellationToken GetCancellationToken();
            TimeSpan? GetDefaultResponseTimeout();
        }

        private interface IResponseCompletionSource { }

        private enum InvokeMethodOptions
        {
            OneWay = 1
        }
    }
}
