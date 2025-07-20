using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;
using GodInject.Prebuild.contexts;
using GodInject.Prebuild.generation;
using GodInject.Prebuild.logger;
using GodInject.Prebuild.mods;
using GodInject.Prebuild.validation;
namespace GodInject.Prebuild
{
    public class Program
    {
        private static ILogger? _globalLogger;
        static async Task<int> Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += GlobalErrorHandler;
            try
            {
                ILogger logger = new SimpleLogger();
                _globalLogger = logger;
                IArgumentValidator argumentValidator = new ArgumentValidator(logger);
                if (!argumentValidator.Validate(args))
                    return (int)ProgramErrors.InvalidArgs;

                RuntimeContext runtimeContext = await new RuntimeContextBuilder(logger).BuildAsync(args);
                logger = runtimeContext.Logger;
                _globalLogger = runtimeContext.Logger;
                GenerationContext generationContext = await new GenerationContextBuilder(logger).BuildAsync(runtimeContext);
                FrameworkContext frameworkContext = new(runtimeContext, generationContext);
                ModsLoader modsLoader = new(runtimeContext.ExecutionPaths.ModsDirPath, frameworkContext);
                MainRunner mainRunner = new(frameworkContext);

                modsLoader.LoadAll();
                var files = FastFileRetriever.GetFilesRecursive(frameworkContext.runtimeContext.ExecutionPaths.ProjectDirPath, "*", [frameworkContext.runtimeContext.ExecutionPaths.GenerationOutputDirPath]);

                frameworkContext.generationContext.GenerationTools.StructureInfoValidator.ProcessStructureInfo(files);
                foreach (var (path, metadata) in files)
                {
                    frameworkContext.generationContext.GenerationTools.DependencyCollector.CollectDocument(path, metadata);
                    frameworkContext.generationContext.GenerationTools.StructureInfoValidator.ValidateFile(path, metadata);
                }

                await mainRunner.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _globalLogger?.LogError($"[FATAL] ");
            }
            return 0;
        }

        private static void GlobalErrorHandler(object sender, UnhandledExceptionEventArgs exceptionEventArgs)
        {
            if (exceptionEventArgs.ExceptionObject is Exception ex)
            {
                string errorMessage =
                    $"[FATAL] Unhandled exception of type {ex.GetType().FullName} occurred.\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace:\n{ex.StackTrace}";

                Console.WriteLine(errorMessage);
                _globalLogger?.LogError("[FATAL] Unhandled exception caught", ex);
            }
            else
            {
                string errorMessage = $"[FATAL] Unhandled exception: {exceptionEventArgs.ExceptionObject}";
                Console.WriteLine(errorMessage);
                _globalLogger?.LogError(errorMessage);
            }
        }

    }
}
