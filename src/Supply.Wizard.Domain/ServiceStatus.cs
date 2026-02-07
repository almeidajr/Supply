namespace Supply.Wizard.Domain;

/// <summary>
/// Common service states abstracted across systemd and Windows Services.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// Indicates unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Indicates running.
    /// </summary>
    Running,

    /// <summary>
    /// Indicates stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Indicates not found.
    /// </summary>
    NotFound,
}
