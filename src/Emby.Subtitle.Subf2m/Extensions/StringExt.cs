﻿namespace Emby.Subtitle.SubF2M.Extensions
{
    public static class StringExt
    {
        /// <summary>
        /// Sub string by start index and end index
        /// </summary>
        /// <param name="value">Original string</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <returns>Sub stringed text</returns>
        public static string SubStr(this string value, int startIndex, int endIndex) =>
            value.Substring(startIndex, (endIndex - startIndex + 1));

        public static string RemoveExtraCharacter(this string text) =>
            text?.Replace("\r\n", "")
                .Replace("\t", "")
                .Trim();

        public static string FixHtml(this string text) =>
            text?.Replace("&nbsp;", " ")
                .Replace("&amp;", "Xamp;")
                .Replace("&", "&amp;")
                .Replace("Xamp;", "&amp;")
                .Replace("--->", "---")
                .Replace("<---", "---")
                .Replace("<--", "--")
                .Replace("<!DOCTYPE html>", "");
    }
}