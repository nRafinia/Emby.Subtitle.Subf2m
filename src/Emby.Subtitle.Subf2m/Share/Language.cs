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
                case "fa":
                case "per":
                    lang = "farsi_persian";
                    break;
                case "ar":
                case "ara":
                    lang = "arabic";
                    break;
                case "en":
                case "eng":
                    lang = "english";
                    break;
                case "my":
                case "bur":
                    lang = "burmese";
                    break;
                case "da":
                case "dan":
                    lang = "danish";
                    break;
                case "nl":
                case "dut":
                    lang = "dutch";
                    break;
                case "he":
                case "heb":
                    lang = "hebrew";
                    break;
                case "id":
                case "ind":
                    lang = "indonesian";
                    break;
                case "ko":
                case "kor":
                    lang = "korean";
                    break;
                case "ms":
                case "may":
                    lang = "malay";
                    break;
                case "es":
                case "spa":
                    lang = "spanish";
                    break;
                case "vi":
                case "vie":
                    lang = "vietnamese";
                    break;
                case "tr":
                case "tur":
                    lang = "turkish";
                    break;
                case "bn":
                case "ben":
                    lang = "bengali";
                    break;
                case "bg":
                case "bul":
                    lang = "bulgarian";
                    break;
                case "hr":
                case "hrv":
                    lang = "croatian";
                    break;
                case "fi":
                case "fin":
                    lang = "finnish";
                    break;
                case "fr":
                case "fre":
                    lang = "french";
                    break;
                case "de":
                case "ger":
                    lang = "german";
                    break;
                case "el":
                case "gre":
                    lang = "greek";
                    break;
                case "hu":
                case "hun":
                    lang = "hungarian";
                    break;
                case "it":
                case "ita":
                    lang = "italian";
                    break;
                case "ku":
                case "kur":
                    lang = "kurdish";
                    break;
                case "mk":
                case "mac":
                    lang = "macedonian";
                    break;
                case "ml":
                case "mal":
                    lang = "malayalam";
                    break;
                case "nn":
                case "nno":
                case "nb":
                case "nob":
                case "no":
                case "nor":
                    lang = "norwegian";
                    break;
                case "pt":
                case "por":
                    lang = "portuguese";
                    break;
                case "ru":
                case "rus":
                    lang = "russian";
                    break;
                case "sr":
                case "srp":
                    lang = "serbian";
                    break;
                case "si":
                case "sin":
                    lang = "sinhala";
                    break;
                case "sl":
                case "slv":
                    lang = "slovenian";
                    break;
                case "sv":
                case "swe":
                    lang = "swedish";
                    break;
                case "th":
                case "tha":
                    lang = "thai";
                    break;
                case "ur":
                case "urd":
                    lang = "urdu";
                    break;
                case "pt-br":
                case "pob":
                    lang = "brazillian-portuguese";
                    break;
            }

            return lang;
        }

        public string Normalize(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
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