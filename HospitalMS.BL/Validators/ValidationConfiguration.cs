namespace HospitalMS.BL.Validators;

public static class ValidationConfiguration
{
    // working hours config
    public static class WorkingHours
    {
        public static readonly TimeSpan Start = new TimeSpan(8, 0, 0);

        public static readonly TimeSpan End = new TimeSpan(20, 0, 0);

        // check within hours
        public static bool IsWithinWorkingHours(TimeSpan time)
        {
            return time >= Start && time <= End;
        }

        // check working day
        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }
    }

    // appointment duration config
    public static class AppointmentDuration
    {
        public static readonly TimeSpan Minimum = new TimeSpan(0, 15, 0);

        public static readonly TimeSpan Maximum = new TimeSpan(2, 0, 0);

        public static readonly TimeSpan Standard = new TimeSpan(0, 30, 0);

        // check valid duration
        public static bool IsValidDuration(TimeSpan duration)
        {
            return duration >= Minimum && duration <= Maximum;
        }
    }

    // age validation config
    public static class AgeValidation
    {
        public const int MinimumAge = 0;

        public const int MaximumAge = 150;

        // calculate age in years
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;
           
            return age;
        }

        // check valid age
        public static bool IsValidAge(DateTime dateOfBirth)
        {
            var age = CalculateAge(dateOfBirth);
           
            return age >= MinimumAge && age <= MaximumAge;
        }

        // check if adult
        public static bool IsAdult(DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth) >= 18;
        }

        // check if minor
        public static bool IsMinor(DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth) < 18;
        }
    }

    // booking limits config
    public static class BookingLimits
    {
        public const int MaxAdvanceBookingMonths = 6;

        public const int MaxAppointmentsPerDay = 10;

        public const int MaxConcurrentAppointments = 1;

        public static DateTime MaxAdvanceBookingDate => DateTime.Today.AddMonths(MaxAdvanceBookingMonths);
    }

    // text length limits
    public static class TextLengthLimits
    {
        public const int ShortText = 100;

        public const int MediumText = 500;

        public const int LongText = 1000;

        public const int VeryLongText = 2000;

        public const int ExtraLongText = 5000;

        public const int PhoneNumber = 20;

        public const int Email = 100;

        public const int LicenseNumber = 50;
    }

    // medical data config
    public static class MedicalData
    {
        public static readonly string[] ValidBloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        public static readonly string[] ValidGenders = { "Male", "Female", "Other" };

        // check valid blood group
        public static bool IsValidBloodGroup(string? bloodGroup)
        {
            if (string.IsNullOrWhiteSpace(bloodGroup))
                return false;
           
            return ValidBloodGroups.Contains(bloodGroup.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        // check valid gender
        public static bool IsValidGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return false;
           
            return ValidGenders.Contains(gender.Trim(), StringComparer.OrdinalIgnoreCase);
        }
    }

    // password requirements config
    public static class PasswordRequirements
    {
        public const int MinimumLength = 8;

        public const int MaximumLength = 100;

        public const bool RequireUppercase = true;

        public const bool RequireLowercase = true;

        public const bool RequireDigit = true;

        public const bool RequireSpecialCharacter = true;
    }

    // numeric range limits
    public static class NumericRanges
    {
        public const int MinYearsOfExperience = 0;

        public const int MaxYearsOfExperience = 70;

        public const decimal MinConsultationFee = 0;

        public const decimal MaxConsultationFee = 100000;
    }

    // security pattern checks
    public static class SecurityPatterns
    {
        public static readonly string[] DangerousSqlPatterns =
        {
            "--", ";--", "/*", "*/", "xp_", "sp_", "DROP ", "DELETE ", "INSERT ", "UPDATE ", "SELECT ", "EXEC ", "EXECUTE ", "ALTER ", "CREATE "
        };

        public static readonly string[] DangerousXssPatterns =
        {
            "<script", "javascript:", "onerror=", "onclick=", "onload=", "<iframe", "<object", "<embed", "eval(", "expression(", "vbscript:", "data:text/html"
        };

        // check sql injection patterns
        public static bool ContainsDangerousSqlPatterns(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            return DangerousSqlPatterns.Any(pattern => input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        // check xss patterns
        public static bool ContainsDangerousXssPatterns(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            return DangerousXssPatterns.Any(pattern => input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}