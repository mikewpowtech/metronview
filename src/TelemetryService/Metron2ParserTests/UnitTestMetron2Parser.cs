using Antlr4.Runtime;
using Metron2Parser;
using Metron2ParserTests.Helpers;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Metron2ParserTests;

public class UnitTestMetron2Parser
{
    [Fact]
    public void TestAgainstExtractedList()
    {
        using TextReader tr = new StreamReader("configs.txt", Encoding.ASCII);
        var (parser, errorBuilder, retval) = Parse(tr);
        
        parser.NumberOfSyntaxErrors.ShouldBe(0);
        errorBuilder.ToString().ShouldBeEmpty();
        retval.ShouldNotBeNull();
    }

    private static (Metron2Parser.Metron2Parser parser, StringBuilder errorBuilder, Metron2Parser.Metron2Parser.CompileUnitContext retval) 
        Parse(TextReader tr)
    {
        var tokenStream = MakeCommonTokenStream(tr);
        Metron2Parser.Metron2Parser parser = new Metron2Parser.Metron2Parser(tokenStream);
        StringBuilder errorBuilder = new StringBuilder();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new AccumulateErrors(errorBuilder));
        Metron2Parser.Metron2Parser.CompileUnitContext retval = parser.compileUnit();
        return (parser, errorBuilder, retval);
    }

    private static CommonTokenStream MakeCommonTokenStream(TextReader tr)
    {
        AntlrInputStream input = new AntlrInputStream(tr);
        Metron2Lexer lexer = new Metron2Lexer(input);
        return new CommonTokenStream(lexer);
    }

    [Fact]
    public void Metron2ConfigurationParser_Parse_IdleIsTwoDigits()
    {
        var configuration = Metron2ConfigurationParser.Parse(new StringReader(@"1234,2,Test Metron,0,0,0,4,4,1,0,11,1,0,0,2,
"));
        configuration.Configuration.SystemConfiguration.Idle.ShouldBe(11);
    }

    public static IEnumerable<object[]> ValidIdleValues = 
        Enumerable.Range(0, 12).ToTestData(i => i.ToString(), i => i);

    [Theory]
    [MemberData(nameof(ValidIdleValues))]
    public void TestValidIdleDigits(string idleString, int idleValue)
    {
        var configuration = ParseIdleValue(idleString);

        configuration.Errors.ShouldBeEmpty();
        configuration.Success.ShouldBeTrue();
        configuration.Configuration.SystemConfiguration.Idle.ShouldBe(idleValue);
    }

    public static IEnumerable<object[]> NotValidIdleValues =
        new[] { "", "111", "a" }.ToTestData();
    
    [Theory]
    [MemberData(nameof(NotValidIdleValues))]
    public void WhenParsingAThreeDigitIdleValueAnErrorIsThrown(string value)
    {
        var configuration = ParseIdleValue(value);

        configuration.Errors.ShouldNotBeEmpty();
        configuration.Success.ShouldBeFalse();
    }

    private static Metron2ParserOutput ParseIdleValue(string idleValue)
    {
        string systemConfigurationLine = $"1234,2,Test Metron,0,0,0,4,4,1,0,{idleValue},1,0,0,2,\n";
        var configuration = Metron2ConfigurationParser.Parse(new StringReader(systemConfigurationLine));
        return configuration;
    }
}

class AccumulateErrors : IAntlrErrorListener<IToken>
{
    private readonly StringBuilder sb;

    public AccumulateErrors(StringBuilder sb)
    {
        this.sb = sb;
    }

    public void SyntaxError(TextWriter textWriter, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e)
    {
        sb.AppendLine("Line " + line + ", character " + charPositionInLine + ": " + msg +
                      " (offending symbol was of type " + offendingSymbol.Type + ")");
    }
}