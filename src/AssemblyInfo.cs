using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("SleepThroughDay")]
[assembly: AssemblyDescription("Adds a day bed to Valheim. Napping in it during the day skips time to nightfall")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

// lets the offline check in test\SleepCheck call the internal helpers
[assembly: InternalsVisibleTo("SleepCheck")]
