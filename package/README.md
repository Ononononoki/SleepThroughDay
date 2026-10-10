# SleepThroughDay

**Want the night to come sooner? Build a Day bed, nap in it, and wake up at nightfall.**

Normal beds only work from the afternoon on, and they skip to the next morning. This mod adds a Day bed that does the opposite. Nap in it during the day, and when every player online is napping in one, time skips to nightfall.

## What it does

- Adds the **Day bed** to the Hammer, under Furniture. It needs a Workbench.
- Nap in it from morning until nightfall. When every player online lies in a Day bed, time skips to nightfall with the usual sleep fade.
- You wake up to "Good evening" and get the Rested bonus like after a normal sleep.
- The Day bed cannot be used at night. Use a normal bed then, which still skips to morning. So nobody can sleep the whole game away.
- Try it at night anyway and the bed refuses with one of 30 jokes. Normal beds that refuse in the morning get 50 jokes of their own.
- The Day bed has a darkwood frame and red straw, so it is easy to tell apart.
- It never claims an owner and never sets your spawn point. Anyone can nap in any Day bed.

## Rules in multiplayer

- Everyone online has to nap in a Day bed for the skip to happen, the same way everyone has to sleep for a normal night skip.
- Mixed beds do nothing. While anyone lies in a Day bed, the normal skip to morning waits too.
- The usual bed checks apply to the Day bed too (a roof, a fire nearby, not wet). Enemies nearby are ignored by default. The Beds settings below change these checks for every bed.

## Building cost

5 Bear Hide, 5 Deer Hide, 5 Leather Scraps and 5 Lox Pelt at a Workbench. You can change it in the config.

## Config

The file is `BepInEx/config/onononoki.SleepThroughDay.cfg`. Settings are synced from the server to every player, and only admins can change them in multiplayer.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | true | Day beds can be napped in |
| `NightStart` | 0.75 | Time of day a nap skips to. 0.75 is nightfall, 0.5 is noon. Day beds work from morning until this time |
| `Recipe` | `BjornHide:5,DeerHide:5,LeatherScraps:5,LoxPelt:5` | Building cost as item prefab names and amounts |
| `FunnyRefusals` | true | Jokes when a bed refuses, the Day bed at night and normal beds in the morning. Off shows the plain messages. Your own game only, not synced |

### Beds (all beds, synced)

| Setting | Default | What it does |
|---|---|---|
| `IgnoreEnemies` | true | Sleep with enemies nearby, for passive creatures that hear you through floors and walls |
| `IgnoreRoof` | false | Sleep without a roof |
| `IgnoreFire` | false | Sleep without a fire nearby |
| `IgnoreWet` | false | Sleep while wet |
| `SleepWithoutSpawn` | false | Hold Left Shift while using a bed to sleep without claiming it or moving your spawn point |
| `SleepInOthersBeds` | false | Sleep in beds owned by other players. They stay theirs and your spawn point does not move |

## Installation

- Use a mod manager (r2modman or Thunderstore Mod Manager), or put `SleepThroughDay.dll` into `BepInEx/plugins`.
- For multiplayer, install it on the server and on every player. Players without the mod cannot join a server that has it, and players with the mod cannot join a server without it.
- Requires BepInExPack Valheim and Jotunn.

## Compatibility

- The Day bed has its own code, so other bed mods cannot make it usable at night.
- The Beds settings cover what BedRules does for enemies, roof, fire, wet, spawn point and owned beds. Sleeping at any time is left out on purpose. Running both together is not tested and not needed.
- Nothing is saved to the character. Day beds you built are world objects and need the mod to show up.

## How it works

The Day bed is a copy of the vanilla bed with its own bed component. Lying in it marks the player as napping. The server checks every player each frame, and when all of them nap in a Day bed during the day, it runs the game's own time skip with nightfall as the target.
