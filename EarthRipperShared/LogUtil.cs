namespace EarthRipperShared
{
    public class LogUtil
    {
        public const string NativeMessageID = "EARTHRIPPER_LOG_NATIVE:";
        public const string InformationMessageID = "EARTHRIPPER_LOG_INFORMATION:";
        public const string WarningMessageID = "EARTHRIPPER_LOG_WARNING:";
        public const string ErrorMessageID = "EARTHRIPPER_LOG_ERROR:";

        public static string GetSharedLogName(int processID)
        {
            return FormattableString.Invariant($"EARTHRIPPER_LOG_{processID}");
        }
    }
}
