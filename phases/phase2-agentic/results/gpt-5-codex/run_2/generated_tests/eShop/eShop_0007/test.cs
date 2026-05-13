using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShop.Ordering.API.Apis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Xunit;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var logger = new TestLogger<OrderServices>();
        var services = new OrderServices(null!, null!, null!, logger);

        var request = new CreateOrderRequest(
            UserId: "user-123",
            UserName: "Test User",
            City: "Seattle",
            Street: "1st Ave",
            State: "WA",
            Country: "USA",
            ZipCode: "98101",
            CardNumber: "1234567812345678",
            CardHolderName: "Test User",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer-123",
            Items: null!);

        // Act
        var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, services);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("RequestId is missing.", badRequest.Value);

        var warningEntry = Assert.Single(logger.Entries.Where(e => e.Level == LogLevel.Warning));
        Assert.Contains("Invalid IntegrationEvent - RequestId is missing", warningEntry.Message);

        var statePairs = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(warningEntry.State!);
        var integrationEventPair = Assert.Single(statePairs.Where(kvp => kvp.Key == "IntegrationEvent"));
        Assert.Same(request, integrationEventPair.Value);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }

        public sealed record LogEntry(LogLevel Level, string Message, object? State);

        public List<LogEntry> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
            Entries.Add(new LogEntry(logLevel, message, state));
        }
    }
}
