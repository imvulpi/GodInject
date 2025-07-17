using DryIoc;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.data;
using GodInject.Prebuild.generation;
using GodInject.Prebuild.generation.collectors;
using GodInject.Prebuild.generation.registry;
using GodInject.Prebuild.logger;
using GodInject.Prebuild.mods;
using Microsoft.Build.Locator;
namespace GodInject.Prebuild
{
    public class Program
    {
        const string PROPER_USAGE = "Usage: dotnet GodInject.Prebuild.dll <path-to-project.csproj> (optional: <path-to-execution-settings>)";
        static async Task<int> Main(string[] args)
        {
            try
            {
                ILogger logger = new SimpleLogger();
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                logger.LogInfo("Checking validity of passed arguments...");
                if (args.Length == 0)
                {
                    logger.LogError(PROPER_USAGE);
                    return 1;
                }
                logger.LogInfo("Arguments are valid");

                logger.LogInfo("Starts collection of execution settings");
                IDataCoupler<ExecutionSettings> execSettingsCoupler = new TomlDataCoupler<ExecutionSettings>(GetExecutionSettingsPath(args));
                ExecutionSettings executionSettings = await GetExecutionSettings(execSettingsCoupler);
                ExecutionPaths generationPaths = new(args[0], executionSettings);
                logger.LogInfo("Ends successful collection of base settings");
                logger.LogInfo("Switches logger type to a file logger");
                logger = new GenerationLogger(generationPaths.GeneratorFilesDirPath);
                logger.LogInfo("Program successfully runs, collection of requirements performed");
                logger.LogInfo("Successful logger switching.");
                logger.LogInfo("Starts collection and creation of structures");
                IDataCoupler<GenerationInfo> generationInfoCoupler = new JsonDataCoupler<GenerationInfo>(generationPaths.GenerationInfoPath);
                GenerationInfo? generationInfo = await generationInfoCoupler.ReadAsync();

                IDataCoupler<StructuresInfo> structuresInfoCoupler = new MemPackDataCoupler<StructuresInfo>(Path.Join(generationPaths.GeneratorFilesDirPath, "structures.bin"));
                StructuresInfo? structuresInfo = await structuresInfoCoupler.ReadAsync();
                structuresInfo ??= new StructuresInfo();
                Console.WriteLine($"{(structuresInfo == null ? "Its null" : "Not null")}");

                string[] absoluteDllPaths = GetAbsolutePaths(generationPaths.ProjectDirPath, executionSettings.ReferencesRelativePaths);

                GeneratorRegistry generatorRegistry = new();
                MissingSymbolRegistry missingSymbolRegistry = new();
                DocumentInfoRegistry documentInfoRegistry = new DocumentInfoRegistry();
                StructureInfoValidator infoValidator = new(structuresInfo);
                DependencyCollector dependencyCollector = new DependencyCollector(generationInfo, absoluteDllPaths, generationPaths.GenerationOutputDirPath, documentInfoRegistry);

                logger.LogInfo("Ends successful collection and creation of structures");
                logger.LogInfo("Starts registration of structures in a container");
                MSBuildLocator.RegisterDefaults();
                Dependencies.Container.RegisterInstance(logger);
                Dependencies.Container.RegisterInstance(executionSettings);
                Dependencies.Container.RegisterInstance(execSettingsCoupler);
                Dependencies.Container.RegisterInstance(generationInfoCoupler);
                Dependencies.Container.RegisterInstance(structuresInfoCoupler);
                Dependencies.Container.RegisterInstance(structuresInfo);
                Dependencies.Container.RegisterInstance(infoValidator);
                Dependencies.Container.RegisterInstance(generationPaths);
                Dependencies.Container.RegisterInstance(generationInfo);
                if (generationInfo != null)
                    Dependencies.Container.RegisterInstance<GenerationInfo>(generationInfo);
                Dependencies.Container.RegisterInstance<IInternalGeneratorRegistry>(generatorRegistry);
                Dependencies.Container.RegisterInstance<IGeneratorRegistry>(generatorRegistry);
                Dependencies.Container.RegisterInstance<IInternalCsFileRegistry>(documentInfoRegistry);
                Dependencies.Container.RegisterInstance<ICsFileRegistry>(documentInfoRegistry);
                Dependencies.Container.RegisterInstance<IDependencyCollector>(dependencyCollector);
                Dependencies.Container.RegisterInstance<IDependencyResolver>(new DependencyResolver(structuresInfo));
                Dependencies.Container.RegisterInstance<IInternalMissingSymbolsRegistry>(missingSymbolRegistry);
                Dependencies.Container.RegisterInstance<IMissingSymbolsRegistry>(missingSymbolRegistry);
                Dependencies.Container.Register<MainRunner>();
                logger.LogInfo("End sucessful registration of structures in a container");

                logger.LogInfo($"Starts to validate structures.bin");
                infoValidator.Validate();
                logger.LogInfo($"Sends validation of structures.bin");

                logger.LogInfo("Starts the file processor loop");
                Action<string, FileMetaRef>[] fileActions = 
                {
                    dependencyCollector.CollectDocument,
                    infoValidator.ProcessFile
                };
                ProjectFileRecursor projectFileRecursor = new ProjectFileRecursor(fileActions);
                projectFileRecursor.RecurseFiles(generationPaths.ProjectDirPath, [generationPaths.GenerationOutputDirPath]);
                logger.LogInfo("Ends the file processor loop");
                Console.WriteLine($"{documentInfoRegistry.GetDocuments().Length} counts.");

                logger.LogInfo("Starts collection and loading of mods");
                ModsLoader modsLoader = new(generationPaths.ModsDirPath);
                modsLoader.LoadAll();
                logger.LogInfo("Ends successful collection and loading of mods");
                logger.LogInfo("Starts main runner loop");
                var mainRunner = Dependencies.Container.Resolve<MainRunner>();
                mainRunner.Run();
                logger.LogInfo("Ends successful main runner loop run");
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"UNHANDLED EXCEPTION\n{e.ExceptionObject.ToString()}");
        }

