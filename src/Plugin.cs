using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace SleepThroughDay
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "onononoki.SleepThroughDay";
        public const string ModName = "SleepThroughDay";
        public const string ModVersion = "1.0.1";
        internal const string PrefabName = "SleepThroughDay_DayBed";
        internal const string DefaultRecipe = "BjornHide:5,DeerHide:5,LeatherScraps:5,LoxPelt:5";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> NightStart;
        internal static ConfigEntry<bool> FunnyRefusals;
        internal static ConfigEntry<bool> IgnoreEnemies;
        internal static ConfigEntry<bool> IgnoreRoof;
        internal static ConfigEntry<bool> IgnoreFire;
        internal static ConfigEntry<bool> IgnoreWet;
        internal static ConfigEntry<bool> SleepWithoutSpawn;
        internal static ConfigEntry<bool> SleepInOthersBeds;
        private static ConfigEntry<string> _recipe;
        private static ConfigEntry<bool> _savePreview;

        private static CustomPiece _piece;

        // darkwood stain for the frame and madder red for the straw, which sits well next to dark wood
        private static readonly Color32 DarkwoodTone = new Color32(74, 50, 36, 255);
        private static readonly Color32 StrawTone = new Color32(128, 44, 38, 255);
        private static readonly Dictionary<Material, Material> RecoloredMaterials = new Dictionary<Material, Material>();
        private static readonly Dictionary<Texture, Texture2D> RecoloredTextures = new Dictionary<Texture, Texture2D>();

        private void Awake()
        {
            Log = Logger;
            var sync = new ConfigurationManagerAttributes { IsAdminOnly = true };

            Enabled = Config.Bind("General", "Enabled", true,
                new ConfigDescription("Day beds can be napped in. Off leaves them as furniture that cannot be used.", null, sync));
            NightStart = Config.Bind("General", "NightStart", 0.75f,
                new ConfigDescription("Time of day a nap skips to, as a fraction of the day. 0.75 is nightfall, 0.5 is noon. Day beds work from morning (0.25) until this time.",
                    new AcceptableValueRange<float>(0.5f, 0.75f), sync));
            FunnyRefusals = Config.Bind("General", "FunnyRefusals", true,
                "Show a random joke when a bed refuses, the Day bed at night and normal beds in the morning. Off shows the plain messages. Only affects your own game.");
            _recipe = Config.Bind("General", "Recipe", DefaultRecipe,
                new ConfigDescription("Building cost of the day bed, as item prefab names and amounts separated by commas.", null, sync));

            IgnoreEnemies = Config.Bind("Beds", "IgnoreEnemies", true,
                new ConfigDescription("Every bed can be slept in with enemies nearby, for passive creatures that hear you through floors and walls.", null, sync));
            IgnoreRoof = Config.Bind("Beds", "IgnoreRoof", false,
                new ConfigDescription("Every bed can be slept in without a roof.", null, sync));
            IgnoreFire = Config.Bind("Beds", "IgnoreFire", false,
                new ConfigDescription("Every bed can be slept in without a fire nearby.", null, sync));
            IgnoreWet = Config.Bind("Beds", "IgnoreWet", false,
                new ConfigDescription("Every bed can be slept in while wet.", null, sync));
            SleepWithoutSpawn = Config.Bind("Beds", "SleepWithoutSpawn", false,
                new ConfigDescription("Hold the alt key (Left Shift by default) while using a normal bed to sleep in it without claiming it or moving your spawn point.", null, sync));
            SleepInOthersBeds = Config.Bind("Beds", "SleepInOthersBeds", false,
                new ConfigDescription("Beds owned by other players can be slept in. They stay theirs and your spawn point does not move.", null, sync));
            _savePreview = Config.Bind("Debug", "SavePreview", false,
                "Save a picture of the day bed next to this config and log its material names on start. Only for checking the look.");

            AddLocalization();
            PrefabManager.OnVanillaPrefabsAvailable += AddPiece;
            SynchronizationManager.OnConfigurationSynchronized += (s, e) => ApplyRecipe();
            _recipe.SettingChanged += (s, e) => ApplyRecipe();

            new Harmony(ModGuid).PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"{ModName} {ModVersion} loaded");
        }

        // which of the game's bed checks the Beds settings skip
        internal static bool SkipsCheck(string check)
        {
            switch (check)
            {
                case "CheckEnemies": return IgnoreEnemies.Value;
                case "CheckExposure": return IgnoreRoof.Value;
                case "CheckFire": return IgnoreFire.Value;
                case "CheckWet": return IgnoreWet.Value;
                default: return false;
            }
        }

        private void AddPiece()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddPiece;
            var prefab = PrefabManager.Instance.CreateClonedPrefab(PrefabName, "bed");
            if (!prefab)
            {
                Log.LogError("vanilla bed not found, the day bed is not added");
                return;
            }

            // swap the game's bed component for the day bed, keeping its spawn point and settings
            var bed = prefab.GetComponent<Bed>();
            var dayBed = prefab.AddComponent<DayBed>();
            dayBed.m_spawnPoint = bed.m_spawnPoint;
            dayBed.m_monsterCheckRadius = bed.m_monsterCheckRadius;
            dayBed.m_hoverOffset = bed.m_hoverOffset;
            DestroyImmediate(bed);

            Recolor(prefab);

            var config = new PieceConfig
            {
                Name = "$piece_sleepthroughday_daybed",
                Description = "$piece_sleepthroughday_daybed_desc",
                PieceTable = "Hammer",
                Category = "Furniture",
                CraftingStation = "piece_workbench",
                Requirements = Requirements(_recipe.Value).Select(r => new RequirementConfig(r.Key, r.Value, 0, true)).ToArray()
            };
            var icon = Render(prefab, 128);
            if (icon) config.Icon = icon;

            _piece = new CustomPiece(prefab, false, config);
            PieceManager.Instance.AddPiece(_piece);

            if (_savePreview.Value) SavePreview(prefab);
        }

        // gives the bed's materials (one atlas for wood and straw) a recolored copy
        private static void Recolor(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (!mats[i] || !mats[i].name.StartsWith("BedSimple")) continue;
                    if (!RecoloredMaterials.TryGetValue(mats[i], out var copy))
                    {
                        var texture = RecolorTexture(mats[i].mainTexture);
                        copy = texture ? new Material(mats[i]) { name = mats[i].name + "_daybed", mainTexture = texture } : mats[i];
                        RecoloredMaterials[mats[i]] = copy;
                    }
                    if (copy == mats[i]) continue;
                    mats[i] = copy;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = mats;
            }
        }

        // readable copy of a game texture, wood stained darkwood and straw dyed red
        private static Texture2D RecolorTexture(Texture source)
        {
            if (!source) return null;
            if (RecoloredTextures.TryGetValue(source, out var done)) return done;

            // game textures are not readable, so they are copied through the gpu
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var active = RenderTexture.active;
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, false)
            {
                name = source.name + "_daybed",
                wrapMode = source.wrapMode,
                filterMode = source.filterMode,
                anisoLevel = source.anisoLevel
            };
            copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(rt);
            if (_savePreview.Value) SavePng(copy, $"SleepThroughDay_{source.name}_original.png");

            // average brightness of each area, so every pixel keeps its grain and shading relative to it
            var pixels = copy.GetPixels32();
            int width = copy.width, height = copy.height;
            double woodSum = 0, strawSum = 0;
            int woodCount = 0, strawCount = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < 8) continue;
                if (InStraw(i % width, i / width, width, height)) { strawSum += Luminance(pixels[i]); strawCount++; }
                else { woodSum += Luminance(pixels[i]); woodCount++; }
            }
            float woodMean = (float)(woodSum / System.Math.Max(1, woodCount));
            float strawMean = (float)(strawSum / System.Math.Max(1, strawCount));

            for (int i = 0; i < pixels.Length; i++)
            {
                bool straw = InStraw(i % width, i / width, width, height);
                pixels[i] = Stain(pixels[i], straw ? strawMean : woodMean, straw ? StrawTone : DarkwoodTone);
            }
            copy.SetPixels32(pixels);
            copy.Apply(true);
            if (_savePreview.Value) SavePng(copy, $"SleepThroughDay_{source.name}_recolored.png");

            RecoloredTextures[source] = copy;
            return copy;
        }

        // the bed atlas (128 px, same layout new and worn): straw is the top right patch, everything else is wood.
        // pixel rows count from the bottom, as in GetPixels32
        internal static bool InStraw(int x, int y, int width, int height) => x >= width * 0.375f && y >= height * 0.625f;

        // the pixel in the new tone, as bright relative to the tone as it was relative to its area's average
        internal static Color32 Stain(Color32 pixel, float areaMean, Color32 tone)
        {
            float k = areaMean > 0f ? Luminance(pixel) / areaMean : 1f;
            return new Color32(Channel(tone.r * k), Channel(tone.g * k), Channel(tone.b * k), pixel.a);
        }

        internal static float Luminance(Color32 c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;

        private static byte Channel(float value) => (byte)System.Math.Max(0, System.Math.Min(255, (int)System.Math.Round(value)));

        // isometric picture of the bed, rendered fresh so tint changes always show
        private static Sprite Render(GameObject prefab, int size) =>
            RenderManager.Instance.Render(new RenderManager.RenderRequest(prefab)
            {
                Width = size,
                Height = size,
                Rotation = RenderManager.IsometricRotation,
                UseCache = false
            });

        // writes a large picture of the bed next to the config and logs its parts, for checking the look
        private static void SavePreview(GameObject prefab)
        {
            var parts = prefab.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials.Where(m => m).Select(m => $"{r.name}/{m.name}"));
            Log.LogInfo("day bed parts: " + string.Join(", ", parts));
            var shot = Render(prefab, 512);
            if (shot) SavePng(shot.texture, "SleepThroughDay_preview.png");
        }

        private static void SavePng(Texture2D texture, string file)
        {
            try
            {
                File.WriteAllBytes(Path.Combine(BepInEx.Paths.ConfigPath, file), texture.EncodeToPNG());
            }
            catch (Exception ex)
            {
                Log.LogWarning($"could not save {file}: {ex.Message}");
            }
        }

        // updates the building cost after a config change or a server sync
        private static void ApplyRecipe()
        {
            if (_piece == null || !_piece.Piece || !ObjectDB.instance) return;
            var resources = new List<Piece.Requirement>();
            foreach (var entry in Requirements(_recipe.Value))
            {
                var prefab = ObjectDB.instance.GetItemPrefab(entry.Key);
                var item = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (!item)
                {
                    Log.LogWarning($"unknown item {entry.Key} in Recipe, skipped");
                    continue;
                }
                resources.Add(new Piece.Requirement { m_resItem = item, m_amount = entry.Value, m_recover = true });
            }
            if (resources.Count > 0) _piece.Piece.m_resources = resources.ToArray();
        }

        // item names and amounts from the recipe setting, or the default recipe when nothing valid is left
        internal static List<KeyValuePair<string, int>> Requirements(string value)
        {
            var list = ParseRecipe(value);
            if (list.Count > 0) return list;
            Log?.LogWarning("Recipe has no valid entries, using the default");
            return ParseRecipe(DefaultRecipe);
        }

        internal static List<KeyValuePair<string, int>> ParseRecipe(string value)
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var part in (value ?? "").Split(','))
            {
                var bits = part.Split(':');
                if (bits.Length == 2 && bits[0].Trim().Length > 0 && int.TryParse(bits[1].Trim(), out int amount) && amount > 0)
                    list.Add(new KeyValuePair<string, int>(bits[0].Trim(), amount));
            }
            return list;
        }

        private static void AddLocalization()
        {
            string english = "English";
            var tokens = new Dictionary<string, string>
            {
                ["piece_sleepthroughday_daybed"] = "Day bed",
                ["piece_sleepthroughday_daybed_desc"] = "A bed for daytime naps. When everyone naps in one, time skips to nightfall.",
                ["piece_sleepthroughday_nap"] = "Nap until night",
                ["piece_sleepthroughday_dayonly"] = "Only for naps during the day",
                ["msg_sleepthroughday_night"] = "Too late for a nap. Use a normal bed at night",
                ["msg_sleepthroughday_off"] = "Day beds are turned off",
                ["msg_sleepthroughday_goodevening"] = "Good evening",
                ["piece_sleepthroughday_sleephere"] = "Sleep here (spawn point stays)"
            };
            foreach (var deck in Refusals.All)
                for (int i = 0; i < deck.Lines.Length; i++) tokens[deck.Token(i)] = deck.Lines[i];
            LocalizationManager.Instance.GetLocalization().AddTranslation(english, tokens);
        }
    }
}
