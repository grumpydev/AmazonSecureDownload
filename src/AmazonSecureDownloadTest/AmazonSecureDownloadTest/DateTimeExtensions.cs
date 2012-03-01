namespace SecureDownloadTest
{
    using System;

    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a .net DateTime to a unix time value
        /// </summary>
        /// <param name="input">Input date</param>
        /// <returns>Unix time equivilent</returns>
        public static double ToUnixTime(this DateTime input)
        {
            return Math.Floor((input - Epoch).TotalSeconds);
        }
    }
}