# SleepThroughDay

**Want the night to come sooner? Build a Day bed, nap in it, and wake up at nightfall.**

Normal beds work from the afternoon and skip to morning. The Day bed does the opposite. When every player online naps in one during the day, time skips to nightfall.

## What it does

- Adds the **Day bed** (darkwood frame, red straw) to the Hammer under Furniture. It needs a Workbench and costs 5 Bear Hide, 5 Deer Hide, 5 Leather Scraps and 5 Lox Pelt.
- Works from morning until nightfall. You wake up to "Good evening" with the Rested bonus.
- Cannot be used at night, so use a normal bed then. Try anyway and it refuses with one of 30 jokes. Normal beds that refuse in the morning get 50 jokes of their own.
- Never claims an owner or sets your spawn point. Anyone can nap in any Day bed.
- In multiplayer, everyone online has to nap in a Day bed. Mixed beds do nothing, and while anyone lies in a Day bed the normal skip to morning waits too.

## Config

The file is `BepInEx/config/onononoki.SleepThroughDay.cfg`. Settings are synced from the server and only admins can change them in multiplayer. The last six apply to every bed. The usual bed checks (a roof, a fire nearby, not wet) still apply unless you turn them off here.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | true | Day beds can be napped in |
| `NightStart` | 0.75 | Time of day a nap skips to. 0.75 is nightfall, 0.5 is noon |
| `Recipe` | `BjornHide:5,DeerHide:5,LeatherScraps:5,LoxPelt:5` | Building cost as item prefab names and amounts |
| `FunnyRefusals` | true | Jokes when a bed refuses. Off shows plain messages. Your own game only, not synced |
| `IgnoreEnemies` | true | Sleep with enemies nearby |
| `IgnoreRoof` | false | Sleep without a roof |
| `IgnoreFire` | false | Sleep without a fire nearby |
| `IgnoreWet` | false | Sleep while wet |
| `SleepWithoutSpawn` | false | Hold Left Shift while using a bed to sleep without claiming it or moving your spawn point |
| `SleepInOthersBeds` | false | Sleep in beds owned by other players. They stay theirs and your spawn point does not move |

## Installation and compatibility

- Use a mod manager, or put `SleepThroughDay.dll` into `BepInEx/plugins`. Requires BepInExPack Valheim and Jotunn.
- For multiplayer, install it on the server and on every player. Players without the mod cannot join a server that has it, and the other way around.
- Other bed mods cannot make the Day bed usable at night.
- The bed settings cover what BedRules does for enemies, roof, fire, wet, spawn point and owned beds. Sleeping at any time is left out on purpose. Running both together is not tested and not needed.
- Nothing is saved to the character. Day beds you built need the mod to show up.

## Feedback and bug reports

Found a bug or have an idea? Open an issue on [GitHub](https://github.com/Ononononoki/SleepThroughDay/issues). The source code is in the [GitHub repository](https://github.com/Ononononoki/SleepThroughDay).
