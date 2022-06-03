using System;
using Emby.Subtitle.SubF2M.Extensions;
using FluentAssertions;
using Xunit;

namespace Emby.Subtitle.SubF2M_Test.Extensions;

public class StringExt_Test
{
    [Theory]
    [InlineData("Hello World", "Hello", 0, 4)]
    [InlineData("Test", "Test", 0, 3)]
    public void SubStr_ReturnValid_String(string text, string meaning, int from, int to)
    {
        //arrange

        //act
        var result = text.SubStr(from, to);

        //assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Be(meaning);
    }

    [Fact]
    public void SubStr_ReturnException_ForEmpty()
    {
        //arrange
        var text = string.Empty;

        //act
        var runFunc =()=> text.SubStr(0, 5);
        
        //assert
        runFunc.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RemoveExtraCharacter_Return_Valid_String()
    {
        //arrange
        var text = " Hello\t World\r\n";

        //act
        var result = text.RemoveExtraCharacter();

        //assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Be("Hello World");
    }
    
    [Fact]
    public void RemoveExtraCharacter_Return_Empty_String()
    {
        //arrange
        var text = string.Empty;

        //act
        var result = text.RemoveExtraCharacter();

        //assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void FixHtml_Return_Valid_String()
    {
        //arrange
        var text = "Hello&nbsp;&World";

        //act
        var result = text.FixHtml();

        //assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Be("Hello &amp;World");
    }
    
    [Fact]
    public void FixHtml_Return_Empty_String()
    {
        //arrange
        var text = string.Empty;

        //act
        var result = text.FixHtml();

        //assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}