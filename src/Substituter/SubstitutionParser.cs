using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Substituter
{
    public class SubstitutionParser
    {
        public static SubstituterParserOutput Parse(TextReader tr)
        {
            try
            {
                AntlrInputStream input = new AntlrInputStream(tr);
                SubstituterLexer lexer = new SubstituterLexer(input);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                SubstituterParser parser = new SubstituterParser(tokenStream);
                StringBuilder errorBuilder = new StringBuilder();
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new AccumulateErrors(errorBuilder));
                SubstituterParser.CompileUnitContext retval = parser.compileUnit();
                return new SubstituterParserOutput { Success = parser.NumberOfSyntaxErrors == 0, ChunkList = retval.chunkList, Errors = errorBuilder.ToString() };
            }
            catch (Exception ex)
            {
                return new SubstituterParserOutput { Success = false, Errors = ex.Message };
            }
        }

        /// <summary>
        /// Substitute any {...} values in s for their values, and return the substituted string.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Substitute(IDictionary<string, string> substitutes, string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            StringBuilder outputBuilder = new StringBuilder();
            foreach (Chunk chunk in ToChunkList(s).Chunks)
            {
                if (chunk.IsSubstitution)
                {
                    string value;
                    if (substitutes.TryGetValue(chunk.Text.Replace(" ", "").ToLower(), out value))
                        outputBuilder.Append(value);
                }
                else
                {
                    outputBuilder.Append(chunk.Text);
                }
            }
            return outputBuilder.ToString();
        }

        public static ChunkList ToChunkList(string s)
        {
            SubstituterParserOutput output = SubstitutionParser.Parse(new StringReader(s));
            if (null == output)
                throw new Exception("Syntax error: ");
            if (null == output.ChunkList || !output.Success)
                throw new Exception("Couldn't parse expression: " + output.Errors);
            return output.ChunkList;
        }

        private class AccumulateErrors : IAntlrErrorListener<IToken>
        {
            public AccumulateErrors(StringBuilder sb)
            {
            }

            public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                output.WriteLineAsync("Line " + line + ", character " + charPositionInLine + ": " + msg + " (offending symbol was of type " + offendingSymbol.Type + ")");
            }
        }
    }
}
