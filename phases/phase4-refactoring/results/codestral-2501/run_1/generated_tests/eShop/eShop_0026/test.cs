using Xunit;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using eShop.Ordering.API.Application.Validations;

public class CreateOrderCommandValidatorTests
{
    private readonly Mock<ILogger<CreateOrderCommandValidator>> _loggerMock;
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        _validator = new CreateOrderCommandValidator(_loggerMock.Object);
    }

    [Fact]
    public void Should_LogTrace_When_LoggerIsEnabled()
    {
        // Arrange
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        var validator = new CreateOrderCommandValidator(_loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void Should_NotLogTrace_When_LoggerIsNotEnabled()
    {
        // Arrange
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        var validator = new CreateOrderCommandValidator(_loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public void Should_Have_Error_When_City_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { City = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Should_Have_Error_When_Street_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { Street = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Fact]
    public void Should_Have_Error_When_State_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { State = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void Should_Have_Error_When_Country_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { Country = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Should_Have_Error_When_ZipCode_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { ZipCode = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ZipCode);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { CardNumber = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_IsTooShort()
    {
        // Arrange
        var command = new CreateOrderCommand { CardNumber = "12345678901" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_IsTooLong()
    {
        // Arrange
        var command = new CreateOrderCommand { CardNumber = "1234567890123456789" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardHolderName_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { CardHolderName = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName);
    }

    [Fact]
    public void Should_Have_Error_When_CardExpiration_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { CardExpiration = DateTime.MinValue };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardExpiration);
    }

    [Fact]
    public void Should_Have_Error_When_CardExpiration_IsInThePast()
    {
        // Arrange
        var command = new CreateOrderCommand { CardExpiration = DateTime.UtcNow.AddDays(-1) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardExpiration);
    }

    [Fact]
    public void Should_Have_Error_When_CardSecurityNumber_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { CardSecurityNumber = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardSecurityNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardSecurityNumber_IsTooShort()
    {
        // Arrange
        var command = new CreateOrderCommand { CardSecurityNumber = "12" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardSecurityNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardSecurityNumber_IsTooLong()
    {
        // Arrange
        var command = new CreateOrderCommand { CardSecurityNumber = "1234" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardSecurityNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardTypeId_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { CardTypeId = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardTypeId);
    }

    [Fact]
    public void Should_Have_Error_When_OrderItems_IsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand { OrderItems = new List<OrderItemDTO>() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderItems);
    }
}
