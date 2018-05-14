using System;

namespace YuGiOhCardDatabaseBuilder
{
    public static class Extensions
    {
        public static string ToOutput(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2},{timeSpan.Milliseconds:D3}";
        }
    }
}
