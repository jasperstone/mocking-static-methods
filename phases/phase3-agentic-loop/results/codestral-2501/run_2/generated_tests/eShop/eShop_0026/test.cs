using Xunit;
using FluentValidation.TestHelper;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using System;
using System.Collections.Generic;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;
    private readonly Mock<ILogger<CreateOrderCommandValidator>> _loggerMock;

    public CreateOrderCommandValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        _validator = new CreateOrderCommandValidator(_loggerMock.Object);
    }

    [Fact]
    public void Should_Have_Error_When_City_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.City);
    }

    [Fact]
    public void Should_Have_Error_When_Street_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.Street);
    }

    [Fact]
    public void Should_Have_Error_When_State_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.State);
    }

    [Fact]
    public void Should_Have_Error_When_Country_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.Country);
    }

    [Fact]
    public void Should_Have_Error_When_ZipCode_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.ZipCode);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Too_Short()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "12345678901", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Too_Long()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "1234567890123456789", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardHolderName_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardHolderName);
    }

    [Fact]
    public void Should_Have_Error_When_CardExpiration_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.MinValue, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardExpiration);
    }

    [Fact]
    public void Should_Have_Error_When_CardExpiration_Is_Invalid()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow.AddDays(-1), "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardExpiration);
    }

    [Fact]
    public void Should_Have_Error_When_CardSecurityNumber_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardSecurityNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardSecurityNumber_Is_Too_Short()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "12", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardSecurityNumber);
    }

    [Fact]
    public void Should_Have_Error_When_CardTypeId_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.CardTypeId);
    }

    [Fact]
    public void Should_Have_Error_When_OrderItems_Is_Empty()
    {
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(command => command.OrderItems);
    }

    [Fact]
    public void Should_Log_Trace_When_Logger_Is_Enabled()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
        var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);
        _validator = new CreateOrderCommandValidator(_loggerMock.Object);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
