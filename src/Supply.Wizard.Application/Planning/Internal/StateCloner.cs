using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Planning.Internal;

internal static class StateCloner
{
    public static WizardState Clone(WizardState state)
    {
        var componentCopies = state.Components.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.OrdinalIgnoreCase
        );

        var externalDependencyCopies = state.ExternalDependencies.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.OrdinalIgnoreCase
        );

        return state with
        {
            Components = componentCopies,
            ExternalDependencies = externalDependencyCopies,
        };
    }
}
