using SindiOps.API.Constants;

namespace SindiOps.API.Authorization;

public sealed class RequireAdminCargoAttribute : RequireCargoAttribute
{
    public RequireAdminCargoAttribute()
        : base(CargoConstants.Sindico, CargoConstants.Secretario)
    {
    }
}

public sealed class RequireExceptPorteiroCargoAttribute : RequireCargoAttribute
{
    public RequireExceptPorteiroCargoAttribute()
        : base(
            CargoConstants.Sindico,
            CargoConstants.Secretario,
            CargoConstants.Zelador,
            CargoConstants.Outro)
    {
    }
}

public sealed class RequireSindicoCargoAttribute : RequireCargoAttribute
{
    public RequireSindicoCargoAttribute()
        : base(CargoConstants.Sindico)
    {
    }
}

public sealed class RequireAllCargoAttribute : RequireCargoAttribute
{
    public RequireAllCargoAttribute()
        : base(
            CargoConstants.Sindico,
            CargoConstants.Secretario,
            CargoConstants.Zelador,
            CargoConstants.Porteiro,
            CargoConstants.Outro)
    {
    }
}
