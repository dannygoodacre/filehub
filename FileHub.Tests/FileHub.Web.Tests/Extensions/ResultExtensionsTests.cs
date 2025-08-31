using FileHub.Core.Common;
using FileHub.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FileHub.Web.Tests.Extensions;

[TestFixture]
public class ResultExtensionsTests : TestBase
{
    [Test]
    public void ToHttpResponse_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var internalResult = Result.Success();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public void ToHttpResponse_WhenFailed_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result.Failed();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public void ToHttpResponse_WhenInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var validationState = new ValidationState();
        validationState.AddError("Property 1", "Message 1");

        var internalResult = Result.Invalid(validationState);

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<ValidationProblemDetails>>());
    }

    [Test]
    public void ToHttpResponse_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var internalResult = Result.NotFound();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.NotFound()));
    }

    [Test]
    public void ToHttpResponse_WhenCancelled_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result.Cancelled();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public void ToHttpResponse_WhenInternalError_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result.InternalError();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public void ToHttpResponse_WhenValueAndSuccess_ShouldReturnOk()
    {
        // Arrange
        var internalResult = Result<int>.Success(123);

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.TypeOf<Ok<int>>());
    }

    [Test]
    public void ToHttpResponse_WhenValueAndFailed_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result<int>.Failed();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public void ToHttpResponse_WhenValueAndInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var internalResult = Result<int>.Invalid(new ValidationState());

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<ValidationProblemDetails>>());
    }

    [Test]
    public void ToHttpResponse_WhenValueAndNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var internalResult = Result<int>.NotFound();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.NotFound()));
    }

    [Test]
    public void ToHttpResponse_WhenValueAndCancelled_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result<int>.Cancelled();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public void ToHttpResponse_WhenValueAndInternalError_ShouldReturnInternalServerError()
    {
        // Arrange
        var internalResult = Result<int>.InternalError();

        // Act
        var result = internalResult.ToHttpResponse();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }
}
