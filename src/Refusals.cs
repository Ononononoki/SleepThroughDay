using System.Collections.Generic;

namespace SleepThroughDay
{
    // joke refusals: the day bed at night, and normal beds in the morning
    internal static class Refusals
    {
        internal static readonly JokeDeck DayBed = new JokeDeck("msg_sleepthroughday_night", new[]
        {
            "The bed refuses. It has seen what you do at night.",
            "Hugin landed on the pillow. He is not moving.",
            "A Greydwarf is already in there. He paid rent.",
            "You lie down. The bed stands up and walks away.",
            "Night naps cost extra. Haldor wants 999 coins.",
            "The pillow has filed a complaint with Odin.",
            "The Lox pelt blanket went out grazing.",
            "The Bear Hide wants its bear back.",
            "Closed. Opening hours are sunrise to sunset.",
            "The moon called dibs.",
            "Too dark. You cannot tell which end is the pillow.",
            "The bed is busy dreaming about the sun.",
            "Error 404. Sun not found.",
            "A Fuling is reading the bed a bedtime story. Please wait.",
            "Night naps are for Trolls only. You are not tall enough.",
            "The blanket went to fight the Queen. Back by morning.",
            "The bed whispers. Go to your real bed.",
            "Deathsquitos are holding a meeting in the sheets.",
            "The bed pretends it does not know you.",
            "Hildir borrowed the pillow. No return date given.",
            "It is already night. Skipping to night again would break the sky.",
            "The Bog Witch cursed this bed. It only works in sunlight.",
            "The bed is on strike until sunrise.",
            "You try to nap. The bed bites.",
            "A Neck is guarding the bed. It looks very serious.",
            "A day bed at night? Bold. Wrong, but bold.",
            "Even Eikthyr does not nap at night. He just stares.",
            "Bonemass booked this bed for the night shift.",
            "Your nap is in another biome.",
            "Yagluth is using the bed to grumble. Try tomorrow."
        });

        internal static readonly JokeDeck NormalBed = new JokeDeck("msg_sleepthroughday_day", new[]
        {
            "You are not tired. You just want to skip chores.",
            "The sun is out. The bed is on its lunch break.",
            "Odin did not bring you to Valheim to nap at breakfast.",
            "The bed checks the sky. Too bright. Request denied.",
            "Sleeping now? The Greydwarfs will tell everyone.",
            "This bed only works in the dark. It is scared of the sun.",
            "The pillow is still warm from last night. Let it rest.",
            "Haldor sells many things. Daytime naps are not one of them.",
            "It is daytime. Even the Draugr are awake.",
            "Munin saw that. He will tell Odin.",
            "The bed says come back after sunset. It has standards.",
            "You cannot sleep with the sun staring at you like that.",
            "Too early. The mead has not even kicked in.",
            "The Trolls are working. You can work too.",
            "Your bed went outside to enjoy the weather.",
            "The bed rolls over and ignores you.",
            "Sleeping in the daytime is how Eikthyr got so lazy.",
            "The chickens would never allow this.",
            "No sleep for you. The Elder is watching from his stump.",
            "The bed has decided you have not earned it yet.",
            "The sun is up. So are you. That is the rule.",
            "Daytime naps are a Plains thing. Ask a Fuling.",
            "Sleep now and Bonemass wins.",
            "A Viking does not sleep before noon. Probably.",
            "The bed is not open yet. Please take a number.",
            "The blanket is drying in the sun.",
            "Your bed is still recovering from last night.",
            "The bed looked at your stamina bar and laughed.",
            "You lie down. The sun pokes you in the eye.",
            "Nap request sent to Odin. He said no.",
            "The Bog Witch says beauty sleep starts after dark.",
            "Somewhere a Greydwarf is doing your chores. Just kidding. Go.",
            "The bed is busy. It is sunbathing.",
            "The bed refuses to sleep while the sun is watching.",
            "Wrong bed. Daytime naps need a Day bed.",
            "You count sheep. There are no sheep in Valheim.",
            "Too bright. Even your eyelids refuse.",
            "Hugin says sleeping at noon is not very Viking of you.",
            "The bed is a night owl. You are not.",
            "Get up. The Boars need feeding.",
            "Your crops are watching you. They look disappointed.",
            "The bed checked the time. Then it checked you. Then it said no.",
            "The sun is a deadline. Your bed respects deadlines.",
            "Yagluth slept in once. Look how that turned out.",
            "The bed only accepts tired Vikings. You look fine.",
            "Daylight detected. Bed powers disabled.",
            "Moder is flying around up there. Do you really want to nap now?",
            "Your portal is still glowing. So are you. Go explore.",
            "Sleeping now would make the night jealous.",
            "The bed is closed for daily maintenance. Try again tonight."
        });

        internal static readonly JokeDeck[] All = { DayBed, NormalBed };
    }

    // lines drawn from a shuffled deck, so every line shows before any repeats and never twice in a row
    internal sealed class JokeDeck
    {
        internal readonly string[] Lines;
        private readonly string _prefix;
        private readonly List<int> _deck = new List<int>();
        private readonly System.Random _rng = new System.Random();
        private int _last = -1;

        internal JokeDeck(string prefix, string[] lines)
        {
            _prefix = prefix;
            Lines = lines;
        }

        internal string Token(int index) => $"{_prefix}_{index + 1:00}";

        // localization token of the next line, with the $ the game expects
        internal string Next() => "$" + Token(Draw());

        internal int Draw()
        {
            if (_deck.Count == 0) Refill();
            _last = _deck[_deck.Count - 1];
            _deck.RemoveAt(_deck.Count - 1);
            return _last;
        }

        private void Refill()
        {
            for (int i = 0; i < Lines.Length; i++) _deck.Add(i);
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
            // the last card is drawn first, so it must not repeat the line just shown
            if (_deck[_deck.Count - 1] == _last) (_deck[0], _deck[_deck.Count - 1]) = (_deck[_deck.Count - 1], _deck[0]);
        }
    }
}
