namespace HospitalMS.BL.Exceptions;

public class BusinessRuleException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message) : base(message, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string ruleName, string message, Dictionary<string, object>? additionalData) : base(message, "BUSINESS_RULE_VIOLATION", additionalData)
    {
        RuleName = ruleName;
    }
}