namespace HospitalMS.BL.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException() : base("You are not authorized to perform this operation.", "UNAUTHORIZED")
    {
    }
}