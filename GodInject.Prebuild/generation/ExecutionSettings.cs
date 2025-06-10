using Tomlet.Attributes;

namespace GodInject.Prebuild.generation
{
    public enum MissingDependenciesActions
    {
        Throw,
        InjectInConstructor,
        InjectLikeGodot,
    }
    /// <summary>
    /// Defines settings for all execution in this program
    /// </summary>
    public class ExecutionSettings
    {
        [TomlPrecedingComment("Relative path from .csproj directory to the generator files. Put a path you would want generator files to be in.")]
        public string RelativeGeneratorPath { get; set; } = "./generator";

        [TomlPrecedingComment("Name of the directory inside the generator path where the generated files will be stored in")]
        public string OutputDirName { get; set; } = "generated";

        [TomlPrecedingComment("Should the manual InjectAll() function be available by default without ManualInjection tag")]
        public bool CreateManualInjectMethod { get; set; } = false;

        [TomlPrecedingComment("Should the program scan for Inject attributes in about to be processed files in order to disregard them or now. " +
            "It can speedup the processing if majority of files don't contain Injections.")]
        public bool InfileInjectSearch { get; set; } = false;

        [TomlPrecedingComment("What action should be done if the search for missing dependencies fails, ex. How should the injection be constructed?")]
        public MissingDependenciesActions MissingDependenciesActions { get; set; } = MissingDependenciesActions.InjectInConstructor;
        public string[] DllPaths { get; set; } = Array.Empty<string>();

        public override string ToString()
        {
            return $"{nameof(OutputDirName)}: {OutputDirName}\n" +
                   $"{nameof(CreateManualInjectMethod)}: {CreateManualInjectMethod}";
        }
    }
}
