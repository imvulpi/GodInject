using System;
using System.Collections.Generic;
using System.Text;

namespace GodInject.Prebuild.generators.builder
{
    public class TextIndentationBuilder
    {
        public string IndentTextLikeCSharp(string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string newText = "";
            int currentIntentation = 0;
            foreach (string line in lines)
            {
                if (line.Trim().EndsWith("}"))
                {
                    currentIntentation--;
                }
                
                string indentation = new string('\t', currentIntentation);
                newText += indentation + line + "\n";
                
                if (line.Trim().EndsWith("{"))
                {
                    currentIntentation++;
                }
            }
            return newText;
        }
    }
}
