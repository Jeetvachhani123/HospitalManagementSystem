namespace HospitalMS.BL.Common;

public static class Constants
{
    // role name constants
    public static class Roles
    {
        public const string Admin = "Admin";

        public const string Doctor = "Doctor";

        public const string Patient = "Patient";
    }

    // system message constants
    public static class Messages
    {
        public const string LoginSuccess = "Login successful";

        public const string LoginFailed = "Invalid email or password";

        public const string RegistrationSuccess = "Registration successful";

        public const string EmailAlreadyExists = "Email already exists";

        public const string UserNotFound = "User not found";

        public const string DoctorNotFound = "Doctor not found";

        public const string PatientNotFound = "Patient not found";

        public const string AppointmentNotFound = "Appointment not found";

        public const string AppointmentConflict = "Doctor is not available at the selected time";

        public const string AppointmentCreated = "Appointment created successfully";

        public const string AppointmentUpdated = "Appointment updated successfully";

        public const string AppointmentCancelled = "Appointment cancelled successfully";

        public const string UnauthorizedAccess = "Unauthorized access";

        public const string InvalidOperation = "Invalid operation";
    }

    // validation message constants
    public static class ValidationMessages
    {
        public const string EmailRequired = "Email is required";

        public const string EmailInvalid = "Email format is invalid";

        public const string PasswordRequired = "Password is required";

        public const string PasswordMinLength = "Password must be at least 6 characters";

        public const string FirstNameRequired = "First name is required";

        public const string LastNameRequired = "Last name is required";

        public const string SpecializationRequired = "Specialization is required";

        public const string LicenseNumberRequired = "License number is required";

        public const string AppointmentDateRequired = "Appointment date is required";

        public const string AppointmentDateInvalid = "Appointment date must be in the future";
    }
}