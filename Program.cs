using System;
using System.IO;

namespace Programowanie2
{
    class Program
    {
        static void Main(string[] args)
        {
            double result = 0.0;
            Programowanie2.ReversePolishNotation rpn = new Programowanie2.ReversePolishNotation();
            rpn.Parse(args[0]);
            rpn.VariableX = Convert.ToDouble(args[1]);
            result = rpn.Evaluate();

            Console.WriteLine("{0}", rpn.TransitionExpression);
            Console.WriteLine("{0}", rpn.PostfixExpression);
            Console.WriteLine(result);
            rpn.CalculateRange(Convert.ToDouble(args[2]),Convert.ToDouble(args[3]),Convert.ToInt32(args[4]));
        }
    }
}