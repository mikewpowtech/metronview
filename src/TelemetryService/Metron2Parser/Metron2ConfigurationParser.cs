using Antlr4.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using Metron2Configuration;

namespace Metron2Parser
{
    public class Metron2ConfigurationParser
    {
        public static Metron2ParserOutput Parse(TextReader tr)
        {
            try
            {
                AntlrInputStream input = new AntlrInputStream(tr);
                Metron2Lexer lexer = new Metron2Lexer(input);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                Metron2Parser parser = new Metron2Parser(tokenStream);
                
                StringBuilder errorBuilder = new StringBuilder();
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new AccumulateErrors(errorBuilder));
                Metron2Parser.CompileUnitContext retval = parser.compileUnit();
                
                return new Metron2ParserOutput { Success = parser.NumberOfSyntaxErrors == 0, Configuration = retval.value, Errors = errorBuilder.ToString() };
            }
            catch (Exception ex)
            {
                return new Metron2ParserOutput { Success = false, Errors = ex.Message };
            }
        }

        private class AccumulateErrors : IAntlrErrorListener<IToken>
        {
            private readonly StringBuilder sb;

            public AccumulateErrors(StringBuilder sb)
            {
                this.sb = sb;
            }

            public void SyntaxError(TextWriter textWriter, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                sb.AppendLine("Line " + line + ", character " + charPositionInLine + ": " + msg + " (offending symbol was of type " + offendingSymbol.Type + ")");
            }
        }
    }

    public class Metron2ParserOutput
    {
        public bool Success { get; set; }
        public Configuration Configuration { get; set; }
        public string Errors { get; set; }
    }
}
