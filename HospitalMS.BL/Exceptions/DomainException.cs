namespace HospitalMS.BL.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public Dictionary<string, object>? AdditionalData { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR") : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Dictionary<string, object>? additionalData) : base(message)
    {
        ErrorCode = errorCode;
        AdditionalData = additionalData;
    }
}