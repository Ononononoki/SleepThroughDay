using System;

namespace SleepThroughDay
{
    // the game's clock. EnvMan stretches the day, so raw 0.15 to 0.85 of a day is shown as 0.25 to 0.75
    // (EnvMan.RescaleDayFraction). nap times use the shown fraction, the time skip needs raw game seconds.
    internal static class DayTime
    {
        internal const float MorningEnd = 0.25f; // night ends here, in the shown fraction

        // same mapping as EnvMan.RescaleDayFraction
        internal static float FromRaw(float raw)
        {
            if (raw >= 0.15f && raw <= 0.85f) return 0.25f + (raw - 0.15f) / 0.7f * 0.5f;
            if (raw < 0.5f) return raw / 0.15f * 0.25f;
            return 0.75f + (raw - 0.85f) / 0.15f * 0.25f;
        }

        // the other way round, for shown fractions from 0.25 to 0.75
        internal static double ToRaw(float shown) => 0.15 + (shown - 0.25) / 0.5 * 0.7;

        // shown fraction at a time in game seconds, computed like EnvMan before its smoothing
        internal static float At(double seconds, long dayLength) => FromRaw((float)((long)seconds % dayLength) / dayLength);

        // nightfall of the day that holds "seconds", in game seconds
        internal static double NightTarget(double seconds, long dayLength, float nightStart) =>
            Math.Floor(seconds / dayLength) * dayLength + dayLength * ToRaw(nightStart);

        // shown fraction right now, read from the network time so the console's tod lock does not count. -1 before a world is loaded
        internal static float Now()
        {
            var env = EnvMan.instance;
            if (!env || !ZNet.instance) return -1f;
            return At(ZNet.instance.GetTimeSeconds(), env.m_dayLengthSec);
        }

        // morning until the configured nightfall
        internal static bool IsNapTime()
        {
            float now = Now();
            return now >= MorningEnd && now < Plugin.NightStart.Value;
        }

        // morning until the afternoon, when normal beds still refuse because of the time
        internal static bool IsMorning()
        {
            float now = Now();
            return now >= MorningEnd && now < 0.5f;
        }
    }
}
