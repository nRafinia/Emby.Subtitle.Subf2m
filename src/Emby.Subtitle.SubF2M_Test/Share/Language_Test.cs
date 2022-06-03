using System;
using System.Buffers;
using System.Linq;
using Emby.Subtitle.SubF2M.Share;
using FluentAssertions;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using Moq;
using Xunit;

namespace Emby.Subtitle.SubF2M_Test.Share;

public class Language_Test
{
    [Theory]
    [InlineData("fa","farsi_persian")]
    [InlineData("per","farsi_persian")]
    [InlineData("ara","arabic")]
    [InlineData("eng","english")]
    [InlineData("bur","burmese")]
    [InlineData("dan","danish")]
    [InlineData("dut","dutch")]
    [InlineData("heb","hebrew")]
    [InlineData("ind","indonesian")]
    [InlineData("kor","korean")]
    [InlineData("may","malay")]
    [InlineData("spa","spanish")]
    [InlineData("vie","vietnamese")]
    [InlineData("tur","turkish")]
    [InlineData("ben","bengali")]
    [InlineData("bul","bulgarian")]
    [InlineData("hrv","croatian")]
    [InlineData("fin","finnish")]
    [InlineData("fre","french")]
    [InlineData("ger","german")]
    [InlineData("gre","greek")]
    [InlineData("hun","hungarian")]
    [InlineData("ita","italian")]
    [InlineData("kur","kurdish")]
    [InlineData("mac","macedonian")]
    [InlineData("mal","malayalam")]
    [InlineData("nno","norwegian")]
    [InlineData("nob","norwegian")]
    [InlineData("nor","norwegian")]
    [InlineData("por","portuguese")]
    [InlineData("rus","russian")]
    [InlineData("srp","serbian")]
    [InlineData("sin","sinhala")]
    [InlineData("slv","slovenian")]
    [InlineData("swe","swedish")]
    [InlineData("tha","thai")]
    [InlineData("urd","urdu")]
    [InlineData("pob","brazillian-portuguese")]
    [InlineData("xxx","xxx")]
    [InlineData("XXX","XXX")]
    public void Map_Return_Correct_Language(string languageCode, string expected)
    {
        //arrange
        var language = new Language(null);
        
        //act
        var result = language.Map(languageCode);
        
        //assert
        result.Should().NotBeEmpty();
        result.Should().Be(expected);
    }
    
    [Fact]
//    [InlineData("English","eng")]
    public void Normalize_Return_Correct_Language()
    {
        //arrange
        var languageManager = new TestLocalizationManager();
        var languageName = "English";
        var expected = "eng";

        var language = new Language(languageManager);
        
        //act
        var result = language.Normalize(languageName);
        
        //assert
        result.Should().NotBeEmpty();
        result.Should().Be(expected);
    }
    
    [Fact]
    private void Normalizer_Return_Empty()
    {
        //arrange
        var language = new Language(null);
        
        //act
        var result = language.Normalize(string.Empty);
        
        //assert
        result.Should().BeEmpty();
    }
    
    private class TestLocalizationManager:ILocalizationManager
    {
        public CultureDto[] GetCultures()
        {
            throw new NotImplementedException();
        }

        public CountryInfo[] GetCountries()
        {
            throw new NotImplementedException();
        }

        public ParentalRating[] GetParentalRatings()
        {
            throw new NotImplementedException();
        }

        public ParentalRating[] GetParentalRatings(string countryCode)
        {
            throw new NotImplementedException();
        }

        public int? GetRatingLevel(ReadOnlySpan<char> rating)
        {
            throw new NotImplementedException();
        }

        public string GetLocalizedString(string phrase, string culture)
        {
            throw new NotImplementedException();
        }

        public string GetLocalizedString(string phrase)
        {
            throw new NotImplementedException();
        }

        public LocalizatonOption[] GetLocalizationOptions()
        {
            throw new NotImplementedException();
        }

        public string RemoveDiacritics(string text)
        {
            throw new NotImplementedException();
        }

        public CultureDto FindLanguageInfo(ReadOnlySpan<char> language)
        {
            return new CultureDto()
            {
                Name = "English",
                DisplayName = "English",
                ThreeLetterISOLanguageNames = new[] { "eng" },
                TwoLetterISOLanguageNames = new[] { "en" }
            };
        }

        public CountryInfo FindCountryInfo(ReadOnlySpan<char> country)
        {
            throw new NotImplementedException();
        }
    }
}