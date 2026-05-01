namespace PlayerApi.Tests.Fixtures;

public static class TestCredentials
{
    public const string CorrectServerLogin = "tester";
    public const string CorrectServerPassword = "tester123";

    public const string AttemptedCorrectLogin = CorrectServerLogin;
    public const string AttemptedCorrectPassword = CorrectServerPassword;

    public const string AttemptedIncorrectLogin = "wrong_user";
    public const string AttemptedIncorrectPassword = "wrong_password";
}
