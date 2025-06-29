using GodInject.Prebuild.API.modding;
using System.Reflection;

namespace GodInject.Prebuild.mods
{
    /// <summary>
    /// Loads mods into the projects, looks for <see cref="IModEntry"/> and initializes it with the <see cref="IModContext"/>
    /// The loading will give access to public accesible dependencies in the <see cref="Dependencies"/>,
    /// You should only load after the important dependencies are registered that way they are accesible to the mods.
    /// </summary>
    internal class ModsLoader
    {
        public ModsLoader(string modsPath) {
            ModsPath = modsPath;
        }
        public string ModsPath { get; set; }
        public ModContext ModContext = new(Dependencies.Container);
        private ModsCollector ModsCollector { get; set; } = new ModsCollector();
        public void LoadAll()
        {
            Assembly[] mods = ModsCollector.CollectMods(ModsPath);
            foreach (Assembly asm in mods)
            {
                ProcessAssembly(asm);
            }
        }

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
                        modEntry.Initialize(ModContext);
                    }
                }
            }
        }
    }
}
