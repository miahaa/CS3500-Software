/// <summary>
/// Author:    Amber (Phuong) Tran
/// Partner:   -none-
/// Date:      10-Jan-2023
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
///    A method that evaluates integer arithmetic expressions written using standard infix notation
///    
/// </summary>
///


using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public class Evaluator
    {
        /// <summary>
        /// delegate points to lookup function to find the value of variable.
        /// </summary>
        /// <param name="string">variable_name</param>
        /// <returns> int value of variable </returns>
        public delegate int Lookup(string variable_name);

        /// <summary>
        /// Check if variable is valid
        /// </summary>
        /// <param name="string">varToken</param>
        /// <returns>boolean</returns>
        public static bool IsVariableValid(string varToken)
        {
            if (Regex.IsMatch(varToken, "^[a-zA-Z]+[0-9]+$"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Method to execute the algorithm if input has int value. In the future, extension is considered to replace this method
        /// </summary>
        /// <param name="Stack<string>">operators</param>
        /// <param name="Stack<int>">values</param>
        /// <param name="int">intValue</param>
        /// <returns>void</returns>
        public static void AlgoForInt(Stack<string> operators, Stack<int> values, int intValue)
        {
            if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
            {
                if (values.Count != 0)
                {
                    string op = operators.Pop();
                    int operand = values.Pop();
                    try
                    {
                        if (op == "*")
                        {
                            values.Push(intValue * operand);
                        }
                        else
                        {
                            if (intValue != 0)
                            {
                                values.Push(operand / intValue);
                            }
                            else
                            {
                                throw new DivideByZeroException("Division by zero.");
                            }
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        throw new ArgumentException("Division by zero.");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid expression.");
                }
            }
            else
            {
                values.Push(intValue);
            }
        }


        /// <summary>
        /// Evaluate value of expression
        /// </summary>
        /// <param name="string">expression</param>
        /// <param name="method">variableEvaluator</param>
        /// <returns> int value of the expression</returns>
        public static int Evaluate(string expression, Lookup variableEvaluator)
        {
            // Initialize stacks for values and operators
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();

            //Remove white space and split expression into tokens
            expression = expression.Replace(" ", "");
            string[] tokens =
                Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            foreach (string token in tokens)
            {
                if (token == "" || token == " ")
                    continue;
                else if (int.TryParse(token, out int intValue))
                {

                    AlgoForInt(operators, values, intValue);

                }

                else if (IsVariableValid(token))
                {

                    int variableValue = variableEvaluator(token);
                    AlgoForInt(operators, values, variableValue);
                }

                else if (token == "+" || token == "-")
                {

                    if (operators.Count > 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                    {
                        if (values.Count >= 2)
                        {
                            string op = operators.Pop();
                            int rightOperand = values.Pop();
                            int leftOperand = values.Pop();
                            if (op == "+")
                            {
                                values.Push(leftOperand + rightOperand);
                            }
                            else
                            {
                                values.Push(leftOperand - rightOperand);
                            }

                        }
                        else throw new ArgumentException("Invalid expression.");

                    }
                    operators.Push(token);

                }

                else if (token == "*" || token == "/" || token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    if (operators.Count > 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                    {
                        if (values.Count >= 2)
                        {
                            string op = operators.Pop();
                            int rightOperand = values.Pop();
                            int leftOperand = values.Pop();
                            if (op == "+")
                            {
                                values.Push(leftOperand + rightOperand);
                            }
                            else
                            {
                                values.Push(leftOperand - rightOperand);
                            }
                        }
                        else throw new ArgumentException("Invalid expression.");
                    }

                    if (operators.Count > 0 && operators.Peek() == "(")
                    {
                        operators.Pop();
                    }
                    else throw new ArgumentException("Missmatch parentheses 4.");
                    if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                    {
                        if (values.Count >= 2)
                        {
                            string op = operators.Pop();
                            int operand2 = values.Pop();
                            int operand1 = values.Pop();
                            if (op == "*")
                            {
                                values.Push(operand1 * operand2);
                            }
                            else
                            {
                                if (operand2 != 0)
                                {
                                    values.Push(operand1 / operand2);
                                }
                                else throw new DivideByZeroException("Division by zero.");
                            }
                        }
                        else throw new ArgumentException("Invalid expression.");
                    }


                }
                else throw new ArgumentException("Invalid expression 5");

            }

            if (operators.Count == 0)
            {
                if (values.Count == 1)
                {
                    return values.Pop();
                }
                else
                {
                    throw new ArgumentException("Invalid expression: Value stack should contain a single number.");
                }
            }
            else if (operators.Count == 1 && (operators.Peek() == "+" || operators.Peek() == "-") && values.Count == 2)
            {
                string op = operators.Pop();
                int rightOperand = values.Pop();
                int leftOperand = values.Pop();

                if (op == "+")
                {
                    return leftOperand + rightOperand;
                }
                else if (op == "-")
                {
                    return leftOperand - rightOperand;
                }
                else
                {
                    throw new ArgumentException("Invalid expression: The top operator is not + or -.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid expression: Operator stack doesn't match the expected conditions.");
            }

        }

    }
}

