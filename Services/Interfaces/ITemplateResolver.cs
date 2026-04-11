namespace SindiCore.API.Services.Interfaces;

public interface ITemplateResolver
{
    /// <param name="propertyNameForErrors">Nome do campo nas mensagens de validação quando existem tokens não resolvidos.</param>
    string Resolve(string template, Dictionary<string, string> values, string? propertyNameForErrors = null);
}
