namespace SleepThroughDay
{
    // the day bed. it lists the interfaces again, so the game calls these methods instead of Bed's,
    // which other bed mods patch. no owner, no spawn point, naps only during the day.
    public class DayBed : Bed, Interactable, Hoverable
    {
        public new bool Interact(Humanoid human, bool repeat, bool alt)
        {
            if (repeat || !(human is Player player)) return false;
            if (!Plugin.Enabled.Value)
            {
                player.Message(MessageHud.MessageType.Center, "$msg_sleepthroughday_off");
                return false;
            }
            if (!DayTime.IsNapTime())
            {
                player.Message(MessageHud.MessageType.Center, Plugin.FunnyRefusals.Value ? Refusals.DayBed.Next() : "$msg_sleepthroughday_night");
                return false;
            }
            if (!BedActions.PassesChecks(this, player)) return false;

            BedActions.LieDown(this, player);
            SleepPatches.SetInDayBed(player, true);
            return false;
        }

        public new string GetHoverText()
        {
            string action = !Plugin.Enabled.Value ? "$msg_sleepthroughday_off"
                : DayTime.IsNapTime() ? "[<color=yellow><b>$KEY_Use</b></color>] $piece_sleepthroughday_nap"
                : "$piece_sleepthroughday_dayonly";
            return Localization.instance.Localize("$piece_sleepthroughday_daybed\n" + action);
        }

        public new string GetHoverName() => Localization.instance.Localize("$piece_sleepthroughday_daybed");
    }
}
