
/// <summary>
/// Author:    Amber (Phuong) Tran
/// Partner:   -none-
/// Date:      4-Feb-2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Phuong Tran - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Amber Tran, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
///
///    class Formula: a partially implemented class called Formula.cs. You will copy over and modify your Assignment One code to generalize your infix expression evaluator. 
///    class FormulaFormatException : Used to report syntactic errors in a formula
///    struct FormulaError : A return value from the the formula evaluator (i.e., when the formula is bad).
///    
/// </summary>
///
/// revised
using System.Text.RegularExpressions;
namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private string formula;
        private Func<string, string> normalize;
        private Func<string, bool> isValid;
        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
        this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            FormulaSyntacCheck(GetTokens(formula).ToArray(), normalize, isValid);
            this.formula = Regex.Replace(formula, @"\s+", "");
            this.normalize = normalize;
            this.isValid = isValid;
        }

        /// <summary>
        /// Evaluates a given formula to match the syntactical definition of a formula
        /// </summary>
        /// <param name="validTokens"> tokens of the formula </param>
        /// <param name="normalize"> variable function given by the user </param>
        /// <param name="isValid"> variable validity function given by the user </param>
        /// <exception cref="FormulaFormatException"> Thrown when the formula doesn't meet the definition </exception>
        private static void FormulaSyntacCheck(String[] validTokens, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (validTokens.Length > 0)
            {
                Stack<string> leftPara = new Stack<string>();
                Stack<string> rightPara = new Stack<string>();

                // Checks if the first token and last token are numbers, variables, open parentheses, or closing parentheses
                if (Regex.IsMatch(validTokens.First(),
                    @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)|([a-zA-Z_](?:[a-zA-Z_]|\d)*)|(\()") &&
                    Regex.IsMatch(validTokens.Last(), @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)|([a-zA-Z_](?:[a-zA-Z_]|\d)*)|(\))"))
                {
                    // Checks validity of first token variable
                    if (Regex.IsMatch(validTokens.First(), @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                    {
                        if (!isValid(normalize(validTokens.First()))) { throw new FormulaFormatException("Not Valid Variable"); }
                    }

                    if (Regex.IsMatch(validTokens.First(), @"\("))
                    {
                        leftPara.Push(validTokens.First());
                    }

                    // Checks validity of last token variable
                    if (Regex.IsMatch(validTokens.Last(), @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                    {
                        if (!isValid(normalize(validTokens.Last()))) { throw new FormulaFormatException("Not Valid Variable"); }
                    }

                    if (Regex.IsMatch(validTokens.Last(), @"\)"))
                    {
                        rightPara.Push(validTokens.Last());
                    }

                    // Checks all tokens in between first and last token
                    for (int i = 1; i < validTokens.Length - 2; i++)
                    {
                        if (Regex.IsMatch(validTokens[i], @"([\+\-*/])|(\()"))
                        {
                            if (Regex.IsMatch(validTokens[i], @"\(")) { leftPara.Push(validTokens[i]); }

                            // Checks if the token adjacent to the operator or open parenthesis is a number, variable, or open parenthesis
                            if (Regex.IsMatch(validTokens[i + 1], @"(\d+\.\d*)|(\d*\.\d+)|(\d+)|([eE][\+-]\d+)|([a-zA-Z_](?:[a-zA-Z_]|\d)*)|(\()"))
                            {
                                // Checks validity of adjacent variable
                                if (Regex.IsMatch(validTokens[i + 1], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                                {
                                    if (!isValid(normalize(validTokens[i + 1]))) { throw new FormulaFormatException("Not Valid Variable"); }

                                    else { continue; }
                                }

                                else { continue; }
                            }

                            else { throw new FormulaFormatException("Parenthesis Following Rule Violated"); }
                        }

                        else if (Regex.IsMatch(validTokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)|(\))"))
                        {
                            if (Regex.IsMatch(validTokens[i], @"\)"))
                            {
                                rightPara.Push(validTokens[i]);

                                // Checks if there are more right parentheses than left parentheses
                                if (!(leftPara.Count >= rightPara.Count)) { throw new FormulaFormatException("Right Parenthesis Rule Violated"); }
                            }

                            // Checks if adjacent token is an operator or closing parenthesis
                            if (Regex.IsMatch(validTokens[i + 1], @"([\+\-*/])|(\))")) { continue; }

                            else { throw new FormulaFormatException("Extra Following Rule Violated"); }
                        }

                        else if (Regex.IsMatch(validTokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                        {
                            if (isValid(normalize(validTokens[i])))
                            {
                                // Checks if the adjacent token is an operator or closing parenthesis
                                if (Regex.IsMatch(validTokens[i + 1], @"([\+\-*/])|(\))")) { continue; }

                                else { throw new FormulaFormatException("Extra Following Rule Violated"); }
                            }
                        }
                    }

                    // Checks if the each left parenthesis has a right parenthesis
                    if (!(leftPara.Count == rightPara.Count)) { throw new FormulaFormatException("Balanced Parenthesis Rule Violated"); }
                }

                else { throw new FormulaFormatException("Starting Token or Ending Token Violated"); }
            }

            else { throw new FormulaFormatException("One Token Rule Violated"); }
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            // splits the expression into tokens
            string[] tokens = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();
            foreach (string token in tokens)
            {
                if (token == "")
                {
                    continue;
                }

                // Checks if token is a minus or plus sign
                else if (Regex.IsMatch(token, "(\\-)|(\\+)"))
                {
                    // Checks if we need to do addition or subtraction
                    if (validOperation(operators, values, token))
                    {
                        // Puts the addition or subtraction result into the values stack
                        values.Push(MathOperations(values.Pop(), operators.Pop(), values.Pop()));
                        operators.Push(token);
                    }

                    else
                        operators.Push(token);
                }

                // Checks if the token is a left parenthesis, multiplication sign, or division sign
                else if (Regex.IsMatch(token, "(\\()|(\\*)|(/)"))
                {
                    operators.Push(token);
                }

                // Checks if the token is a right parenthesis
                else if (Regex.IsMatch(token, "(\\))"))
                {
                    // Checks if we need to do addition or subtraction
                    if (operators.Count > 0 && values.Count >= 2 && (operators.Peek() == "+" || operators.Peek() == "-"))
                        values.Push(MathOperations(values.Pop(), operators.Pop(), values.Pop()));

                    // Removes left parenthesis from the stack
                    operators.Pop();

                    // Checks if we need to do multiplication or division
                    if (operators.Count > 0 && values.Count >= 1 && (operators.Peek() == "*" || operators.Peek() == "/"))
                    {
                        if (values.Peek() == 0)
                            return new FormulaError("Can't divide by zero");

                        values.Push(MathOperations(values.Pop(), operators.Pop(), values.Pop()));
                    }
                }

                // Checks if the token is a digit
                else if (Regex.IsMatch(token, @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)"))
                {
                    // Checks if we need to do multiplication or division
                    if (validOperation(operators, values, token))
                    {
                        if (double.Parse(token) == 0)
                            return new FormulaError("Can't divide by zero");

                        values.Push(MathOperations(double.Parse(token), operators.Pop(), values.Pop()));
                    }

                    else
                    {
                        // Converts token from string to integer and adds to the values stack
                        values.Push(double.Parse(token));
                    }
                }

                // Looks up the variable and goes through digit process above
                else if (Regex.IsMatch(token, @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                {
                    double variable = 0;
                    try
                    {
                        variable = lookup(normalize(token));
                    }
                    catch (Exception e)
                    {
                        return new FormulaError("Cannot find variable");
                    }

                    if (variable < 0)
                        return new FormulaError("Invalid Variable");

                    // Checks if we need to do multiplication or division
                    if (validOperation(operators, values, variable.ToString()))
                    {
                        if (variable == 0)
                            return new FormulaError("Can't divide by zero");

                        values.Push(MathOperations(variable, operators.Pop(), values.Pop()));
                    }
                    else
                        values.Push(variable);
                }
            }

            // Checks if we need to do one more calculation
            if (operators.Count == 1 && values.Count == 2)
                return MathOperations(values.Pop(), operators.Pop(), values.Pop());

            else
                return values.Pop();
        }

        /// <summary>
        /// Executes the specified math operation
        /// </summary>
        /// <param name="right"> right represents the right operand of the math operation </param>
        /// <param name="oper"> oper represents the operator to execute in the math operation </param>
        /// <param name="left"> left represents the left operand of the math operation </param>
        /// <returns> Result of the specified math operation </returns>
        private static double MathOperations(double right, string oper, double left)
        {
            double result = 0;
            switch (oper)
            {
                case "+":
                    result = left + right;
                    break;
                case "-":
                    result = left - right;
                    break;
                case "*":
                    result = left * right;
                    break;
                case "/":
                    result = left / right;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Checks if the operator and value stacks have the necessary conditions to execute a math operation
        /// </summary>
        /// <param name="operators"> operators represents the current operator stack to meet the conditions </param>
        /// <param name="values"> values represents the current value stack to meet the conditions </param>
        /// <returns> true if all conditions are met for either math operation, otherwise false </returns>
        private static bool validOperation(Stack<string> operators, Stack<double> values, string token)
        {

            // Checks if we need to do addition or subtraction
            if (operators.Count > 0 && values.Count >= 2 && ((operators.Peek() == "+" || operators.Peek() == "-") && Regex.IsMatch(token, "(\\-)|(\\+)")))
            {
                return true;
            }

            // Checks if we need to do multiplication or division
            else if (operators.Count > 0 && values.Count >= 1 &&
                ((operators.Peek() == "*" || operators.Peek() == "/") && Regex.IsMatch(token, @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)")))
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x +y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            String NotVarPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                @"[a-zA-Z_](?: [a-zA-Z_]|\d)*", @"\(", @"\)", @"[\+\-*/]", @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?", @"\s+");
            foreach (string token in Regex.Split(formula, NotVarPattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (Regex.IsMatch(normalize(token), @"[a-zA-Z_](?:[a-zA-Z_]|\d)*", RegexOptions.Singleline))
                {
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return normalize(formula);
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Formula) { return false; }

            else
            {
                Formula f2 = (Formula)obj;
                List<string> f1Tokens = GetTokens(normalize(this.formula)).ToList();
                List<string> f2Tokens = GetTokens(f2.formula).ToList();

                // Formulas must be equal in length
                if (f1Tokens.Count < f2Tokens.Count || f1Tokens.Count > f2Tokens.Count) { return false; }

                else
                {
                    for (int i = 0; i < f1Tokens.Count; i++)
                    {
                        if (Regex.IsMatch(f1Tokens[i], @"([\+\-*/])|(\))|(\()") && Regex.IsMatch(f2Tokens[i], @"([\+\-*/])|(\))|(\()"))
                        {
                            if (f1Tokens[i] == f2Tokens[i]) { continue; }
                            else { return false; }
                        }

                        if (Regex.IsMatch(f1Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)")
                            && Regex.IsMatch(f2Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)"))
                        {
                            // Checks if numbers are equal
                            if (Double.Parse(f1Tokens[i]).ToString() == Double.Parse(f2Tokens[i]).ToString()) { continue; }

                            else { return false; }
                        }

                        if (Regex.IsMatch(f1Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*") && Regex.IsMatch(f2Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                        {
                            // Checks if normalized variable is equal to the other variable
                            if (normalize(f1Tokens[i]) == f2Tokens[i]) { continue; }
                            else { return false; }
                        }

                        else { return false; }
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            List<string> f1Tokens = GetTokens(f1.formula).ToList();
            List<string> f2Tokens = GetTokens(f2.formula).ToList();
            if (f1Tokens.Count < f2Tokens.Count || f1Tokens.Count > f2Tokens.Count)
            {
                return false;
            }

            else
            {
                for (int i = 0; i < f1Tokens.Count; i++)
                {
                    if (Regex.IsMatch(f1Tokens[i], @"([\+\-*/])|(\))|(\()") && Regex.IsMatch(f2Tokens[i], @"([\+\-*/])|(\))|(\()"))
                    {
                        if (f1Tokens[i] == f2Tokens[i]) { continue; }
                        else { return false; }
                    }

                    if (Regex.IsMatch(f1Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)")
                        && Regex.IsMatch(f2Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)"))
                    {
                        // Checks if numbers are equal
                        if (Double.Parse(f1Tokens[i]).ToString() == Double.Parse(f2Tokens[i]).ToString()) { continue; }

                        else { return false; }
                    }

                    if (Regex.IsMatch(f1Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*") && Regex.IsMatch(f2Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                    {
                        // Checks if normalized variable is equal to the other variable
                        if (f1.normalize(f1Tokens[i]) == f2Tokens[i]) { continue; }
                        else { return false; }
                    }

                    else { return false; }
                }
                return true;
            }
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            List<string> f1Tokens = GetTokens(f1.formula).ToList();
            List<string> f2Tokens = GetTokens(f2.formula).ToList();
            if (f1Tokens.Count < f2Tokens.Count || f1Tokens.Count > f2Tokens.Count) { return true; }

            else
            {
                for (int i = 0; i < f1Tokens.Count; i++)
                {
                    if (Regex.IsMatch(f1Tokens[i], @"([\+\-*/])|(\))|(\()") && Regex.IsMatch(f2Tokens[i], @"([\+\-*/])|(\))|(\()"))
                    {
                        if (f1Tokens[i] != f2Tokens[i]) { return true; }
                        else { continue; }
                    }

                    if (Regex.IsMatch(f1Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)")
                        && Regex.IsMatch(f2Tokens[i], @"(^\d+\.\d*)|(^\d*\.\d+)|(^\d+)|(^[eE][\+-]\d+)"))
                    {
                        if (Double.Parse(f1Tokens[i]).ToString() != Double.Parse(f2Tokens[i]).ToString()) { return true; }

                        else { continue; }
                    }

                    if (Regex.IsMatch(f1Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*") && Regex.IsMatch(f2Tokens[i], @"[a-zA-Z_](?:[a-zA-Z_]|\d)*"))
                    {
                        if (f1.normalize(f1Tokens[i]) != f2Tokens[i]) { return true; }
                        else { continue; }
                    }

                    else { return true; }
                }
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return normalize(formula).GetHashCode() + normalize.GetHashCode() + isValid.GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal;and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";
            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})", lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);
            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}

