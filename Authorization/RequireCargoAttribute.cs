using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireCargoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _allowedCargos;

    public RequireCargoAttribute(params string[] allowedCargos)
    {
        _allowedCargos = allowedCargos.Length > 0 ? allowedCargos : CargoPermissions.All;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            return;

        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        var cargo = await currentUser.GetCargoAsync(context.HttpContext.RequestAborted);

        if (CargoPermissions.IsAllowed(cargo, _allowedCargos))
            return;

        context.Result = new ObjectResult(
            ApiResponse<object>.Fail("Você não tem permissão para acessar este recurso."))
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
    }
}
