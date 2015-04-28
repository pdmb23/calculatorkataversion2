using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorKata
{
    public class LexicalAnalyzer
    {
        #region Properties
        public TokenType TokenType { get; private set; }
        public string Lexeme { get; private set; }
        public int Level { get; private set; }
        #endregion

        #region Constructor
        public LexicalAnalyzer(TokenType tokenType, string lexeme, int level)
        {
            this.TokenType = tokenType;
            this.Lexeme = lexeme;
            this.Level = level;
        }
        #endregion
    }
}
