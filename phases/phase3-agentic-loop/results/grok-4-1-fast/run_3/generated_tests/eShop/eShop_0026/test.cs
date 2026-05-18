using Xunit;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using System.Reflection;

namespace eShop.Ordering.API.Application.Tests.Validations;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenLoggerTraceEnabled_LogsTraceMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Constructor_WhenLoggerTraceDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommandViaReflection();

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidCity_ReturnsInvalid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommandViaReflection();
        typeof(CreateOrderCommand).GetProperty("City")!
            .SetValue(command, "");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City");
    }

    private static CreateOrderCommand CreateValidCommandViaReflection()
    {
        var command = new CreateOrderCommand();
        
        var properties = typeof(CreateOrderCommand).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        properties.First(p => p.Name == "UserId")!.SetValue(command, "user1");
        properties.First(p => p.Name == "UserName")!.SetValue(command, "user");
        properties.First(p => p.Name == "City")!.SetValue(command, "city");
        properties.First(p => p.Name == "Street")!.SetValue(command, "street");
        properties.First(p => p.Name == "State")!.SetValue(command, "state");
        properties.First(p => p.Name == "Country")!.SetValue(command, "country");
        properties.First(p => p.Name == "ZipCode")!.SetValue(command, "zip");
        properties.First(p => p.Name == "CardNumber")!.SetValue(command, "1234567890123456");
        properties.First(p => p.Name == "CardHolderName")!.SetValue(command, "John Doe");
        properties.First(p => p.Name == "CardExpiration")!.SetValue(command, DateTime.UtcNow.AddYears(1));
        properties.First(p => p.Name == "CardSecurityNumber")!.SetValue(command, "123");
        properties.First(p => p.Name == "CardTypeId")!.SetValue(command, 1);

        // Set _orderItems field to have at least one item (using object as placeholder since OrderItemDTO not available)
        var orderItemsField = typeof(CreateOrderCommand).GetField("_orderItems", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        orderItemsField!.SetValue(command, new List<object> { new object() });

        return command;
    }
}
