namespace Supply.Wizard.Domain;

/// <summary>
/// Common service states abstracted across systemd and Windows Services.
/// </summary>
public enum ServiceStatus
{
    Unknown,
    Running,
    Stopped,
    NotFound,
}
