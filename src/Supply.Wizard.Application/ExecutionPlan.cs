using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Application;

/// <summary>
/// Ordered runbook generated from the requested operation and manifest.
/// </summary>
public sealed record ExecutionPlan
{
    /// <summary>
    /// Gets the unique identifier for the planned run.
    /// </summary>
    public required Guid RunId { get; init; }

    /// <summary>
    /// Gets the requested operation kind.
    /// </summary>
    public required OperationKind Operation { get; init; }

    /// <summary>
    /// Gets the original request used to build this plan.
    /// </summary>
    public required WizardRequest Request { get; init; }

    /// <summary>
    /// Gets the manifest snapshot used for planning.
    /// </summary>
    public required ManifestDocument Manifest { get; init; }

    /// <summary>
    /// Gets the state snapshot before execution.
    /// </summary>
    public required WizardState InitialState { get; init; }

    /// <summary>
    /// Gets the expected state snapshot after successful execution.
    /// </summary>
    public required WizardState TargetState { get; init; }

    /// <summary>
    /// Gets the ordered steps to execute.
    /// </summary>
    public IReadOnlyList<IPlanStep> Steps { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the plan contains no steps.
    /// </summary>
    public bool IsEmpty => Steps.Count is 0;
}
