using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.modding;
using System.Reflection;

namespace GodInject.Prebuild.mods
{
    internal class ModsLoader
    {
        public ModsLoader(string modsPath, FrameworkContext frameworkContext) {
            ModsPath = modsPath;
            FrameworkContext = frameworkContext;
        }
        public string ModsPath { get; set; }
        private ModsCollector ModsCollector { get; set; } = new ModsCollector();
        private FrameworkContext FrameworkContext { get; set; }
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
                        modEntry.Initialize(FrameworkContext);
                    }
                }
            }
        }
    }
}
