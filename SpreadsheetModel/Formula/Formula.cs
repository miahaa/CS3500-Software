// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens

/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [02/01/2024]
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and [Thu Ha] - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, [Thu Ha], certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
/// This class evaluates integer arithmetic expressions written using
/// standard infix notation (following precedence rules and
/// integer arithmetic).
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        //Create variables and made it immutable
        private readonly string formula;
        private readonly Func<string, string> normalize ;
        private readonly Func<string, bool> isValid;

        private List<String> substrings;
        private HashSet<String> normalizedVariables;
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
            if (formula is null)
                throw new FormulaFormatException("The formula is null");
            formula = formula.Trim();
            substrings = new List<String>(GetTokens(formula));
            this.normalize = normalize;
            this.isValid = isValid;

            FormulaParser();
        }


        /// <summary>
        /// Helper method to check the valid of formula
        /// </summary>
        private void FormulaParser()
        {
            if (substrings.Count == 0)
                throw new FormulaFormatException("No token passed in.");
            int openingParenthesis = 0; //Handling the number of opening parenthesis
            int closingParenthesis = 0; // Handling number of closing parenthesis

            // Check the validity of formula
            // If the formula does not start with a number, a variable or "(" then throw exception
            if (!isVariable(substrings[0]))
            {
                if (!isDouble(substrings[0]))
                {
                    if (!substrings[0].Equals("("))
                    {
                        throw new FormulaFormatException("The formula should begin with a number, a variable or openning parenthesis");
                    }
                }
            }

            // If the formula does not end with a number, a variable or ")" then throw exception
            if (!isVariable(substrings[substrings.Count - 1]))
            {
                if (!isDouble(substrings[substrings.Count - 1]))
                {
                    if (!substrings[substrings.Count - 1].Equals(")"))
                    {
                        throw new FormulaFormatException("The formula should end with a number, a variable or closing parenthesis");
                    }
                }
            }

            // Loop to check further parsing rules
            for (int i = 0; i < substrings.Count; i++)
            {
                // Checking Parentheis/Operator following rules
                if (substrings[i].Equals("(") || isOperator(substrings[i]))
                {
                    if (i + 1 < substrings.Count)
                    {
                        if (substrings[i + 1] != "(" && !isDouble(substrings[i + 1]) && !isVariable(substrings[i + 1]))
                        {
                            throw new FormulaFormatException("Token that immediately follows an opening parenthesis or an operator must be either " +
                                "a number, a variable, or an opening parenthesis.");
                        }
                    }
                }

                //Checking extra following rule
                if (substrings[i].Equals(")") || isDouble(substrings[i]) || isVariable(substrings[i]))
                {
                    if (i + 1 < substrings.Count)
                    {
                        if (substrings[i + 1] != ")" && !isOperator(substrings[i + 1]))
                        {
                            throw new FormulaFormatException("Token that immediately follows a number, a variable or a closing parenthesis must be " +
                                "either an operator or a closing parenthesis.");
                        }
                    }
                }

                // Check rightparentheses rules.
                if (substrings[i].Equals("("))
                {
                    openingParenthesis++;
                }

                // Counting and checking balanced parentheses
                if (substrings[i].Equals(")"))
                {
                    closingParenthesis++;
                    if (closingParenthesis > openingParenthesis)
                    {
                        throw new FormulaFormatException("There are more right parantheses than left");
                    }
                }
            }

            if (!openingParenthesis.Equals(closingParenthesis))
                throw new FormulaFormatException("Balanced parentheses rule as the total of number parentheses must equal the total number of closing parentheses.");

            // Normalize
            normalizedVariables = new HashSet<string>();
            // Create a temp to check the normalize token is valid or not
            string temp;
            foreach (string token in substrings)
            {
                if (isVariable(token))
                {
                    temp = normalize(token);
                    if (!isVariable(temp) || !isValid(temp))
                        throw new FormulaFormatException("The normalized token is not a valid variable");
                    normalizedVariables.Add(temp);
                }
            }
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
            // Initialize stacks for values and operators
            Stack<double> value = new Stack<double>();
            Stack<string> operators = new Stack<string>();

            // Variables to track the state of the algorithm
            Boolean isFirstToken = true;
            double curr = 0;

            //Process tokens from left to right
            for (int i = 0; i < substrings.Count; i++)
            {

                // Ignore spaces
                Double.TryParse(substrings[i].Trim(), out curr);

                if (Double.TryParse(substrings[i], out curr))  //parse the double type string to curr
                {

                    if (isFirstToken) //always push when t is a number and be the
                                      //first token to avoid possible errors.
                    {
                        value.Push(curr);
                        isFirstToken = false;
                    }

                    // Handling with method that might throw Divide by 0 exception
                    else if (!isFirstToken && operators.Count != 0)
                    {

                        if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                        {
                            try
                            {
                                value.Push(curr);
                                value.Push(Calculating(value, operators));
                            }
                            catch (ArgumentException)
                            {
                                return new FormulaError("Divide by 0");
                            }
                        }
                        else value.Push(curr);
                    }
                }

                // t is a variable
                else if (isVariable(substrings[i]))
                {
                    try
                    {
                        curr = lookup(substrings[i]); //Lookup value of this token
                                                      //Do the same as above
                    }
                    catch (Exception)
                    {
                        return new FormulaError("The look up variable might be null");
                    }

                    if (isFirstToken)
                    {
                        value.Push(curr);
                        isFirstToken = false;
                    }
                    else if (!isFirstToken && operators.Count != 0)
                    {
                        if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                        {
                            try
                            {
                                value.Push(curr);
                                value.Push(Calculating(value, operators));
                            }
                            catch (ArgumentException)
                            {
                                return new FormulaError("Divide by 0");
                            }
                        }
                        else value.Push(curr);
                    }
                }

                // t is * or /
                else if (substrings[i].Equals("*") || substrings[i].Equals("/"))
                {
                    operators.Push(substrings[i]);
                }

                // t is + or -
                else if (substrings[i].Equals("+") || substrings[i].Equals("-"))
                {
                    if (value.Count >= 2 && operators.Count > 0 &&
                        (operators.Peek().Equals("+") || operators.Peek().Equals("-")))
                    {
                        try
                        {
                            value.Push(Calculating(value, operators));
                        }
                        catch (ArgumentException)
                        {
                            return new FormulaError("Divide by 0");
                        }
                    }
                    operators.Push(substrings[i]);
                }

                // Handle parenthesis
                else if (substrings[i].Equals("("))
                {
                    // Counting the number of leftparenthesis in the expression as 
                    // leftParenthesis are pushed into stack and always appeared before rightparenthesis
                    // so if leftparenthesis is less than right then we should throw exception.
                    operators.Push(substrings[i]);
                }

                // Also counting the number of rightparenthesis, and whenever token equals ")", we start pop 
                // operator from operator stack until token euals "(" to calculate the expression inside parenthesis
                else if (substrings[i].Equals(")"))
                {
                    //Fixing test fail, check size of stack before trying to pop
                    if (operators.Count > 0)
                    {
                        if (operators.Peek() != "(" && value.Count > 1)
                            value.Push(Calculating(value, operators));
                        operators.Pop();
                        //Check "*' or "/" operator after popping
                        if (operators.Count > 0 && (operators.Peek().Equals("*") || operators.Peek().Equals("/")))
                            try
                            {
                                value.Push(Calculating(value, operators));
                            }
                            catch (ArgumentException)
                            {
                                return new FormulaError("Divide by 0");
                            }
                    }
                }
            }

            // Handle remaining values and operators after the loop and return final result
            if (operators.Count == 0 && value.Count == 1)
                return value.Pop();
            else
            {
                // This case just returns "+" or "-"
                value.Push(Calculating(value, operators));
                return value.Pop();
            }
        }

        /// <summary>
        /// A helper method calculating each expression
        /// </summary>
        /// <param name="value">a stack of values</param>
        /// <param name="operators">stack of operators</param>
        /// <returns>The result after calculating the expression</returns>
        private static Double Calculating(Stack<Double> value, Stack<string> operators)
        {
            // The method pops values and operators from stacks and performs the corresponding calculation.
            // It supports 4 basic arithmetic operations: +, -, *, /
            // Throws an exception for division by zero or invalid operators.
            Double val = value.Pop();
            Double val2 = value.Pop();
            string op = operators.Pop();
            switch (op)
            {
                case "+":
                    return val + val2;
                case "-":
                    return val2 - val;
                case "*":
                    return val * val2;
                case "/":
                    if (val == 0) throw new ArgumentException("Divide by 0");
                    return val2 / val;
                default:
                    throw new FormulaFormatException("Invalid Operator");

            }
        }

        /// <summary>
        /// Checks if the given token is a variable, which has one or more letters
        /// followed by one or more digits.
        /// </summary>
        /// <param name="token">name of the token to be checked</param>
        /// <returns>True if the token is a variable, otherwise falsereturns>
        private static Boolean isVariable(string token)
        {
            string pattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            return Regex.IsMatch(token, pattern);
        }

        /// <summary>
        /// Check the token is a valid operators or not
        /// </summary>
        /// <param name="token"> checked token</param>
        /// <returns></returns>
        private static Boolean isOperator(string token)
        {
            if (token.Equals("+") || token.Equals("-") || token.Equals("*") || token.Equals("/"))
                return true;
            return false;
        }

        /// <summary>
        /// Chcek the token is a valid double or float point number or not
        /// </summary>
        /// <param name="token"> checked token</param>
        /// <returns></returns>
        private static Boolean isDouble(string token)
        {
            if (Double.TryParse(token, out double result))
                return true;
            return false;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return new HashSet<String>(normalizedVariables);
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
            StringBuilder result = new StringBuilder();

            foreach (string token in substrings)
            {
                if (token.Equals(" "))
                    continue;
                
                result.Append(token);
            }
            return result.ToString();
        }



        /// <summary>
        ///
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
            if (!(obj is Formula) || obj == null) return false;

            Formula comparedFormula = (Formula)obj;
            for (int i = 0; i < substrings.Count; i++)
            {
                String token = substrings[i];
                String comparedToken = comparedFormula.substrings[i];

                // if it is numeric token
                if (isDouble(token) && isDouble(comparedToken))
                {
                    Double result1 = Double.Parse(token);
                    Double result2 = Double.Parse(comparedToken);
                    if (result1 != result2)
                        return false;
                }

                // token is variabke or operators
                else
                {
                    if (!normalize(token).Equals(comparedFormula.normalize(comparedToken)))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// </summary>
        /// <param name="f1"> first formula to be compared</param>
        /// <param name="f2"> second formula to be compared</param>
        /// <returns> true if f1 == f2 or returns false if not</returns>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }

        /// <summary>
        ///   Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// </summary>
        /// <param name="f1"> first formula to be compared</param>
        /// <param name="f2"> second formula to be compared</param>
        /// <returns> true if f1 != f2 or returns false if f1 == f2</returns>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1 == f2)
                return false;
            return true;
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < substrings.Count; i++)
            {
                if (Double.TryParse(substrings[i], out Double result))
                    hash *= 23 + result.GetHashCode();
                else if (isVariable(substrings[i]))
                    hash *= 23 + normalize(substrings[i]).GetHashCode();
                else
                    hash *= 23 + substrings[i].GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
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
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

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



