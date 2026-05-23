using MediRoute.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace MediRoute.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Convert a Result&lt;T&gt; into an ActionResult.</summary>
    protected ActionResult<T> ToActionResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });

    protected ActionResult ToActionResult(Result result) =>
        result.IsSuccess
            ? NoContent()
            : StatusCode(result.StatusCode, new { error = result.Error });

    protected int? CurrentHospitalId()
    {
        var claim = User.FindFirst("hospitalId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    protected string? CurrentUsername() => User.Identity?.Name;

    protected bool IsSuperAdmin() => User.IsInRole("SuperAdmin");
}
