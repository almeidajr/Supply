using System.Runtime.InteropServices;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Application.Planning.Internal;
using Supply.Wizard.Application.Steps;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application.Planning;

/// <summary>
/// Default plan generation logic using dependency resolution and topological ordering.
/// </summary>
public sealed class DefaultPlanBuilder : IPlanBuilder
{
    /// <inheritdoc />
    public Task<ExecutionPlan> BuildAsync(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var componentById = manifest.Components.ToDictionary(
            component => component.Id,
            StringComparer.OrdinalIgnoreCase
        );
        var dependencyById = manifest.Dependencies.ToDictionary(
            dependency => dependency.Id,
            StringComparer.OrdinalIgnoreCase
        );

        var selectedComponentIds = ResolveSelectedComponents(request, manifest, state);

        var externalDependencies = new HashSet<string>();
        ResolveDependencyPolicies(
            request,
            selectedComponentIds,
            componentById,
            dependencyById,
            state,
            externalDependencies
        );

        var dependenciesByNode = BuildDependencyGraph(selectedComponentIds, request, componentById, dependencyById);

        var ordered = TopologicalSorter.Sort(selectedComponentIds, dependenciesByNode);

        if (request.Operation is OperationKind.Uninstall)
        {
            ordered = [.. ordered.Reverse()];
        }

        var steps = new List<IPlanStep>();
        AddDependencyValidationSteps(request, externalDependencies, steps);
        AddComponentSteps(request, manifest, state, componentById, ordered, steps);

        var targetState = BuildTargetState(request, manifest, state, ordered);

        var plan = new ExecutionPlan
        {
            RunId = Guid.NewGuid(),
            Operation = request.Operation,
            Request = request,
            Manifest = manifest,
            InitialState = StateCloner.Clone(state),
            TargetState = targetState,
            Steps = steps,
        };

        return Task.FromResult(plan);
    }

    private static HashSet<string> ResolveSelectedComponents(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state
    )
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var selected = new HashSet<string>(comparer);

        if (request.TargetComponentIds.Count > 0)
        {
            selected.UnionWith(request.TargetComponentIds);
            return selected;
        }

        switch (request.Operation)
        {
            case OperationKind.Install:
                selected.UnionWith(
                    manifest.Components.Where(component => component.EnabledByDefault).Select(component => component.Id)
                );
                break;
            case OperationKind.Update:
                if (state.Components.Count > 0)
                {
                    selected.UnionWith(state.Components.Keys);
                }
                else
                {
                    selected.UnionWith(
                        manifest
                            .Components.Where(component => component.EnabledByDefault)
                            .Select(component => component.Id)
                    );
                }

                break;
            case OperationKind.Uninstall:
                selected.UnionWith(state.Components.Keys);
                break;
            default:
                throw new WizardValidationException($"Unsupported operation '{request.Operation}'.");
        }

