# SleepThroughDay

Adds a Day bed. When everyone naps in one during the day, time skips to nightfall. It refuses at night with jokes. Optional rules for all beds, like sleeping with enemies nearby. Server synced.

A Valheim mod by Onononoki. Get it on Thunderstore: https://thunderstore.io/c/valheim/p/Onononoki/SleepThroughDay/

The full description, settings and changelog are in [package/README.md](package/README.md) and [package/CHANGELOG.md](package/CHANGELOG.md).

## Bug reports and feedback

Bug reports and ideas: open an issue at https://github.com/Ononononoki/SleepThroughDay/issues. If something breaks, please add your mod list and the BepInEx log (`BepInEx/LogOutput.log`).

## Building

Needs the .NET SDK, Valheim and a mod manager profile with BepInEx and Jotunn.

```
dotnet build -c Release -p:GamePath="<Valheim folder>" -p:ProfilePath="<mod manager profile folder>"
```

The dll is built into `bin/Release/net472` and copied to `package/`.

## License

MIT, see [LICENSE](LICENSE).
