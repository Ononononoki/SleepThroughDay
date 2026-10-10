using System;
using HarmonyLib;
using UnityEngine;

namespace SleepThroughDay
{
    // bed steps shared by the day bed and the extra options for normal beds
    internal static class BedActions
    {
        // the game's own bed checks, so the Beds settings apply to every bed
        private static readonly Func<Bed, Player, bool> CheckEnemies = Check("CheckEnemies");
        private static readonly Func<Bed, Player, bool> CheckExposure = Check("CheckExposure");
        private static readonly Func<Bed, Player, bool> CheckFire = Check("CheckFire");
        private static readonly Func<Bed, Player, bool> CheckWet = Check("CheckWet");
        private static readonly Func<Bed, long> GetOwner = AccessTools.MethodDelegate<Func<Bed, long>>(AccessTools.Method(typeof(Bed), "GetOwner"));
        private static readonly Func<Bed, bool> IsMine = AccessTools.MethodDelegate<Func<Bed, bool>>(AccessTools.Method(typeof(Bed), "IsMine"));

        private static Func<Bed, Player, bool> Check(string name) =>
            AccessTools.MethodDelegate<Func<Bed, Player, bool>>(AccessTools.Method(typeof(Bed), name, new[] { typeof(Player) }));

        // enemies, roof, fire and wet, in the game's order. each check shows its own message when it fails
        internal static bool PassesChecks(Bed bed, Player player) =>
            CheckEnemies(bed, player) && CheckExposure(bed, player) && CheckFire(bed, player) && CheckWet(bed, player);

        // lies down the way the vanilla bed does, without claiming the bed or moving the spawn point
        internal static void LieDown(Bed bed, Player player) =>
            player.AttachStart(bed.m_spawnPoint, bed.gameObject, true, true, false, "attach_bed", new Vector3(0f, 0.5f, 0f), null);

        internal static bool OwnedByOther(Bed bed) => GetOwner(bed) != 0 && !IsMine(bed);
    }
}