        return selected;
    }

    private static void ResolveDependencyPolicies(
        WizardRequest request,
        HashSet<string> selectedComponentIds,
        Dictionary<string, ComponentManifest> componentById,
        Dictionary<string, DependencyManifest> dependencyById,
        WizardState state,
        HashSet<string> externalDependencies
    )
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var componentId in selectedComponentIds.ToArray())
            {
                if (!componentById.TryGetValue(componentId, out var component))
                {
                    if (request.Operation is OperationKind.Uninstall)
                    {
                        continue;
                    }

                    throw new WizardValidationException($"Component '{componentId}' is not present in the manifest.");
                }

                foreach (var dependencyId in component.DependencyIds)
                {
                    if (!dependencyById.TryGetValue(dependencyId, out var dependency))
                    {
                        throw new WizardValidationException(
                            $"Dependency '{dependencyId}' required by component '{componentId}' is missing in the manifest."
                        );
                    }

                    var policy = request.DependencyPolicies.TryGetValue(dependencyId, out var requestedPolicy)
                        ? requestedPolicy
                        : dependency.DefaultPolicy;

                    if (policy is DependencyPolicy.Managed)
                    {
                        if (string.IsNullOrWhiteSpace(dependency.ManagedComponentId))
                        {
                            throw new WizardValidationException(
                                $"Dependency '{dependencyId}' does not define a managed component."
                            );
                        }

                        if (!componentById.ContainsKey(dependency.ManagedComponentId))
                        {
                            throw new WizardValidationException(
                                $"Managed component '{dependency.ManagedComponentId}' for dependency '{dependencyId}' is missing in the manifest."
                            );
                        }

                        changed |= selectedComponentIds.Add(dependency.ManagedComponentId);
                        continue;
                    }

                    if (
                        !request.ExternalDependencyEndpoints.ContainsKey(dependencyId)
                        && !state.ExternalDependencies.ContainsKey(dependencyId)
                    )
                    {
                        throw new DependencyValidationException(
                            $"Dependency '{dependencyId}' is external but no endpoint was provided."
                        );
                    }

                    externalDependencies.Add(dependencyId);
                }
            }
        } while (changed);
    }

    private static Dictionary<string, IReadOnlyCollection<string>> BuildDependencyGraph(
        HashSet<string> selectedComponentIds,
        WizardRequest request,
        Dictionary<string, ComponentManifest> componentById,
        Dictionary<string, DependencyManifest> dependencyById
    )
    {
        var dependenciesByNode = selectedComponentIds.ToDictionary(
            componentId => componentId,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var componentId in selectedComponentIds)
        {
            if (!componentById.TryGetValue(componentId, out var component))
            {
                continue;
            }

            foreach (var dependsOn in component.DependsOnComponentIds.Where(selectedComponentIds.Contains))
            {
                dependenciesByNode[componentId].Add(dependsOn);
            }

            foreach (var dependencyId in component.DependencyIds)
            {
                if (!dependencyById.TryGetValue(dependencyId, out var dependency))
                {
                    continue;
                }

                var policy = request.DependencyPolicies.TryGetValue(dependencyId, out var requestedPolicy)
                    ? requestedPolicy
                    : dependency.DefaultPolicy;

                if (policy is DependencyPolicy.Managed && !string.IsNullOrWhiteSpace(dependency.ManagedComponentId))
                {
                    dependenciesByNode[componentId].Add(dependency.ManagedComponentId);
                }
            }
        }

        return dependenciesByNode.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<string>)[.. pair.Value],
            StringComparer.OrdinalIgnoreCase
        );
    }

    private static void AddDependencyValidationSteps(
        WizardRequest request,
        IEnumerable<string> externalDependencies,
        List<IPlanStep> steps
    )
    {
        if (request.Operation is OperationKind.Uninstall)
        {
            return;
        }

        foreach (
            var dependencyId in externalDependencies.OrderBy(
                static dependencyId => dependencyId,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            if (!request.ExternalDependencyEndpoints.TryGetValue(dependencyId, out Uri? endpoint))
            {
                continue;
            }

            steps.Add(new ValidateExternalDependencyStep(dependencyId, endpoint));
        }
    }

    private static void AddComponentSteps(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state,
        Dictionary<string, ComponentManifest> componentById,
        IReadOnlyList<string> orderedComponentIds,
        List<IPlanStep> steps
    )
    {
        foreach (var componentId in orderedComponentIds)
        {
            if (request.Operation is OperationKind.Uninstall)
            {
                steps.Add(new UninstallComponentStep(componentId, request.PurgeData));
                continue;
            }

            if (!componentById.TryGetValue(componentId, out var component))
            {
                continue;
            }

            var isInstalled = state.Components.TryGetValue(componentId, out var installedState);
            var shouldApply = request.Operation switch
            {
                OperationKind.Install => !isInstalled
                    || !string.Equals(installedState!.Version, component.Version, StringComparison.OrdinalIgnoreCase),
                OperationKind.Update => !isInstalled
                    || !string.Equals(installedState!.Version, component.Version, StringComparison.OrdinalIgnoreCase),
                _ => false,
            };

            if (!shouldApply)
            {
                continue;
            }

            var artifact = SelectArtifact(manifest, component);
            steps.Add(new InstallOrUpdateComponentStep(component, artifact));
        }
    }

    private static ArtifactManifest SelectArtifact(ManifestDocument manifest, ComponentManifest component)
    {
        string os;
        if (OperatingSystem.IsWindows())
        {
            os = "windows";
        }
        else if (OperatingSystem.IsLinux())
        {
            os = "linux";
        }
        else
        {
            throw new WizardValidationException("Unsupported operating system.");
        }

        var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        var artifact =
            component.Artifacts.FirstOrDefault(candidate =>
                string.Equals(candidate.Os, os, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.Architecture, architecture, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new WizardValidationException(
                $"No artifact found for component '{component.Id}' on '{os}/{architecture}' in manifest '{manifest.ManifestVersion}'."
            );
        return artifact;
    }

    private static WizardState BuildTargetState(
        WizardRequest request,
        ManifestDocument manifest,
        WizardState state,
        IReadOnlyList<string> orderedComponentIds
    )
    {
        var target = StateCloner.Clone(state) with
        {
            LastManifestVersion = manifest.ManifestVersion,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

        if (request.Operation is OperationKind.Uninstall)
        {
            foreach (var componentId in orderedComponentIds)
            {
                target.Components.Remove(componentId);
            }

            return target;
        }

        var componentById = manifest.Components.ToDictionary(
            component => component.Id,
            StringComparer.OrdinalIgnoreCase
        );
        var stateDirectoryPath = Path.GetDirectoryName(request.StateFilePath) ?? Directory.GetCurrentDirectory();

        foreach (var componentId in orderedComponentIds)
        {
            if (!componentById.TryGetValue(componentId, out var component))
            {
                continue;
            }

            var serviceName = string.IsNullOrWhiteSpace(component.Service.ServiceName)
                ? component.Id
                : component.Service.ServiceName;
            var installedPath = Path.Combine(stateDirectoryPath, "components", component.Id, component.Version);

            target.Components[componentId] = new InstalledComponentState
            {
                ComponentId = componentId,
                Version = component.Version,
                InstalledPath = installedPath,
                ServiceName = serviceName,
                InstalledAtUtc = DateTimeOffset.UtcNow,
            };
        }

        return target;
    }
}
