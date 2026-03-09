namespace HospitalMS.BL.Exceptions;

public class ConcurrencyException : DomainException
{
    public string ResourceType { get; }

    public int ResourceId { get; }

    public ConcurrencyException(string resourceType, int resourceId) : base(message: $"The {resourceType} with ID {resourceId} has been modified by another user. " + "Please refresh and try again.", errorCode: "CONCURRENCY_CONFLICT")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}