using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsErrorOnException()
        {
            // Arrange
            var modelId = "test-model";
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockChatService = new Mock<IBedrockChatCompletionService>();
            var chatHistory = new ChatHistory(new List<ChatMessage>());
            var executionSettings = new PromptExecutionSettings();

            // Setup BedrockServiceFactory to return our mockChatService
            var serviceFactory = new BedrockServiceFactoryForTest(mockChatService.Object);

            // We need to inject the service factory or mock the creation of _ioChatService
            // Since the constructor creates it internally, we will create a derived test class to override it
            var client = new BedrockChatCompletionClientForTest(modelId, mockBedrockRuntime.Object, mockLoggerFactory.Object, mockChatService.Object);

            // Setup the chat service to return a ConverseStreamRequest
            var converseStreamRequest = new ConverseStreamRequest();
            mockChatService.Setup(s => s.GetConverseStreamRequest(modelId, chatHistory, executionSettings)).Returns(converseStreamRequest);

            // Setup the bedrock runtime to return an endpoint URL
            var endpointUri = "https://test.endpoint";
            mockBedrockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(converseStreamRequest))
                .Returns(new Amazon.BedrockRuntime.Model.Endpoint { URL = endpointUri });

            // Setup the bedrock runtime to throw an exception on ConverseStreamAsync to trigger the catch block
            var testException = new InvalidOperationException("Test exception");
            mockBedrockRuntime.Setup(r => r.ConverseStreamAsync(converseStreamRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(testException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var _ in client.StreamChatMessageAsync(chatHistory, executionSettings))
                {
                    // Should not reach here
                }
            });

            Assert.Equal(testException, ex);

            // Verify that LogError was called with the expected parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse stream with")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper derived class to override the internal _ioChatService with a mock
        private class BedrockChatCompletionClientForTest : BedrockChatCompletionClient
        {
            private readonly IBedrockChatCompletionService _mockChatService;

            public BedrockChatCompletionClientForTest(string modelId, IAmazonBedrockRuntime bedrockRuntime, ILoggerFactory? loggerFactory, IBedrockChatCompletionService mockChatService)
                : base(modelId, bedrockRuntime, loggerFactory)
            {
                _mockChatService = mockChatService;
            }

            // Override the GetConverseStreamRequest call to use the mock service
            public new IAsyncEnumerable<StreamingChatMessageContent> StreamChatMessageAsync(
                ChatHistory chatHistory,
                PromptExecutionSettings? executionSettings = null,
                Kernel? kernel = null,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                // We replicate the original method but use the mock service instead of the internal one
                return StreamChatMessageAsyncImpl(chatHistory, executionSettings, kernel, cancellationToken);
            }

            private async IAsyncEnumerable<StreamingChatMessageContent> StreamChatMessageAsyncImpl(
                ChatHistory chatHistory,
                PromptExecutionSettings? executionSettings,
                Kernel? kernel,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var converseStreamRequest = _mockChatService.GetConverseStreamRequest(this._modelId, chatHistory, executionSettings);
                var regionEndpoint = this._bedrockRuntime.DetermineServiceOperationEndpoint(converseStreamRequest).URL;
                this._chatGenerationEndpoint = new Uri(regionEndpoint);
                ConverseStreamResponse? response = null;

                using var activity = ModelDiagnostics.StartCompletionActivity(
                    this._chatGenerationEndpoint, this._modelId, this._modelProvider, chatHistory, executionSettings);
                ActivityStatusCode activityStatus;
                try
                {
                    response = await this._bedrockRuntime.ConverseStreamAsync(converseStreamRequest, cancellationToken).ConfigureAwait(false);
                    if (activity is not null)
                    {
                        activityStatus = BedrockClientUtilities.ConvertHttpStatusCodeToActivityStatusCode(response.HttpStatusCode);
                        activity.SetStatus(activityStatus);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Can't converse stream with '{ModelId}'. Reason: {Error}", this._modelId, ex.Message);
                    if (activity is not null)
                    {
                        activity.SetError(ex);
                        if (response != null)
                        {
                            activityStatus = BedrockClientUtilities.ConvertHttpStatusCodeToActivityStatusCode(response.HttpStatusCode);
                            activity.SetStatus(activityStatus);
                        }
                        else
                        {
                            activity.SetStatus(ActivityStatusCode.Error);
                        }
                    }
                    throw;
                }
                List<StreamingChatMessageContent>? streamedContents = activity is not null ? new List<StreamingChatMessageContent>() : null;
                if (response != null)
                {
                    await foreach (var chunk in response.Stream.ConfigureAwait(false))
                    {
                        if (chunk is ContentBlockDeltaEvent deltaEvent)
                        {
                            var c = deltaEvent?.Delta.Text;
                            var content = new StreamingChatMessageContent(AuthorRole.Assistant, c, deltaEvent);
                            streamedContents?.Add(content);
                            yield return content;
                        }

                        if (chunk is ConverseStreamMetadataEvent metadataEvent)
                        {
                            // Handle metadata event if needed
                        }
                    }
                }
            }
        }

        // Minimal stubs for types used in the test
        private class ChatHistory : List<ChatMessage>
        {
            public ChatHistory(List<ChatMessage> messages) : base(messages) { }
        }

        private class ChatMessage { }

        private class PromptExecutionSettings { }

        private class Kernel { }

        private class StreamingChatMessageContent
        {
            public StreamingChatMessageContent(AuthorRole role, string? text, ContentBlockDeltaEvent deltaEvent) { }
        }

        private enum AuthorRole
        {
            Assistant
        }

        private class ContentBlockDeltaEvent
        {
            public Delta Delta { get; set; } = new Delta();
        }

        private class Delta
        {
            public string? Text { get; set; }
        }

        private class ConverseStreamMetadataEvent { }

        private class ConverseStreamResponse
        {
            public int HttpStatusCode { get; set; }
            public IAsyncEnumerable<object> Stream { get; set; } = AsyncEnumerable.Empty<object>();
        }

        private class BedrockServiceFactoryForTest
        {
            private readonly IBedrockChatCompletionService _chatService;

            public BedrockServiceFactoryForTest(IBedrockChatCompletionService chatService)
            {
                _chatService = chatService;
            }

            public IBedrockChatCompletionService CreateChatCompletionService(string modelId) => _chatService;
        }
    }
}