        private static string[] GetAbsolutePaths(string basePath, string[] paths)
        {
            string[] absoluteDllPaths = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string dllPath = paths[i];
                absoluteDllPaths[i] = Path.GetFullPath(Path.Join(basePath, dllPath));
            }
            return absoluteDllPaths;
        }

        private static async Task<ExecutionSettings> GetExecutionSettings(IDataCoupler<ExecutionSettings> execSettingsCoupler)
        {
            ExecutionSettings? executionSettings = await execSettingsCoupler.ReadAsync();
            if (executionSettings == null)
            {
                executionSettings = new();
                await execSettingsCoupler.SaveAsync(executionSettings);
            }
            return executionSettings;
        }

        private static string GetExecutionSettingsPath(string[] args)
        {
            string path = "";
            string projectCsprojPath = args[0];
            string? projectDirPath = Path.GetDirectoryName(projectCsprojPath);

            bool argumentsContainExecSettingsPath = args.Length == 2;
            if (argumentsContainExecSettingsPath)
            {
                string execSettingsPath = args[1];

                if (Path.IsPathRooted(execSettingsPath))
                {
                    path = execSettingsPath;
                }
                else if (projectDirPath != null)
                {
                    string combinedPaths = Path.Combine(projectDirPath, execSettingsPath);
                    path = Path.GetFullPath(combinedPaths);
                }
                else
                {
                    throw new ApplicationException($"Failed because the program could not figure out the project directory path\n{PROPER_USAGE}");
                }
            }
            else
            {
                path = Path.Join(projectDirPath, ExecutionSettings.GENERATOR_FILES_DEFAULT, "exec.conf"); // consts later
            }

            string? directoryPath = Path.GetDirectoryName(path);
            if (directoryPath != null) { 
                Directory.CreateDirectory(directoryPath);
                return path;
            }
            else
            {
                throw new ApplicationException("Could not get directory path of the exec.conf");
            }
        }
    }
}
