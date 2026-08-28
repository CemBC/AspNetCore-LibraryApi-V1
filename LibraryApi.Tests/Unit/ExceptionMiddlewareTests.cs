using LibraryApi.Exceptions;
using LibraryApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibraryApi.Tests.Unit;

public class ExceptionMiddlewareTests
{
    [Theory]
    [InlineData("notfound", StatusCodes.Status404NotFound)]
    [InlineData("badrequest", StatusCodes.Status400BadRequest)]
    [InlineData("unauthorized", StatusCodes.Status401Unauthorized)]
    [InlineData("unexpected", StatusCodes.Status500InternalServerError)]
    public async Task InvokeAsync_MapsExceptionsToExpectedStatusCode(
        string exceptionType,
        int expectedStatusCode)
    {
        RequestDelegate next = _ =>
        {
            Exception exception = exceptionType switch
            {
                "notfound" => new NotFoundException("missing"),
                "badrequest" => new BadRequestException("bad"),
                "unauthorized" => new UnauthorizedException("unauthorized"),
                _ => new InvalidOperationException("boom")
            };

            return Task.FromException(exception);
        };

        ExceptionMiddleware middleware = new(
            next,
            NullLogger<ExceptionMiddleware>.Instance);

        DefaultHttpContext context = new();
        context.Request.Method = "GET";
        context.Request.Path = "/test";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
    }
}
