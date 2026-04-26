namespace GarbageCollection.API.Validators
{
    internal static class ValidatorConstants
    {
        public const string EmailRegex    = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";
        public const string PasswordRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+\[\]{}|;':"",./<>?`~\\]).{8,16}$";
        public const string PhoneRegex    = @"^\d{10,11}$";
        public const string OtpRegex      = @"^\d{6}$";
    }
}
