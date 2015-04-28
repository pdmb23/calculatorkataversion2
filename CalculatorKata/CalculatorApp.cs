using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorKata
{
    public class CalculatorApp
    {
        #region Fields
        private List<string> inputCollection;       
        private Queue<string> operatorCollection;
        private List<LexicalAnalyzer> lexicalAnalyzer;
        private List<double> subTotalParserList;
        #endregion

        #region Constructor

        public CalculatorApp()
        {
            this.inputCollection = new List<string>();            
            this.operatorCollection = new Queue<string>();
            this.lexicalAnalyzer = new List<LexicalAnalyzer>();
            this.subTotalParserList = new List<double>();
        }

        #endregion

        #region Methods

        public void Input(string token)
        {
            this.inputCollection.Add(token);
        }

        public double GetAnswer()
        {
            var initialIndex = 0;
            var initialLevel = 1;
            this.LexicalAnalysis(initialIndex, initialLevel);
            return this.ParseAndCompute();
        }

        private void LexicalAnalysis(int inputIdx, int level)
        {
            while (inputIdx < this.inputCollection.Count)
            {
                var currentToken = this.GetInput(inputIdx);
                if (this.IsNumber(currentToken))
                {
                    Queue<string> decimalNumberCollection = this.IdentifyDecimalNumber(ref inputIdx, ref currentToken);
                    string decimalNumberValue = this.DecimalNumberParse(decimalNumberCollection);

                    this.InsertLexer(TokenType.Numbers, decimalNumberValue, level);
                }
                else if (this.IsOperator(currentToken))
                {
                    this.InsertLexer(TokenType.Operator, currentToken, level);
                    inputIdx++;
                }
                else if (this.IsOpenParenthesis(currentToken))
                {
                    inputIdx++;
                    level++;
                    this.LexicalAnalysis(inputIdx, level);
                    break;
                }
                else if (this.IsCloseParenthesis(currentToken))
                {
                    inputIdx++;
                    level++;
                }
                else
                {
                    inputIdx++;
                }
            }
        }

        private Queue<string> IdentifyDecimalNumber(ref int inputIdx, ref string currentToken)
        {
            Queue<string> decimalNumberCollection = new Queue<string>();
            while (this.IsNumber(currentToken))
            {
                decimalNumberCollection.Enqueue(currentToken);
                currentToken = this.LookAhead(ref inputIdx);
            }
            if (this.IsDecimalSeparator(currentToken))
            {
                decimalNumberCollection.Enqueue(currentToken);
                currentToken = this.LookAhead(ref inputIdx);
                if (this.IsDecimalSeparator(currentToken))
                {
                    throw new ArgumentException("double decimal symbol is not allowed.");
                }
                while (this.IsNumber(currentToken))
                {
                    decimalNumberCollection.Enqueue(currentToken);
                    currentToken = this.LookAhead(ref inputIdx);
                }
            }
            return decimalNumberCollection;
        }

        private string LookAhead(ref int index)
        {
            index++;
            return this.GetInput(index);
        }

        private void InsertLexer(TokenType tokenType, string lexeme, int level)
        {
            var lexer = new LexicalAnalyzer(tokenType, lexeme, level);
            this.lexicalAnalyzer.Add(lexer);
        }

        private string DecimalNumberParse(Queue<string> decimalNumberCollection)
        {
            string decimalNumberValue = string.Empty;
            while (decimalNumberCollection.Count > 0)
            {
                decimalNumberValue = decimalNumberValue + decimalNumberCollection.Dequeue();
            }
            return decimalNumberValue;
        }

        private string GetInput(int index)
        {
            if (index < this.inputCollection.Count)
            {
                return this.inputCollection[index];
            }
            else
            {
                return string.Empty;
            }
        }

        private double ParseAndCompute()
        {
            double currentItem = 0;
            double partialTotal = 0;
            var operatorMemory = string.Empty;
            int tokenLevel = this.lexicalAnalyzer.Min(l => l.Level);
            int maxLevel = this.lexicalAnalyzer.Max(l => l.Level);
            while (tokenLevel <= maxLevel)
            {                
                for (int i = 0; i < this.lexicalAnalyzer.Count; i++)
                {
                    var item = this.lexicalAnalyzer[i];
                    if (item.Level == tokenLevel)
                    {
                        if (item.TokenType == TokenType.Numbers)
                        {
                            currentItem = Convert.ToDouble(item.Lexeme);
                            if (i == 0)
                            {
                                partialTotal = currentItem;
                                continue;
                            }
                            if (partialTotal > 0)
                            {
                                switch (operatorMemory)
                                {
                                    case "+": 
                                        partialTotal = partialTotal + currentItem;                                        
                                        break;
                                    case "-":
                                        partialTotal = partialTotal - currentItem;
                                        break;
                                    case "*":
                                        partialTotal = partialTotal * currentItem;
                                        break;
                                    case "/":
                                        partialTotal = partialTotal / currentItem;
                                        break;
                                }
                                operatorMemory = string.Empty;
                            }
                            else
                            {
                                partialTotal = currentItem;
                            }                            
                        }
                        else if (item.TokenType == TokenType.Operator)
                        {
                            operatorMemory = item.Lexeme;
                            continue;
                        }
                    }
                }
                if (partialTotal > 0)
                {
                    subTotalParserList.Add(partialTotal);
                }
                if (operatorMemory != string.Empty)
                {
                    this.operatorCollection.Enqueue(operatorMemory);
                }
                partialTotal = 0;
                tokenLevel++;           
            }
            return this.CalculateLexerPerLevel();
        }

        private double CalculateLexerPerLevel()
        {
            double subTotalItem = 0;
            double total = 0;
            for (int i = 0; i < this.subTotalParserList.Count; i++)
            {
                subTotalItem = this.subTotalParserList[i];
                if (i == 0)
                {
                    total = subTotalItem;
                    continue;
                }
                if (total > 0)
                {
                    var operatorMemory = this.operatorCollection.Dequeue();
                    switch (operatorMemory)
                    {
                        case "+": 
                            total = total + subTotalItem;
                            break;
                        case "-": 
                            total = total - subTotalItem;
                            break;
                        case "*":
                            total = total * subTotalItem;
                            break;
                        case "/":
                            total = total / subTotalItem;
                            break;
                    }
                }
                else
                {
                    total = subTotalItem;
                }
            }
            return Math.Round(total, 1);
        }        

        private bool IsNumber(string entry)
        {
            int numberValue;
            if (Int32.TryParse(entry, out numberValue))
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsDecimalSeparator(string entry)
        {
            if (entry == ".")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsOperator(string entry)
        {
            var operationList = new List<string>() { "+", "-", "*", "/" };
            if (operationList.Contains(entry))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsOpenParenthesis(string entry)
        {
            if (entry == "(")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsCloseParenthesis(string entry)
        {
            if (entry == ")")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}