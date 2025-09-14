using FileHub.Application.Commands;
using FileHub.Application.Queries;
using FileHub.Web.Extensions;
using FileHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHub.Web.Controllers;

/// <summary>
/// Endpoints for handling categories.
/// </summary>
[Authorize]
[ApiController]
[Route("categories")]
public class CategoryController(IAddCategory addCategory, IGetAllCategories getAllCategories) : ControllerBase
{
    /// <summary>
    /// Add a new category.
    /// </summary>
    [HttpPost("add")]
    public async Task<IResult> AddCategoryAsync([FromForm] AddCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await addCategory.ExecuteAsync(request.Category, cancellationToken);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Get all categories.
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await getAllCategories.ExecuteAsync(cancellationToken);

        return result.ToHttpResponse();
    }
}
