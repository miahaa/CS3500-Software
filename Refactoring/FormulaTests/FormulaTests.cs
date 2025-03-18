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
/// [This file contains test cases for the FormulaEvaluator class.]
/// </summary>
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class Testers
    {
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test null formula
        public void TestNullFormula()
        {
            Formula f = new Formula(null, s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test empty formula
        public void TestEmptyFormula()
        {
            Formula f = new Formula("", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testEmptyToken()
        {
            Formula f = new Formula("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void testInvalidBegin()
        {
            Formula f = new Formula("+23*4", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidEnding()
        {
            Formula f = new Formula("2*3(", s => s, s => true);
        }

        [TestMethod]
        public void TestIsValid()
        {
            Formula f = new Formula("2", s => s, s => false);
        }

        [TestMethod]
        public void TestIsValid2()
        {
            Formula f = new Formula("2 + x", s => s, s => (s == "x"));
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test single operator
        public void TestInvalifsingleOp()
        {
            Formula f = new Formula("/", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test extra operators
        public void TestInvalidExtraOp()
        {
            Formula f = new Formula("2+1+", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test unbalanced parentheses
        public void TestInvalidExtraParentheses()
        {
            Formula f = new Formula("((2+1)", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test unbalanced parentheses more closing than opening
        public void TestInvalidExtraParentheses2()
        {
            Formula f = new Formula("(2+1))) * (1+2)", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test extra closing parentheses
        public void TestExtraClosing()
        {
            Formula f = new Formula("2x+1)", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void UnbalancedOpenningParentheses()
        {
            Formula f = new Formula("(((2+3)", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void UnbalancedClosingParentheses()
        {
            Formula f = new Formula("(2+3)))", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test invalid format missing operator
        public void TestInvalidFormat()
        {
            Formula f = new Formula("1 1", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        // Test 2 operators
        public void TestInvalidFormat2()
        {
            Formula f = new Formula("2 ++ 2", s => s, s => true);
        }

        [TestMethod()]
        public void TestEvaluateSingleNumber()
        {
            Formula f = new Formula("5", s => s, s => true);
            Assert.AreEqual(5.0, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestEvaluateSingleVar()
        {
            Formula f = new Formula("x1", s => s, s => true);
            Assert.AreEqual(2.0, f.Evaluate(s => 2));
        }

        [TestMethod]
        public void TestAddition()
        {
            Formula f = new Formula("1 + 1 + 1", s => s, s => true);
            Assert.AreEqual(3.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestSubtraction()
        {
            Formula f = new Formula("2 - 1", s => s, s => true);
            Assert.AreEqual(1.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestMultiplication()
        {
            Formula f = new Formula("1 * 2", s => s, s => true);
            Assert.AreEqual(2.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestDivision()
        {
            Formula f = new Formula("16 / 4", s => s, s => true);
            Assert.AreEqual(4.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void SimpleEvaluateVariable()
        {
            Formula f = new Formula("2 + x1", s => s, s => true);
            Assert.AreEqual(3.0, f.Evaluate(s => 1));
        }

        [TestMethod]
        public void TestLookUpUnknown()
        {
            Formula f = new Formula("x2", s => s, s => true);
            Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }),
                typeof(FormulaError));
        }

        [TestMethod]
        public void TestEvaluateLeftToRight()
        {
            Formula f = new Formula("2*4+7", s => s, s => true);
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestOrderOperations()
        {
            Formula f = new Formula("2+4*7", s => s, s => true);
            Assert.AreEqual(30.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestParenthesesTimes()
        {
            Formula f = new Formula("(2+4)*7", s => s, s => true);
            Assert.AreEqual(42.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestTimesParentheses()
        {
            Formula f = new Formula("2*(4+7)", s => s, s => true);
            Assert.AreEqual(22.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestPlusParentheses()
        {
            Formula f = new Formula("2+(4+7)", s => s, s => true);
            Assert.AreEqual(13.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestPlusComplex()
        {
            Formula f = new Formula("2+(3+5*9)", s => s, s => true);
            Assert.AreEqual(50.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestOperatorAfterParens()
        {
            Formula f = new Formula("(1*1)-2/2", s => s, s => true);
            Assert.AreEqual(0.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestComplexTimesParentheses()
        {
            Formula f = new Formula("2+3*(3+5)", s => s, s => true);
            Assert.AreEqual(26.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestComplexAndParentheses()
        {
            Formula f = new Formula("2+3*5+(3+4*8)*5+2", s => s, s => true);
            Assert.AreEqual(194.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void DivideBy0()
        {
            Formula f = new Formula("2/0", s => s, s => true);
            Object error = f.Evaluate(s => 0);
            Assert.IsTrue(error != null);
        }

        [TestMethod]
        public void DivideBy0Variable()
        {
            Formula f = new Formula("2/x1", s => s, s => true);
            Object error = f.Evaluate(s => 0);
            Assert.IsTrue(error != null);
        }

        [TestMethod]
        public void DivideBy0AfterParentheses()
        {
            Formula f = new Formula("(2+1)/0", s => s, s => true);
            Object error = f.Evaluate(s => 0);
            Assert.IsTrue(error != null);
        }

        [TestMethod]
        public void DivideBy0InsideParentheses()
        {
            Formula f = new Formula("(2/0 + 3) * 2", s => s, s => true);
            Object error = f.Evaluate(s => 0);
            Assert.IsTrue(error != null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidvariable()
        {
            Formula f = new Formula("2xy + 1", s => s, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidOperator()
        {
            Formula f = new Formula("2x # 1", s => s, s => true);
            Assert.AreEqual(0.0, f.Evaluate(s => 0));
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestParensNoOperator()
        {
            Formula f = new Formula("5+7+(5)8", s => s, s => true);
        }

        [TestMethod]
        public void TestComplexMultiVar()
        {
            Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7", s => s, s => true);
            Assert.AreEqual(-3.428571428571429, f.Evaluate(s => 4));
        }

        [TestMethod]
        public void TestComplexNestedParensRight()
        {
            Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))", s => s, s => true);
            Assert.AreEqual(6.0, f.Evaluate(s => 1));
        }

        [TestMethod]
        public void TestComplexNestedParensLeft()
        {
            Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6", s => s, s => true);
            Assert.AreEqual(12.0, f.Evaluate(s => 2));
        }

        [TestMethod]
        public void TestRepeatedVar()
        {
            Formula f = new Formula("a4-a4*a4/a4", s => s, s => true);
            Assert.AreEqual(0.0, f.Evaluate(s => 3));
        }

        [TestMethod]
        public void TestClearStacks()
        {
            Formula f = new Formula("2*6+3", s => s, s => true);
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestNormalizer()
        {
            Formula f = new Formula("x1", s => s, s => true);
            HashSet<string> list = new HashSet<string>(f.GetVariables());
            Assert.IsFalse(list.SetEquals(new HashSet<string> { "X1" }));
            Assert.IsTrue(list.SetEquals(new HashSet<string> { "x1" }));
        }

        [TestMethod]
        public void TestToStringAndEquals()
        {
            Formula f1 = new Formula("3+x1", s => s.ToUpper(), s => true);
            Formula f2 = new Formula(f1.ToString());

            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod]
        public void TestNormalization()
        {
            Formula f = new Formula("x1", s => s.ToUpper(), s => true);
            Assert.AreEqual("X1", f.ToString());
        }

        [TestMethod]
        public void TestTrimFormula()
        {
            Formula f = new Formula("  2 + 3 ", s => s, s => true);
            Assert.AreEqual("2+3", f.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidAfterNormalization()
        {
            Formula f = new Formula("x1", s => s.ToUpper(), s => false);
        }

        [TestMethod]
        public void TestNormalizeVariable()
        {
            Formula f = new Formula("x1 + Y2 * Z3", s => s.ToLower(), s => true);
            Assert.AreEqual("x1+y2*z3", f.ToString());
        }

        [TestMethod]
        public void TestFormulaEquality()
        {
            Formula f1 = new Formula("x + y * z", s => s, s => true);
            Formula f2 = new Formula("X + Y * Z", s => s.ToLower(), s => true);
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod]
        public void TestEqualNull()
        {
            Object f1 = null;
            Formula f2 = new Formula("x1");
            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void TestFormulaInequality()
        {
            Formula f1 = new Formula("a + b * c", s => s, s => true);
            Formula f2 = new Formula("A + B * C", s => s.ToUpper(), s => true);
            Assert.IsFalse(f1 == f2);
        }

        [TestMethod]
        public void TestHashCode()
        {
            Formula f1 = new Formula("a + b * c", s => s, s => true);
            Formula f2 = new Formula("A + B * C", s => s.ToLower(), s => true);
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void TestNotEq()
        {
            Formula f1 = new Formula("2 + x");
            Formula f2 = new Formula("2 + x");

            Assert.IsFalse(f1 != f2);

            f2 = new Formula("2 + y");

            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void TestGetHashCode2()
        {
            // Same formula
            Formula f1 = new Formula("2 + x1");
            Formula f2 = new Formula("2 + x1");

            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());

            // Different formula
            Formula f3 = new Formula("3 + y");

            Assert.IsTrue(f1.GetHashCode() != f3.GetHashCode());

            // Normalization
            Formula f4 = new Formula("2 + X1", s => s.ToLower(), s => true);

            Assert.AreEqual(f1.GetHashCode(), f4.GetHashCode());

            // Simple random formula
            Formula f5 = new Formula("2 + 3");

            Assert.IsFalse(f1.GetHashCode() == f5.GetHashCode());

            // Reverse
            Formula f6 = new Formula("2 + x");

            Assert.AreNotEqual(f1.GetHashCode(), f6.GetHashCode());
        }
    }
}