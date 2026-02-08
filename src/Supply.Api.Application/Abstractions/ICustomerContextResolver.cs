using Microsoft.AspNetCore.Http;
using Supply.Api.Domain.Contracts;

namespace Supply.Api.Application.Abstractions;

public interface ICustomerContextResolver
{
    CustomerContext Resolve(HttpContext httpContext);
}
