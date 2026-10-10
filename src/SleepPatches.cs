using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SleepThroughDay
{
    // the "in a day bed" flag on each player, the server side skip to night, and the Beds options for normal beds
    public static class SleepPatches
    {
        // player data key for a player lying in a day bed
        internal static readonly int InDayBedKey = "onononoki.SleepThroughDay.inDayBed".GetStableHashCode();
        private const double SleepPause = 10.0; // the game waits this long after a sleep before the next one
        private const double FadeSeconds = 12.0; // a time skip takes this long, same as the skip to morning

        private static readonly AccessTools.FieldRef<Game, bool> GameSleeping = AccessTools.FieldRefAccess<Game, bool>("m_sleeping");
        private static readonly AccessTools.FieldRef<Game, double> LastSleepTime = AccessTools.FieldRefAccess<Game, double>("m_lastSleepTime");
        private static readonly AccessTools.FieldRef<EnvMan, bool> SkipTime = AccessTools.FieldRefAccess<EnvMan, bool>("m_skipTime");
        private static readonly AccessTools.FieldRef<EnvMan, double> SkipToTime = AccessTools.FieldRefAccess<EnvMan, double>("m_skipToTime");
        private static readonly AccessTools.FieldRef<EnvMan, double> TimeSkipSpeed = AccessTools.FieldRefAccess<EnvMan, double>("m_timeSkipSpeed");

        internal static void SetInDayBed(Player player, bool value)
        {
            var nview = player ? player.GetComponent<ZNetView>() : null;
            if (nview && nview.IsValid() && nview.IsOwner()) nview.GetZDO().Set(InDayBedKey, value);
        }

        internal static bool InDayBed(Player player)
        {
            var nview = player ? player.GetComponent<ZNetView>() : null;
            return nview && nview.IsValid() && nview.GetZDO().GetBool(InDayBedKey, false);
        }

        private static void StartSkipToNight(EnvMan env, double now, double target)
        {
            SkipTime(env) = true;
            SkipToTime(env) = target;
            TimeSkipSpeed(env) = (target - now) / FadeSeconds;
            Plugin.Log.LogInfo($"everyone is napping, skipping to nightfall ({now:0} to {target:0})");
        }

        private static bool EverybodyInDayBed()
        {
            var players = ZNet.instance.GetAllCharacterZDOS();
            if (players.Count == 0) return false;
            foreach (var zdo in players)
                if (!zdo.GetBool(ZDOVars.s_inBed, false) || !zdo.GetBool(InDayBedKey, false)) return false;
            return true;
        }

        private static bool AnyInDayBed()
        {
            if (!ZNet.instance) return false;
            foreach (var zdo in ZNet.instance.GetAllCharacterZDOS())
                if (zdo.GetBool(InDayBedKey, false)) return true;
            return false;
        }

        // anyone in a day bed blocks the normal skip to morning, from the game or from other sleep mods
        [HarmonyPatch(typeof(Game), "EverybodyIsTryingToSleep")]
        private static class BlockMorningSkip
        {
            private static void Postfix(ref bool __result)
            {
                if (__result && AnyInDayBed()) __result = false;
            }
        }

        // on the server, skip to nightfall once every player online naps in a day bed
        [HarmonyPatch(typeof(Game), "UpdateSleeping")]
        private static class SkipToNight
        {
            private static void Postfix(Game __instance)
            {
                if (!Plugin.Enabled.Value || !ZNet.instance || !ZNet.instance.IsServer()) return;
                var env = EnvMan.instance;
                if (!env || GameSleeping(__instance) || env.IsTimeSkipping() || !DayTime.IsNapTime()) return;
                double now = ZNet.instance.GetTimeSeconds();
                if (now - LastSleepTime(__instance) < SleepPause || !EverybodyInDayBed()) return;

                double target = DayTime.NightTarget(now, env.m_dayLengthSec, Plugin.NightStart.Value);
                if (target - now < 1.0) return; // never skip backwards
                StartSkipToNight(env, now, target);
                GameSleeping(__instance) = true;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SleepStart");
            }
        }

        // the flag goes away when the player leaves the bed, also when waking up
        [HarmonyPatch(typeof(Player), nameof(Player.AttachStop))]
        private static class ClearFlag
        {
            private static void Postfix(Player __instance)
            {
                var nview = __instance.GetComponent<ZNetView>();
                if (!nview || !nview.IsValid() || !nview.IsOwner()) return;
                var zdo = nview.GetZDO();
                if (zdo.GetBool(InDayBedKey, false) && !zdo.GetBool(ZDOVars.s_inBed, false)) zdo.Set(InDayBedKey, false);
            }
        }

        // waking from a nap says good evening instead of good morning
        [HarmonyPatch(typeof(Player), nameof(Player.Message))]
        private static class EveningMessage
        {
            private static void Prefix(Player __instance, ref string msg)
            {
                if (msg == "$msg_goodmorning" && __instance == Player.m_localPlayer && InDayBed(__instance))
                    msg = "$msg_sleepthroughday_goodevening";
            }
        }

        // a normal bed refusing in the morning tells a joke. a refusal right after waking at night keeps the plain text
        [HarmonyPatch(typeof(Player), nameof(Player.Message))]
        private static class MorningJoke
        {
            private static void Prefix(Player __instance, ref string msg)
            {
                if (msg == "$msg_cantsleep" && Plugin.FunnyRefusals.Value && __instance == Player.m_localPlayer && DayTime.IsMorning())
                    msg = Refusals.NormalBed.Next();
            }
        }

        // the Beds settings skip the game's enemy, roof, fire and wet checks, for every bed
        [HarmonyPatch]
        private static class SkipBedChecks
        {
            private static IEnumerable<MethodBase> TargetMethods() =>
                new[] { "CheckEnemies", "CheckExposure", "CheckFire", "CheckWet" }.Select(name => (MethodBase)AccessTools.Method(typeof(Bed), name));

            private static bool Prefix(MethodBase __originalMethod, ref bool __result)
            {
                if (!Plugin.SkipsCheck(__originalMethod.Name)) return true;
                __result = true;
                return false;
            }
        }

        // SleepWithoutSpawn (alt and use) or SleepInOthersBeds: sleep in a normal bed without claiming it or moving the spawn point
        [HarmonyPatch(typeof(Bed), nameof(Bed.Interact))]
        private static class SleepWithoutClaim
        {
            private static bool Prefix(Bed __instance, Humanoid human, bool repeat, bool alt, ref bool __result)
            {
                if (repeat || !(human is Player player)) return true;
                bool viaAlt = alt && Plugin.SleepWithoutSpawn.Value;
                bool othersBed = Plugin.SleepInOthersBeds.Value && BedActions.OwnedByOther(__instance);
                if (!viaAlt && !othersBed) return true;

                __result = false;
                if (!EnvMan.CanSleep())
                {
                    player.Message(MessageHud.MessageType.Center, "$msg_cantsleep");
                    return false;
                }
                if (BedActions.PassesChecks(__instance, player)) BedActions.LieDown(__instance, player);
                return false;
            }
        }

        // hover hints for those two options
        [HarmonyPatch(typeof(Bed), nameof(Bed.GetHoverText))]
        private static class BedHints
        {
            private static void Postfix(Bed __instance, ref string __result)
            {
                if (Plugin.SleepInOthersBeds.Value && BedActions.OwnedByOther(__instance))
                    __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_sleepthroughday_sleephere");
                else if (Plugin.SleepWithoutSpawn.Value)
                    __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $piece_sleepthroughday_sleephere");
            }
        }
    }
}
