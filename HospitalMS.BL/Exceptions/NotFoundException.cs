namespace HospitalMS.BL.Exceptions;

public class NotFoundException : DomainException
{
    public string ResourceType { get; }

    public object ResourceId { get; }

    public NotFoundException(string resourceType, object resourceId) : base($"{resourceType} with ID '{resourceId}' was not found.", "NOT_FOUND")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string message) : base(message, "NOT_FOUND")
    {
        ResourceType = "Resource";
        ResourceId = "Unknown";
    }
}