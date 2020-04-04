using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Programowanie2
{
    public enum TokenType
    {
        None,
        Number,
        Constant,
        Plus,
        Minus,
        Multiply,
        Divide,
        Exponent,
        UnaryMinus,
        Sin,
        Cos,
        Tan,
        Abs,
        Exp,
        Log,
        Sqrt,
        Cosh,
        Sinh,
        Tanh,
        Acos,
        Asin,
        Atan,
        Pi,
        E,
        LeftParenthesis,
        RightParenthesis,
        Variable
    }

    public struct ReversePolishNotationToken
    {
        public string TokenValue;
        public TokenType TokenValueType;
    }

    public class ReversePolishNotation
    {
        private Queue output;
        private Stack ops;

        private string sOriginalExpression;
        public string OriginalExpression
        {
            get { return sOriginalExpression; }
        }

        private string sTransitionExpression;
        public string TransitionExpression
        {
            get { return sTransitionExpression; }
        }

        private string sPostfixExpression;
        public string PostfixExpression
        {
            get { return sPostfixExpression; }
        }

        private double sVariableX;
        public double VariableX
        {
            get { return sVariableX; }
            set { sVariableX = value; }
        }

        public ReversePolishNotation()
        {
            sOriginalExpression = string.Empty;
            sTransitionExpression = string.Empty;
            sPostfixExpression = string.Empty;
        }

        public void Parse(string Expression)
        {
            output = new Queue();
            ops = new Stack();

            sOriginalExpression = Expression;

            string sBuffer = Expression.ToLower();

            sBuffer = Regex.Replace(sBuffer, @"(?<number>\d+(\.\d+)?)", " ${number} ");                    
            sBuffer = Regex.Replace(sBuffer, @"(?<ops>[+\-*/^()])", " ${ops} ");                          
            sBuffer = Regex.Replace(sBuffer, "(?<alpha>(exp|asin|sinh|acos|cosh|atan|tanh|pi|e|sin|cos|tan|abs|log|sqrt))", " ${alpha} ");     
            sBuffer = Regex.Replace(sBuffer, @"\s+", " ").Trim();                                         
            sBuffer = Regex.Replace(sBuffer, "-", "MINUS");
            sBuffer = Regex.Replace(sBuffer, @"(?<number>(pi|e|([)]|\d+(\.\d+)?)))\s+MINUS", "${number} -");
            sBuffer = Regex.Replace(sBuffer, "MINUS", "~");

            sTransitionExpression = sBuffer;

            //TOKEN
            string[] saParsed = sBuffer.Split(" ".ToCharArray());
            int i = 0;
            double tokenvalue;
            ReversePolishNotationToken token, opstoken;
            for (i = 0; i < saParsed.Length; ++i)
            {
                token = new ReversePolishNotationToken();
                token.TokenValue = saParsed[i];
                token.TokenValueType = TokenType.None;

                try
                {
                    tokenvalue = double.Parse(saParsed[i]);
                    token.TokenValueType = TokenType.Number;

                    output.Enqueue(token);
                }
                catch
                {
                    switch (saParsed[i])
                    {
                        case "+":
                            token.TokenValueType = TokenType.Plus;
                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                while (IsOperatorToken(opstoken.TokenValueType))       
                                {

                                    output.Enqueue(ops.Pop());      
                                    if (ops.Count > 0)
                                    {
                                        opstoken = (ReversePolishNotationToken)ops.Peek();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            ops.Push(token);        
                            break;
                        case "-":
                            token.TokenValueType = TokenType.Minus;
                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                while (IsOperatorToken(opstoken.TokenValueType))      
                                {

                                    output.Enqueue(ops.Pop());    
                                    if (ops.Count > 0)
                                    {
                                        opstoken = (ReversePolishNotationToken)ops.Peek();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            ops.Push(token);      
                            break;
                        case "*":
                            token.TokenValueType = TokenType.Multiply;
                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                while (IsOperatorToken(opstoken.TokenValueType))       
                                {
                                    if (opstoken.TokenValueType == TokenType.Plus || opstoken.TokenValueType == TokenType.Minus)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        output.Enqueue(ops.Pop()); 
                                        if (ops.Count > 0)
                                        {
                                            opstoken = (ReversePolishNotationToken)ops.Peek();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            ops.Push(token); 
                            break;
                        case "/":
                            token.TokenValueType = TokenType.Divide;
                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                while (IsOperatorToken(opstoken.TokenValueType)) 
                                {
                                    if (opstoken.TokenValueType == TokenType.Plus || opstoken.TokenValueType == TokenType.Minus)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        output.Enqueue(ops.Pop()); 
                                        if (ops.Count > 0)
                                        {
                                            opstoken = (ReversePolishNotationToken)ops.Peek();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            ops.Push(token);   
                            break;
                        case "^":
                            token.TokenValueType = TokenType.Exponent;

                            ops.Push(token);   
                            break;
                        case "~":
                            token.TokenValueType = TokenType.UnaryMinus;

                            ops.Push(token); 
                            break;
                        case "(":
                            token.TokenValueType = TokenType.LeftParenthesis;

                            ops.Push(token); 
                            break;
                        case ")":
                            token.TokenValueType = TokenType.RightParenthesis;
                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                while (opstoken.TokenValueType != TokenType.LeftParenthesis) 
                                {

                                    output.Enqueue(ops.Pop()); 
                                    if (ops.Count > 0)
                                    {
                                        opstoken = (ReversePolishNotationToken)ops.Peek();
                                    }
                                    else
                                    {
                                        throw new Exception("Unbalanced parenthesis!"); 
                                    }

                                }

                                ops.Pop(); 
                            }

                            if (ops.Count > 0)
                            {
                                opstoken = (ReversePolishNotationToken)ops.Peek();

                                if (IsFunctionToken(opstoken.TokenValueType)) 
                                {

                                    output.Enqueue(ops.Pop()); 
                                }
                            }
                            break;
                        case "x":
                            token.TokenValueType = TokenType.Variable;
                            output.Enqueue(token);
                            break;
                        case "pi":
                            token.TokenValueType = TokenType.Constant;
                            output.Enqueue(token);
                            break;
                        case "e":
                            token.TokenValueType = TokenType.Constant;
                            output.Enqueue(token);
                            break;
                        case "sin":
                            token.TokenValueType = TokenType.Sin;
                            ops.Push(token);
                            break;
                        case "cos":
                            token.TokenValueType = TokenType.Cos;
                            ops.Push(token);
                            break;
                        case "tan":
                            token.TokenValueType = TokenType.Tan;
                            ops.Push(token);
                            break;
                        case "abs":
                            token.TokenValueType = TokenType.Abs;
                            ops.Push(token);
                            break;
                        case "exp":
                            token.TokenValueType = TokenType.Exp;
                            ops.Push(token);
                            break;
                        case "log":
                            token.TokenValueType = TokenType.Log;
                            ops.Push(token);
                            break;
                        case "sqrt":
                            token.TokenValueType = TokenType.Sqrt;
                            ops.Push(token);
                            break;
                        case "cosh":
                            token.TokenValueType = TokenType.Cosh;
                            ops.Push(token);
                            break;
                        case "sinh":
                            token.TokenValueType = TokenType.Sinh;
                            ops.Push(token);
                            break;
                        case "tanh":
                            token.TokenValueType = TokenType.Tanh;
                            ops.Push(token);
                            break;
                        case "acos":
                            token.TokenValueType = TokenType.Acos;
                            ops.Push(token);
                            break;
                        case "asin":
                            token.TokenValueType = TokenType.Asin;
                            ops.Push(token);
                            break;
                        case "atan":
                            token.TokenValueType = TokenType.Atan;
                            ops.Push(token);
                            break;
                    }
                }
            }

            while (ops.Count != 0) 
            {
                opstoken = (ReversePolishNotationToken)ops.Pop();

                if (opstoken.TokenValueType == TokenType.LeftParenthesis) 
                {
                    throw new Exception("Unbalanced parenthesis!"); 
                }
                else
                {
                    output.Enqueue(opstoken);  
                }
            }

            sPostfixExpression = string.Empty;
            foreach (object obj in output)
            {
                opstoken = (ReversePolishNotationToken)obj;
                sPostfixExpression += string.Format("{0} ", opstoken.TokenValue);
            }
        }

        public double Evaluate()
        {
            Stack result = new Stack();
            double oper1 = 0.0, oper2 = 0.0;
            ReversePolishNotationToken token = new ReversePolishNotationToken();
            foreach (object obj in output) 
            {
                token = (ReversePolishNotationToken)obj; 
                switch (token.TokenValueType)
                {
                    case TokenType.Variable:
                        result.Push(sVariableX);
                        break;
                    case TokenType.Number:

                        result.Push(double.Parse(token.TokenValue));
                        break;
                    case TokenType.Constant:
 
                        result.Push(EvaluateConstant(token.TokenValue));
                        break;
                    case TokenType.Plus:
                       
                        if (result.Count >= 2)
                        {
                       
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                         
                            result.Push(oper1 + oper2);
                        }
                        else
                        {
                           
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Minus:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();

                            result.Push(oper1 - oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Multiply:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();

                            result.Push(oper1 * oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Divide:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();

                            result.Push(oper1 / oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Exponent:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();

                            result.Push(Math.Pow(oper1, oper2));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.UnaryMinus:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(-oper1);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Sin:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Sin(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Cos:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Cos(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Tan:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Tan(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Abs:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Abs(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Exp:
                        if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Exp(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Log:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Log(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Sqrt:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Sqrt(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Cosh:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Cosh(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Sinh:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Sinh(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Tanh:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Tanh(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Acos:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Acos(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Asin:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Asin(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Atan:
                    if (result.Count >= 1)
                        {
                            oper1 = (double)result.Pop();

                            result.Push(Math.Atan(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                }
            }

            if (result.Count == 1)
            {
                return (double)result.Pop(); 
            }
            else
            {
                throw new Exception("Evaluation error!");
            }
        }

        private bool IsOperatorToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Exponent:
                case TokenType.UnaryMinus:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        private bool IsFunctionToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.Sin:
                case TokenType.Cos:
                case TokenType.Tan:
                case TokenType.Exp:
                case TokenType.Abs:
                case TokenType.Log:
                case TokenType.Sqrt:
                case TokenType.Cosh:
                case TokenType.Sinh:
                case TokenType.Tanh:
                case TokenType.Acos:
                case TokenType.Asin:
                case TokenType.Atan:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        private double EvaluateConstant(string TokenValue)
        {
            double result = 0.0;
            switch (TokenValue)
            {
                case "pi":
                    result = Math.PI;
                    break;
                case "e":
                    result = Math.E;
                    break;
            }
            return result;
        }

    public void CalculateRange(double xMin, double xMax, int n) //calculate range between xMin and xMax
        {
            double result = 0;
            double length = (xMax - xMin) / (n - 1);
            sVariableX = xMin;
            for (int i = 0; i < n; i++)
            {
                result = Evaluate();
                System.Console.WriteLine("{0} => {1}", String.Format("{0:0.##}", sVariableX), result);
                sVariableX += length;
            }
        }
    }
}