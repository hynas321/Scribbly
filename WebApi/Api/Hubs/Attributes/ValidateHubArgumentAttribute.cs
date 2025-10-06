namespace WebApi.Api.Hubs.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ValidateHubArgumentAttribute : Attribute
{
    public string ArgumentName { get; }
    public ValidationType Type { get; }

    public ValidateHubArgumentAttribute(string argumentName, ValidationType type)
    {
        ArgumentName = argumentName;
        Type = type;
    }
}

public enum ValidationType
{
    GameHash,
    HostToken,
    PlayerToken,
    DrawingToken,
}