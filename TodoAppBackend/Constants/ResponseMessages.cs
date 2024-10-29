namespace TodoAppBackend.Constants
{
    public static class ResponseMessages
    {
        public const string INVALID_CREDENTIALS = "Invalid username or password.";
        public const string INVALID_REFRESH_TOKEN = "Invalid refresh token.";
        public const string INVALID_ID_IN_RT = "Invalid user ID in refresh token.";
        public const string USER_NOT_FOUND = "User not found.";
        public const string UNSUPPORTED_GRANT_TYPE = "Unsupported grant type.";
    }
}
