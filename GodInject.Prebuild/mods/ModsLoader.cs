using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.API.modding;
using System.Reflection;

namespace GodInject.Prebuild.mods
{
    /// <summary>
    /// Processes assemblies and loads valid mods in <paramref name="modsPath"/>
    /// </summary>
    /// <param name="logger">Logger to be used for logging </param>
    /// <param name="modsPath">Directory path where mods should be in</param>
    /// <param name="frameworkContext">Framework context that will be used in initialization of the mods.</param>
    internal class ModsLoader(ILogger logger, string modsPath, FrameworkContext frameworkContext)
    {
        public string ModsPath { get; set; } = modsPath;
        private ModsCollector ModsCollector { get; set; } = new ModsCollector();

        /// <summary>
        /// Initializes collected mods from <see cref="ModsPath"/>
        /// </summary>
        public void LoadAll()
        {
            logger.LogInfo("Loading mods");
            Assembly[] mods = ModsCollector.CollectMods(ModsPath);
            foreach (Assembly asm in mods)
            {
                ProcessAssembly(asm);
            }
            logger.LogInfo("Ends the loading of mods");
        }

        /// <summary>
        /// Proceses the collected .dll assemblies to find valid mod entries to then initialize the mods with <see cref="FrameworkContext"/>
        /// </summary>
        /// <param name="asm">Assembly of possible mods</param>
        private void ProcessAssembly(Assembly asm)
        {
            var types = asm.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                foreach (var item in type.CustomAttributes)
                {
                    if (!item.AttributeType.IsEquivalentTo(typeof(ModEntryAttribute)))
                        continue;

                    var modClass = Activator.CreateInstance(type);
                    if (modClass != null && modClass is IModEntry modEntry)
                    {
                        modEntry.Initialize(frameworkContext);
                    }
                }
            }
        }
    }
}
