namespace SindiOps.API.Constants;

/// <summary>Grupos de cargo alinhados ao frontend (sidebar / RoleGuard).</summary>
public static class CargoPermissions
{
    public static readonly string[] All =
        [CargoConstants.Sindico, CargoConstants.Secretario, CargoConstants.Zelador, CargoConstants.Porteiro, CargoConstants.Outro];

    public static readonly string[] ExceptPorteiro =
        [CargoConstants.Sindico, CargoConstants.Secretario, CargoConstants.Zelador, CargoConstants.Outro];

    public static readonly string[] Admin =
        [CargoConstants.Sindico, CargoConstants.Secretario];

    public static readonly string[] SindicoOnly =
        [CargoConstants.Sindico];

    public static bool IsAllowed(string cargo, params string[] allowed) =>
        !string.IsNullOrWhiteSpace(cargo) && allowed.Contains(cargo);

    public static bool CanAccessAdmin(string cargo) => IsAllowed(cargo, Admin);

    public static bool CanAccessManutencoes(string cargo) => IsAllowed(cargo, ExceptPorteiro);

    public static bool CanSeeManutencaoAlertas(string cargo) => CanAccessManutencoes(cargo);

    public static bool CanSeeOcorrenciaAlertas(string cargo) => IsAllowed(cargo, All);

    public static bool CanSeeComprasAlertas(string cargo) => CanAccessAdmin(cargo);

    public static bool CanSeeContratosAlertas(string cargo) => CanAccessAdmin(cargo);

    public static bool CanSeeContratoAgenda(string cargo) => CanAccessAdmin(cargo);

    public static bool CanSeeManutencaoAgenda(string cargo) => CanAccessManutencoes(cargo);

    public static bool CanSeeMandatoAgenda(string cargo) => IsAllowed(cargo, SindicoOnly);

    public static bool CanExportRelatorios(string cargo) => CanAccessAdmin(cargo);
}
