using GodInject.Prebuild.API.IO;

namespace GodInject.Prebuild.generation
{
    /// <summary>
    /// Recurses around all project files to perform multiple actions at the same time without needing multiple calls
    /// </summary>
    public class ProjectFileRecursor(params Action<string, FileMetaRef>[] fileActions)
    {
        public Action<string, FileMetaRef>[] FileActions { get; set; } = fileActions;

        public void RecurseFiles(string path, string[] ignoreDirs)
        {
            var files = FastFileRetriever.GetFilesRecursive(path, "*", ignoreDirs);
            foreach (var file in files)
            {
                foreach (var action in FileActions)
                {
                    action?.Invoke(file.path, file.metadata);
                }
            }
        }
    }
}
