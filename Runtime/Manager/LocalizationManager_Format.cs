using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {
        public static string GetTranslation(string term, string arg0)
        {
            string translation = GetTranslation(term);
            if (string.IsNullOrEmpty(translation))
                return string.Empty;

            return string.Format(translation, arg0);
        }

        public static string GetTranslation(string term, string arg0, string arg1)
        {
            string translation = GetTranslation(term);
            if (string.IsNullOrEmpty(translation))
                return string.Empty;

            return string.Format(translation, arg0, arg1);
        }

        public static string GetTranslation(string term, string arg0, string arg1, string arg2)
        {
            string translation = GetTranslation(term);
            if (string.IsNullOrEmpty(translation))
                return string.Empty;

            return string.Format(translation, arg0, arg1, arg2);
        }

        public static string GetTranslation(string term, params object[] args)
        {
            string translation = GetTranslation(term);
            if (string.IsNullOrEmpty(translation))
                return string.Empty;

            return string.Format(translation, args);
        }
    }
}