using System.Text;

namespace GodInject.Generator.utils
{
    /// <summary>
    /// Class for help with text formatting
    /// </summary>
    /// <remarks>
    /// It's used when making generation outputs
    /// </remarks>
    public static class TextHelper
    {
        /// <summary>
        /// Appends new line characters (\n) for each of the texts to provide a safer new line formatting.
        /// </summary>
        /// <remarks>
        /// if text inside <paramref name="texts"/> is null it will not be appended.
        /// </remarks>
        /// <param name="texts">Texts to be formatted</param>
        /// <returns>Formatted text with new linex</returns>
        public static string FormatNewLines(params string?[] texts)
        {
            if (texts.Length == 0) return "";
            StringBuilder stringBuilder = new();
            for (int i = 0; i < texts.Length-1; i++)
            {
                if (texts[i] == null) continue;
                stringBuilder.AppendLine(texts[i]);
            }
            stringBuilder.Append(texts[texts.Length - 1]);
            return stringBuilder.ToString();
        }
    }
}
