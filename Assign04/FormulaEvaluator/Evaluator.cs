/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [01/11/2024]
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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace FormulaEvaluator
{
    /// <summary>
    /// The Evaluator class provides methods to evaluate integer arithmetic
    /// expression written in standard infix notation, following precedence rules and integer arithmetic.
    /// </summary>   

    public class Evaluator
    {
        /// <summary>
        /// A delegate that helps look up the value of input variable
        /// </summary>
        /// <param name="variable_name">The name of the variable to look up</param>
        /// <returns>The value of the variable</returns>
        public delegate int Lookup(string variable_name);

        /// <summary>
        /// Evaluates the given arithmetic expression and returns the result.
        /// </summary>
        /// <param name="expression">The arithmetic expression to evaluateparam>
        /// <param name="variableEvaluator">Delegate to look up the value of variables</param>
        /// <returns>The result of the arithmetic expressionreturns>
        /// <exception cref="ArgumentException">Thrown for various parsing and evaluation errors</exception>
        public static int Evaluate(string expression,
                                   Lookup variableEvaluator)

        {
            // Initialize stacks for values and operators
            Stack<int> value = new Stack<int>();
            Stack<string> operators = new Stack<string>();

            //Break the expression into a sequence of tokens
            string[] substrings =
            Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // Variables to track the state of the algorithm
            Boolean isFirstToken = true;
            int leftParenthesis = 0; //Handling the number of left parenthesis
            int rightParenthesis = 0; // Handling number of right parenthesis
            int curr = 0;

            // Check for null or empty expression
            if (expression == null || expression == "")
            {
                throw new ArgumentException("The expression is null or empty");
            }

            //Process tokens from left to right
            for (int i = 0; i < substrings.Length; i++)
            {

                // Ignore spaces
                if (substrings[i].Equals(""))
                    continue;

                // I tried to find the best way to check if the token is an interger
                // or not and this is what CodeMaze told me to do.
                if (int.TryParse(substrings[i], out curr))  //parse the int type string to curr
                {

                    if (isFirstToken) //always push when t is a number and be the
                                      //first token to avoid possible errors.
                    {
                        value.Push(curr);
                        isFirstToken = false;
                    }

                    else if (!isFirstToken && operators.Count != 0)
                    {
                        if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                        {
                            value.Push(curr);
                            value.Push(Calculating(value, operators));
                        }
                        else value.Push(curr);
                    }
                }

                // t is a variable
                else if (isVariable(substrings[i]))
                {
                    curr = variableEvaluator(substrings[i]); //Lookup value of this token
                    //Do the same as above
                    if (isFirstToken)
                    {
                        value.Push(curr);
                        isFirstToken = false;
                    }
                    else if (!isFirstToken && operators.Count != 0)
                    {
                        if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                        {
                            value.Push(curr);
                            value.Push(Calculating(value, operators));
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
                    if (value.Count >= 2 && operators.Count >= 1 &&
                        (operators.Peek().Equals("+") || operators.Peek().Equals("-")))
                    {
                        value.Push(Calculating(value, operators));
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
                    leftParenthesis++;
                    if (leftParenthesis < rightParenthesis)
                        throw new ArgumentException("Missing parenthesis");
                }

                // Also counting the number of rightparenthesis, and whenever token equals ")", we start pop 
                // operator from operator stack until token euals "(" to calculate the expression inside parenthesis
                else if (substrings[i].Equals(")"))
                {
                    rightParenthesis++;
                    // Fixing test failed
                    if (leftParenthesis < rightParenthesis)
                        throw new ArgumentException("Extra parenthesis");
                    //Fixing test fail, check size of stack before trying to pop
                    if (operators.Count > 0)
                    {
                        if (operators.Peek() != "(" && value.Count > 1)
                            value.Push(Calculating(value, operators));
                        operators.Pop();
                        //Check "*' or "/" operator after popping
                        if (value.Count > 1 && (operators.Peek().Equals("*") || operators.Peek().Equals("/")))
                            value.Push(Calculating(value, operators));
                    }
                    else
                    {
                        throw new ArgumentException("Extra parenthesis");
                    }
                }
            }
            // Handle remaining values and operators after the loop and return final result
            if (operators.Count == 0 && value.Count == 1)
                return value.Pop();
            else if (value.Count == 2 && operators.Count == 1)
            {
                value.Push(Calculating(value, operators));
                return value.Pop();
            }
            else
                throw new ArgumentException("Bad Token");
        }

        /// <summary>
        /// A helper method calculating each expression
        /// </summary>
        /// <param name="value">a stack of values</param>
        /// <param name="operators">stack of operators</param>
        /// <returns>The result after calculating the expression</returns>
        private static int Calculating(Stack<int> value, Stack<string> operators)
        {
            // The method pops values and operators from stacks and performs the corresponding calculation.
            // It supports 4 basic arithmetic operations: +, -, *, /
            // Throws an exception for division by zero or invalid operators.
            int val = value.Pop();
            int val2 = value.Pop();
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
                    throw new ArgumentException("Invalid operator");
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
            string pattern = @"[a-zA-Z]+\d+";
            return Regex.IsMatch(token, pattern);
        }
    }
}
