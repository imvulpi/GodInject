using System.Text;

namespace GodInject.Generator.utils
{
    public static class TextHelper
    {
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
