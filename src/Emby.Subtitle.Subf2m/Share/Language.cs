using MediaBrowser.Model.Globalization;
using System;

namespace Emby.Subtitle.SubF2M.Share
{
    public class Language
    {
        private readonly ILocalizationManager _localizationManager;

        public Language(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        public string Map(string lang)
        {
            switch (lang.ToLower())
            {
                case "per":
                    lang = "farsi_persian";
                    break;
                case "ara":
                    lang = "arabic";
                    break;
                case "eng":
                    lang = "english";
                    break;
                case "bur":
                    lang = "burmese";
                    break;
                case "dan":
                    lang = "danish";
                    break;
                case "dut":
                    lang = "dutch";
                    break;
                case "heb":
                    lang = "hebrew";
                    break;
                case "ind":
                    lang = "indonesian";
                    break;
                case "kor":
                    lang = "korean";
                    break;
                case "may":
                    lang = "malay";
                    break;
                case "spa":
                    lang = "spanish";
                    break;
                case "vie":
                    lang = "vietnamese";
                    break;
                case "tur":
                    lang = "turkish";
                    break;
                case "ben":
                    lang = "bengali";
                    break;
                case "bul":
                    lang = "bulgarian";
                    break;
                case "hrv":
                    lang = "croatian";
                    break;
                case "fin":
                    lang = "finnish";
                    break;
                case "fre":
                    lang = "french";
                    break;
                case "ger":
                    lang = "german";
                    break;
                case "gre":
                    lang = "greek";
                    break;
                case "hun":
                    lang = "hungarian";
                    break;
                case "ita":
                    lang = "italian";
                    break;
                case "kur":
                    lang = "kurdish";
                    break;
                case "mac":
                    lang = "macedonian";
                    break;
                case "mal":
                    lang = "malayalam";
                    break;
                case "nno":
                    lang = "norwegian";
                    break;
                case "nob":
                    lang = "norwegian";
                    break;
                case "nor":
                    lang = "norwegian";
                    break;
                case "por":
                    lang = "portuguese";
                    break;
                case "rus":
                    lang = "russian";
                    break;
                case "srp":
                    lang = "serbian";
                    break;
                case "sin":
                    lang = "sinhala";
                    break;
                case "slv":
                    lang = "slovenian";
                    break;
                case "swe":
                    lang = "swedish";
                    break;
                case "tha":
                    lang = "thai";
                    break;
                case "urd":
                    lang = "urdu";
                    break;
                case "pob":
                    lang = "brazillian-portuguese";
                    break;
            }

            return lang;
        }

        public string Normalize(string language)
        {
            if (!string.IsNullOrWhiteSpace(language))
            {
                return language;
            }

            var culture = _localizationManager?.FindLanguageInfo(language.AsSpan());
            return culture != null
                ? culture.ThreeLetterISOLanguageName
                : language;
        }

    }
}