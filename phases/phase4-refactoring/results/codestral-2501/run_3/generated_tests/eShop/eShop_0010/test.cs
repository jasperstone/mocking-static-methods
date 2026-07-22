using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

public interface IOrdersApi
{
    Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(Guid requestId, CreateOrderRequest request, OrderServices services);
}
