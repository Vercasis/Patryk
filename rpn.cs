using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WEB_API
{
 enum elementTypes
    {
        NUMBER,
        SIGN,
        X,
        EXPRESSION,
        POWER,
        BRACKET
    };

    class elementType
    {
        public elementType(elementTypes t, string v)
        {
            elementtype = t;
            value = v;
        }
        public string value { get; private set; }
        public elementTypes elementtype { get; private set; }
    }

    class EquasionException : Exception
    {
        public EquasionException(string message) : base(message)
        { }
    }

    class RPN
    {
        Dictionary<string, int> priority = new Dictionary<string, int>()
        {
            {"-",1},
            {"+",1},
            {"*",2},
            {"/",2},
            {"^",3}
        };
        char[] signs = { '-', '+', '/', '*', '^'};
        char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ',' };
        char[] signs2 = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        public dynamic errorM(string ex)
        {
            dynamic error = new ExpandoObject();

            error.status = "error";
            error.message = ex;
            return error;
        }

        public dynamic Formula(string row)
        {
            List<string> infix;
           
            try
            {
                if(row == null || row == "")
                    throw new EquasionException("Nie podałeś równania");
                List<elementType> oNP = CreateONP(row,  out infix);
                List<string> s = new List<string>();
                foreach(elementType t in oNP)
                {
                    s.Add(t.value);
                }

                dynamic result = new ExpandoObject();
                dynamic returned = new ExpandoObject();

                result.infix = infix.ToArray();
                result.rpn = s.ToArray();

                returned.status = "ok";
                returned.result = result;
             
                return returned;
            }
            catch (EquasionException iq)
            {             
                return errorM(iq.Message);
            }
            catch (Exception ex)
            {   
                return errorM(ex.Message);
            }

        }

        public dynamic Formula(string row, double x)
        {
            StringBuilder sb = new StringBuilder();
            List<string> infix;         
            try
            {
                if(row == null || row == "")
                    throw new EquasionException("Nie podałeś równania");
                List<elementType> oNP = CreateONP(row, out infix);
                double result = CalculateUnknown(x, oNP);

                dynamic returned = new ExpandoObject();

                returned.status = "ok";
                returned.result = result;
                return returned;
            }
            catch (EquasionException iq)
            {
                return errorM(iq.Message);
            }
            catch (Exception ex)
            {
                return errorM(ex.Message);
            }

        }

        public dynamic Formula(string row, double min, double max, int ammount)
        {
            List<string> infix;

            StringBuilder sb = new StringBuilder();
            try
            {
                if(row == null || row == "")
                    throw new EquasionException("Nie podałeś równania");
                List<elementType> oNP = CreateONP(row, out infix);
                List<dynamic> result = CalculateUnknownRange(min, max, ammount, oNP);

                dynamic returned = new ExpandoObject();


                returned.status = "ok";
                returned.result = result.ToArray();
                return returned;
            }
            catch (EquasionException iq)
            {
                return errorM(iq.Message);
            }
            catch (Exception ex)
            {
                return errorM(ex.Message);
            }

        }        List<elementType> Deepcopy(List<elementType> elements)
        {
            List<elementType> temp = new List<elementType>();
            for (int j = 0; j < elements.Count; j++)
            {
                temp.Add(new elementType(elements[j].elementtype, elements[j].value));
            }
            return temp;
        }

        public dynamic Reverse(string onp)
        {
              try
            {
                dynamic returned = new ExpandoObject();
                returned.status = "ok";
                returned.result = ReverseONP2(onp);
                return returned;
            }
            catch (EquasionException iq)
            {
                return errorM(iq.Message);
            }
            catch (Exception ex)
            {
                return errorM(ex.Message);
            }
        }
        List<elementType> CreateONP(string row, out List<string> infix)
        {
           
            row = row.Replace("=","+").Replace('.', ',');//.Replace(' ','+');
            List<elementType> elements = new List<elementType>();
            List<Stack<elementType>> characters = new List<Stack<elementType>>();
            Stack<elementType> sincosStack = new Stack<elementType>();
            characters.Add(new Stack<elementType>());
            infix = new List<string>();
            int bracketCounter = 0, sincosCounter = 0;
            string number = "", minus = "", tempString;
            elementType characterTemp = null;
            bool divide = false, umberIsTaken = false, bracketIsOpen = true;

            void numbers()
            {
                minus = "";
                divide = false;
                umberIsTaken = true;
                bracketIsOpen = false;
            }

            void equasions()
            {
                umberIsTaken = false;
                bracketIsOpen = true;
                minus = "";
                bracketCounter++;
                sincosCounter++;
                if (characters.Count <= bracketCounter)
                {
                    characters.Add(new Stack<elementType>());
                }
            }

            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == '-' && bracketIsOpen)
                {
                    minus = "-";
                    bracketIsOpen = false;
                    continue;
                }

                if (row.Length >= i + 2 && row.Substring(i, 2) == "PI")
                {
                    if (umberIsTaken)
                        throw new EquasionException("Umieściłeś PI zaraz po liczbie bez żadnego znaku rozdielającego jak +,-,*,/,^");

                    i += 1;
                    elements.Add(new elementType(elementTypes.NUMBER, minus + Math.PI.ToString()));                   
                    infix.Add(minus + "PI");
                    numbers();
                    continue;
                }

                if (row[i] == 'x')
                {
                    if (umberIsTaken)
                        throw new EquasionException("Umieściłeś x zaraz po liczbie bez żadnego znaku rozdielającego jak +,-,*,/,^");

                    elements.Add(new elementType(elementTypes.X, minus + "x"));
                    infix.Add(minus + "x");
                    numbers();
                    continue;
                }

                if (row.IndexOfAny(this.numbers, i) == i)
                {
                    while (row.IndexOfAny(this.numbers, i) == i)
                    {
                        number += row[i];
                        i++;
                    }
                    i--;
                    if (divide && double.Parse(number) == 0 || umberIsTaken || number == "," || number.IndexOf(",") != number.LastIndexOf(","))
                        throw new EquasionException("Wrównaniu któraś liczba ma za dużo kropek lub dzielisz przez zero");

                    elements.Add(new elementType(elementTypes.NUMBER, minus + number));               
                    infix.Add(minus + number);
                    number = "";
                    numbers();
                    continue;
                }

                if (row.IndexOfAny(signs, i) == i)
                {
                    if (!umberIsTaken || minus == "-" || bracketIsOpen)
                        throw new EquasionException("W miejscu: " + (i+1) + "występują 2 lub więcej znaków koło siebi umieść nawiasy lub usuń niepoprawne znaki, niepoprawnym znakiem jest: " + row[i]);

                    if (row[i] == '^')
                        characterTemp = new elementType(elementTypes.POWER, row[i].ToString());
                    else
                    {
                        if (row[i].ToString() == "/")
                            divide = true;
                        characterTemp = new elementType(elementTypes.SIGN, row[i].ToString());
                    }
                    infix.Add(row[i].ToString());
                    elementType z = null;

                    int p1, p2;
                        priority.TryGetValue(characterTemp.value, out p1);                     
                            while (characters[bracketCounter].Count > 0)
                            {
                                z = characters[bracketCounter].Pop();
                                priority.TryGetValue(z.value, out p2);
                                if (p1 <= p2)
                                {
                                    elements.Add(z);
                                }
                                else
                                {
                                    characters[bracketCounter].Push(z);
                                    break;
                                }
                            }
                    
                    characters[bracketCounter].Push(characterTemp);
                    umberIsTaken = false;
                    continue;
                }

                if (row.Length >= i + 5)
                {                   
                    tempString = row.Substring(i, 4);
                    if (tempString == "abs(" || tempString == "cos(" || tempString == "sin(" || tempString == "tan(" || tempString == "exp(" || tempString == "log(")
                    {
                        tempString = minus + tempString.Substring(0, 3);
                        sincosStack.Push(new elementType(elementTypes.EXPRESSION, tempString));
                        infix.Add(tempString);
                        i += 3;
                        equasions();
                        continue;
                    }
                    tempString = row.Substring(i, 5);
                    if (tempString == "sqrt(" || tempString == "cosh(" || tempString == "sinh(" || tempString == "tanh(" || tempString == "asin(" || tempString == "acos(" || tempString == "atan(")
                    {
                        tempString = minus + tempString.Substring(0, 4);
                        sincosStack.Push(new elementType(elementTypes.EXPRESSION, tempString));
                        infix.Add(tempString);
                        i += 4;
                        equasions();
                        continue;
                    }
                }

                if (row[i] == '(')
                {
                  
                    bracketCounter++;
                    if (characters.Count <= bracketCounter)
                    {
                        characters.Add(new Stack<elementType>());
                    }
                    sincosStack.Push(new elementType(elementTypes.BRACKET, "("));
                    bracketIsOpen = true;
                    continue;
                }

                if (row[i] == ')')
                {
                    
                    if (!umberIsTaken || sincosStack.Count <= 0)
                        throw new EquasionException("Masz za dużo nawiasów zamykających");

                    elementType t = sincosStack.Pop();
                    while (characters[bracketCounter].Count != 0)
                    {
                        elements.Add(characters[bracketCounter].Pop());
                    }
                    bracketCounter--;
                    if (t.elementtype == elementTypes.EXPRESSION)
                    {
                        sincosCounter--;
                        elements.Add(t);
                    }
                    continue;

                }
                throw new EquasionException("równanie ma niepoprawne znaki w miejscu zaznaczonym < błąd >: " + row.Substring(0,i) + "< " +row[i] + " >" + row.Substring(i+1));
            }

            if (bracketCounter != 0 || !umberIsTaken || divide)
            {
                throw new EquasionException("Coś jest źle z równaniem brak nawiasu lub nawiasów zamykających, lub na końcu równania znajduje się któryś z tych znakó: +,-,*,/,^");
            }

            while (characters[bracketCounter].Count != 0)
            {
                elements.Add(characters[bracketCounter].Pop());
            }

            return elements;
        }
        double CalculateUnknown(double x, List<elementType> elements)
        {
                List<elementType> backup = Deepcopy(elements);
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].elementtype == elementTypes.X)
                    {
                        if (elements[i].value == "-x")
                            backup[i] = new elementType(elementTypes.NUMBER, '-' + x.ToString());
                        else
                            backup[i] = new elementType(elementTypes.NUMBER, x.ToString());
                    }
                }
            double temp = Calculate(backup);
            if (double.IsInfinity(temp))
                throw new EquasionException("Wynik jest zbyt wysoki");
            return temp;                 
        }
        List<dynamic> CalculateUnknownRange(double min, double max, int ammount, List<elementType> elements)
        {
                dynamic d = new ExpandoObject();
                List<dynamic> results = new List<dynamic>();
              
                List<elementType> backup = Deepcopy(elements);
                double am = (max - min) / (ammount-1);
                am = Math.Round(am, 15);
                for (double i = 0; i < ammount; i++)
                {
                
                    for (int j = 0; j < elements.Count; j++)
                    {
                        if (elements[j].elementtype == elementTypes.X)
                        {
                            if (elements[j].value == "-x")
                                backup[j] = new elementType(elementTypes.NUMBER, '-' + min.ToString());
                            else
                                backup[j] = new elementType(elementTypes.NUMBER, min.ToString());
                        }
                    }
                    double temp = Calculate(backup);
                    d.x = Math.Round(min, 10);
                    if (double.IsInfinity(temp))
                    {                   
                        d.y = "Wynik jest zbyt wysoki";
                    }
                    else
                    {
                        d.y = temp;
                    }
                   
                    results.Add(d);
                    d = new ExpandoObject();
                    min = min + am;
                }
            
            return results;
        }
        double Calculate(List<elementType> elements)
        {
            double typ1, typ2;
            Stack<elementType> equasion = new Stack<elementType>();

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].elementtype == elementTypes.NUMBER || elements[i].elementtype == elementTypes.X)
                {
                    equasion.Push(elements[i]);
                }
                else if (elements[i].elementtype == elementTypes.SIGN)
                {
                    if (equasion.Count() >= 2 && double.TryParse(equasion.Pop().value, out typ1) && double.TryParse(equasion.Pop().value, out typ2))
                        equasion.Push(new elementType(elementTypes.NUMBER, CalculateSigns(typ2, typ1, elements[i].value)));
                }
                else if (elements[i].elementtype == elementTypes.EXPRESSION)
                {
                    if (equasion.Count() >= 1 && double.TryParse(equasion.Pop().value, out typ1))
                    {
                        equasion.Push(new elementType(elementTypes.NUMBER, CalculateSinCos(typ1, elements[i].value)));
                    }
                }
                else if (elements[i].elementtype == elementTypes.POWER)
                {
                    if (equasion.Count() >= 2 && double.TryParse(equasion.Pop().value, out typ1) && double.TryParse(equasion.Pop().value, out typ2))
                        equasion.Push(new elementType(elementTypes.NUMBER, Math.Pow(typ2, typ1).ToString()));
                }
            }          
            return double.Parse(equasion.Pop().value);
        } 
        string ReverseONP2(string onp)
        {
            if (onp != null || onp =="")
            {
                onp = onp.Replace("=","+");
                string[] elements = onp.Split(' ');               
                string one, two;
                StringBuilder stringBuilder = new StringBuilder();

                Stack<string> store = new Stack<string>();
                Regex regexNumber = new Regex(@"^\d\.\d$|^-\d\.\d$|^\d$|^-\d$|^PI$|^-PI$|^x$|^-x$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Regex regexSigns = new Regex(@"^\+$|^\-$|^\^$|^\*$|^/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                for (int i = 0; i < elements.Length; i++)
                {
                    if (regexNumber.IsMatch(elements[i]))
                    {
                        store.Push(elements[i]);
                    }
                    else if (regexSigns.IsMatch(elements[i]))//elements[i].IndexOfAny(signs) == 0 )
                    {
                        if(store.Count <2)
                            throw new EquasionException("Odwrotna notacja którą podałeś jest nie poprawna nie podałeś w odpowiedniej kolejności liczb i znaków lub kótrychś elementów jest za dużo, błąd znajduje się na pozycji: " + (i+1) + " jest nim: " + elements[i]);
                       
                  
                        one = store.Pop();
                        two = store.Pop();

                        switch (elements[i])
                        {
                            case "-":
                                if (two.Contains("*") || two.Contains("/") || two.Contains("^") )
                                {
                                    two = stringBuilder.AppendFormat("({0})", two).ToString();
                                    stringBuilder.Clear();
                                }
                                if (one.Contains("*") || one.Contains("/") || one.Contains("^"))
                                {
                                    one = stringBuilder.AppendFormat("({0})", one).ToString();
                                    stringBuilder.Clear();
                                }

                                if (one[0] == '-')
                                {
                                    one = one.Remove(0,1);
                                    store.Push(stringBuilder.AppendFormat("{0}+{1}", two, one).ToString());
                                }
                                else
                                    store.Push(stringBuilder.AppendFormat("{0}-{1}", two, one).ToString());
                                break;
                            case "+":
                                if (two.Contains("*") || two.Contains("/") || two.Contains("^"))
                                {
                                    two = stringBuilder.AppendFormat("({0})", two).ToString();
                                    stringBuilder.Clear();
                                }
                                if (one.Contains("*") || one.Contains("/") || one.Contains("^"))
                                {
                                    one = stringBuilder.AppendFormat("({0})", one).ToString();
                                    stringBuilder.Clear();
                                }

                                if (one[0] == '-')
                                {
                                    one = one.Remove(0,1);
                                    store.Push(stringBuilder.AppendFormat("{0}-{1}", two, one).ToString());
                                }
                                else
                                    store.Push(stringBuilder.AppendFormat("{0}+{1}", two, one).ToString());
                                break;
                            case "*":
                                if (two.Contains("+") || two.Contains("-"))
                                {
                                    two = stringBuilder.AppendFormat("({0})", two).ToString();
                                    stringBuilder.Clear();
                                }
                                if (one.Contains("+") || one.Contains("-"))
                                {
                                    one = stringBuilder.AppendFormat("({0})", one).ToString();
                                    stringBuilder.Clear();
                                }
                                store.Push(stringBuilder.AppendFormat("{0}*{1}", two, one).ToString());
                                break;
                            case "/":
                                if (two.Contains("+") || two.Contains("-"))
                                {
                                    two = stringBuilder.AppendFormat("({0})", two).ToString();
                                    stringBuilder.Clear();
                                }
                                if (one.Contains("+") || one.Contains("-"))
                                {
                                    one = stringBuilder.AppendFormat("({0})", one).ToString();
                                    stringBuilder.Clear();
                                }
                                store.Push(stringBuilder.AppendFormat("{0}/{1}", two, one).ToString());
                                break;
                            case "^":
                                if (one.Contains("+") || one.Contains("-"))
                                {
                                    one = stringBuilder.AppendFormat("({0})", one).ToString();
                                    stringBuilder.Clear();
                                }
                                if (two.Contains("+") || two.Contains("-"))
                                {
                                    two = stringBuilder.AppendFormat("({0})", two).ToString();
                                    stringBuilder.Clear();
                                }
                                store.Push(stringBuilder.AppendFormat("{0}^{1}", two, one).ToString());
                                stringBuilder.Clear();
                                break;
                        }
                        stringBuilder.Clear();
                    }
                    else if (elements[i] != "")
                    {
                        if (store.Count < 1)
                            throw new EquasionException("Odwrotna notacja którą podałeś jest nie poprawna nie podałeś w odpowiedniej kolejności liczb i wyrażeń lub kótrychś elementów jest za dużo, błąd znajduje się na pozycji: " + (i+1)  + " jest nim: " + elements[i]);

                        one = store.Pop();

                        switch (elements[i])
                        {
                            case "cos": store.Push(stringBuilder.AppendFormat("cos({0})", one).ToString()); break;
                            case "sin": store.Push(stringBuilder.AppendFormat("sin({0})", one).ToString()); break;
                            case "abs": store.Push(stringBuilder.AppendFormat("abs({0})", one).ToString()); break;
                            case "tan": store.Push(stringBuilder.AppendFormat("tan({0})", one).ToString()); break;
                            case "exp": store.Push(stringBuilder.AppendFormat("exp({0})", one).ToString()); break;
                            case "log": store.Push(stringBuilder.AppendFormat("log({0})", one).ToString()); break;
                            case "sqrt": store.Push(stringBuilder.AppendFormat("sqrt({0})", one).ToString()); break;
                            case "cosh": store.Push(stringBuilder.AppendFormat("cosh({0})", one).ToString()); break;
                            case "sinh": store.Push(stringBuilder.AppendFormat("sinh({0})", one).ToString()); break;
                            case "tanh": store.Push(stringBuilder.AppendFormat("tanh({0})", one).ToString()); break;
                            case "asin": store.Push(stringBuilder.AppendFormat("asin({0})", one).ToString()); break;
                            case "acos": store.Push(stringBuilder.AppendFormat("acos({0})", one).ToString()); break;
                            case "atan": store.Push(stringBuilder.AppendFormat("atan({0})", one).ToString()); break;

                            case "-cos": store.Push(stringBuilder.AppendFormat("-cos({0})", one).ToString()); break;
                            case "-sin": store.Push(stringBuilder.AppendFormat("-sin({0})", one).ToString()); break;
                            case "-abs": store.Push(stringBuilder.AppendFormat("-abs({0})", one).ToString()); break;
                            case "-tan": store.Push(stringBuilder.AppendFormat("-tan({0})", one).ToString()); break;
                            case "-exp": store.Push(stringBuilder.AppendFormat("-exp({0})", one).ToString()); break;
                            case "-log": store.Push(stringBuilder.AppendFormat("-log({0})", one).ToString()); break;
                            case "-sqrt": store.Push(stringBuilder.AppendFormat("-sqrt({0})", one).ToString()); break;
                            case "-cosh": store.Push(stringBuilder.AppendFormat("-cosh({0})", one).ToString()); break;
                            case "-sinh": store.Push(stringBuilder.AppendFormat("-sinh({0})", one).ToString()); break;
                            case "-tanh": store.Push(stringBuilder.AppendFormat("-tanh({0})", one).ToString()); break;
                            case "-asin": store.Push(stringBuilder.AppendFormat("-asin({0})", one).ToString()); break;
                            case "-acos": store.Push(stringBuilder.AppendFormat("-acos({0})", one).ToString()); break;
                            case "-atan": store.Push(stringBuilder.AppendFormat("-atan({0})", one).ToString()); break;

                        }
                        stringBuilder.Clear();
                    }
                }
                if(store.Count > 1)
                    throw new EquasionException("Brakuje znaku lub wyrażenia aby utwożyć równanie ze wszystkich podanych parametrów");               
                return store.Pop().Replace(',', '.');
            }
            throw new EquasionException("Nie podałeś żadnego elementu");
        }
       
        string CalculateSigns(double one, double two, string signs)
        {
            switch (signs)
            {
                case "-":
                    return (one - two).ToString();            
                case "+":
                    return (one + two).ToString();              
                case "*":
                    return (one * two).ToString(); 
                case "/":
                    if (two == 0)
                        throw new EquasionException("Nie możesz dzielić przez 0");
                    return (one / two).ToString();       
            }
            return "";
        }
        string CalculateSinCos(double one, string word)
        {
            if (word.Contains("-"))
            {
                one *= -1;
                word = word.Substring(1, word.Length - 1);
            }
            switch (word)
            {
                case "cos":
                    return (Math.Cos(one)).ToString();
                case "sin":
                    return (Math.Sin(one)).ToString();
                case "abs":
                    return (Math.Abs(one)).ToString();
                case "tan":
                    return (Math.Tan(one)).ToString();
                case "exp":
                    return (Math.Exp(one)).ToString();
                case "log":
                    return (Math.Log(one)).ToString();
                case "sqrt":
                    return (Math.Sqrt(one)).ToString();
                case "cosh":
                    return (Math.Cosh(one)).ToString();
                case "sinh":
                    return (Math.Sinh(one)).ToString();
                case "tanh":
                    return (Math.Tanh(one)).ToString();
                case "asin":
                    if (one > 1 || one < -1)
                        throw new EquasionException("wartość w asin() przekracza dziedzinę funkcji którą jest zakres od -1 do 1");
                    return (Math.Asin(one)).ToString();
                case "acos":
                    if (one > 1 || one < -1)
                        throw new EquasionException("wartość w acos() przekracza dziedzinę funkcji którą jest zakres od -1 do 1");
                    return (Math.Acos(one)).ToString();
                case "atan":
                    if (one > 1 || one < -1)
                        throw new EquasionException("wartość w atan() przekracza dziedzinę funkcji którą jest zakres od -1 do 1");
                    return (Math.Atan(one)).ToString();
            }
            return "";
        }
    }
}
