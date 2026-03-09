namespace HospitalMS.BL.Exceptions;

public class ConflictException : DomainException
{
    public string ConflictType { get; }

    public ConflictException(string conflictType, string message) : base(message, "CONFLICT")
    {
        ConflictType = conflictType;
    }

    public ConflictException(string message) : base(message, "CONFLICT")
    {
        ConflictType = "DataConflict";
    }
}