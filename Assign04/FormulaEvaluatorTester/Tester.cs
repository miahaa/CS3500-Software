/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [01/14/2024]
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
/// [This file contains test cases for the FormulaEvaluator class.]
/// </summary>
using System;
using static FormulaEvaluator.Evaluator;
using FormulaEvaluator;

namespace FormulaEvaluatorTester
{
    /// <summary>
    /// The FormulaEvaluatorTest class contains various test cases to ensure
    /// the correctness of the FormulaEvaluator class.
    /// </summary>
    class FormulaEvaluatorTest
    {
        /// <summary>
        /// A helper method for testing expressions with expected results.
        /// </summary>
        /// <param name="expression">The arithmetic expression to testparam>
        /// <param name="variableEvaluator">Delegate to look up the value of variables</param>
        /// <param name="expected">The expected result of the expression</param>
        /// <param name="testName">A descriptive name for the test case</param>
        public static void test(string expression, Lookup variableEvaluator, int expected, string testName)
        {
            try
            {
                int result = Evaluator.Evaluate(expression, variableEvaluator);
                if (result == expected)
                {
                    Console.WriteLine(testName + "passed.");
                }
                else
                {
                    Console.WriteLine(testName + "failed. Expected throws " + expected + ", Actual: " + result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(testName + "failed. Exception throws: " + e.Message);
            }
        }

        /// <summary>
        /// The main method contains various test cases to ensure the correctness of
        /// the FormulaEvaluator class.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static void Main()
        {
            //Test single number
            test("8", s => 0, 8, "Test single number: ");

            //Test simple "+"
            test("8+7", s => 0, 15, "Test simple addition: ");

            //Test simple "-"
            test("10-6", s => 0, 4, "Test simple subtraction: ");

            //Test simple "*"
            test("2*3", s => 0, 6, "Test simple multiplication: ");

            //Test illegal "/"
            test("0/0", s => 0, 0, "Test division exception: ");

            //Test simple "/"
            test("10/5", s => 0, 2, "Test simple division: ");

            //Test with variable
            test("x1", s => 25, 25, "Test variable: ");

            //Test simple arithmetic with variable
            test("x1 + 5", s => 17, 22, "Test simple arithmetic with variable: ");

            //Test infix evaluation
            test("15*2+4", s => 0, 34, "Test infix: ");

            //Test operator handling
            test("2+6/3", s => 0, 4, "Test operator handling: ");

            //Test parenthesis
            test("2*(3+5)", s => 0, 16, "Test parenthesis: ");

            //Test parenthesis and operator handling
            test("2+3*(10-4)", s => 0, 20, "Test parenthesis and operator handling: ");

            //Test complex1
            test("2+(10-4*5+20)-(2+3)*4", s => 0, -8, "Test complex1: ");

            //Test complex2
            test("4+8-7*5+(3*10-8+6)*2+4/2", s => 0, 35, "Test complex2: ");

            //Test exception single operator
            test("*", s => 0, 0, "Test single operator exception: ");

            //Test exception illegal number of operators
            test("2+3*", s => 0, 0, "Test illegal number of operators: ");

            //Test exception parenthesis
            test("2-3)", s => 0, 0, "Test exception parenthesis: ");

            //Test complex arithmetic with variable
            test("x1+3*7-4+x1-5*(x1+3)-4", s => 2, -8, "Test complex arithmetic with variable: ");

            //Test exception no-value variable
            test("x1", s => { throw new ArgumentException("Unknown variable"); }, 0, "Test no-value variable: ");

            //Test multi parenthesis
            test("((((v2*v3)*v4)+v1)-v5)/v6", s => 2, 4, "Test multi parenthesis: ");

            //Test complex multi variables
            test("2+4*x1/2+(x1+4)*x2", s => 2, 18, "Test complex multi variables: ");

            //Test more complex variable
            test("x1+x2*(x3-x4)+x5/x6", s => s switch { "x1" => 2, "x2" => 3, "x3" => 12, "x4" => 10, "x5" => 2, "x6" => 1, _ => throw new NotImplementedException() },
                10, "Test more complex variables: ");

            //Test negative numbers
            test("-2+4-6", s => 0, 0, "Test negative numbers: ");

            //Test decimal number
            test("1.5 * 2", s => 0, 0, "Test decimal number: ");

            //Test long variable name
            test("x1x2 + x3x4", s => 0, 0, "Test long variable name: ");

            //Test repeated operator
            test("2**3", s => 0, 0, "Test repeated operators: ");

            //Test arithmetic with invalid variable
            test("5+xx", s => { throw new ArgumentException("Unknown variable"); }, 0, "Test arithmetic with invalid variable: ");

            //Test null expression
            test("", s => 0, 0, "Test null expression: ");
        }
    }
}

