/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [02/08/2024]
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
/// This file contains all test cases for Spreadsheet project that support various 
/// type of cell contents and cell dependencies management.
/// </summary>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;

namespace SS
{
    [TestClass]
    public class SpreadsheetTests
    {
        static Spreadsheet setUp()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 10.5);
            s.SetCellContents("A2", "Apple");
            s.SetCellContents("A3", new Formula("2"));
            s.SetCellContents("A4", new Formula("3 * 2"));

            return s;
        }

        [TestMethod]
        public void TestGetNamesOfAllNonemptyCells()
        {
            Spreadsheet s = setUp();
            IEnumerable<string> nonEmptyCells = s.GetNamesOfAllNonemptyCells();

            CollectionAssert.AreEquivalent(new List<string> { "A1", "A2", "A3", "A4" }, nonEmptyCells.ToList());
        }

        [TestMethod]
        public void TestGetCellContentsEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContentsInvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("123");
        }

        [TestMethod]
        public void TestGetCellContentsNumber()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 10.5);

            object cellContents = s.GetCellContents("A1");

            Assert.AreEqual(10.5, cellContents);
        }

        [TestMethod]
        public void TestGetCellContentsText()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", "Apple");

            object cellContents = s.GetCellContents("A1");

            Assert.AreEqual("Apple", cellContents);
        }

        [TestMethod]
        public void TestGetCellContentsFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A3", new Formula("A1 + A2"));

            object cellContents = s.GetCellContents("A3");

            Assert.IsInstanceOfType(cellContents, typeof(Formula));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentNumberInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("123", 2.0);
        }

        [TestMethod]
        public void TestSetExistingCellContentNumber()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("A1", 12);
            Assert.AreNotEqual(10.5, s.GetCellContents("A1"));
            Assert.AreEqual(12.0, s.GetCellContents("A1"));
        }

        [TestMethod]
        public void TestSetExistingCellContentText()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("A2", "Banana");
            Assert.IsFalse(s.GetCellContents("A1").Equals("Apple"));
            Assert.IsTrue("Banana".Equals(s.GetCellContents("A2")));
        }

        [TestMethod]
        public void TestSetExistingCellContentFormula()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("A4", new Formula("2 * 2"));
            Assert.IsTrue(new Formula("3 * 2") != s.GetCellContents("A4"));
            Assert.IsTrue(new Formula("2 * 2") != s.GetCellContents("A4"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetCellContentTextNull()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("A1", (string)null);
        }

        [TestMethod]
        public void TestRemoveEmptyCell()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("A1", "");
            IEnumerable<string> nonEmptyCells = s.GetNamesOfAllNonemptyCells();

            CollectionAssert.AreEquivalent(new List<string> { "A2", "A3", "A4" }, nonEmptyCells.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentTextInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("123", "Banana");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentFormulaInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetCellContents("123", new Formula("2"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetCellContentNullFormula()
        {
            Spreadsheet s = setUp();
            try
            {
                s.SetCellContents("A1", new Formula(null));
            }
            catch (FormulaFormatException)
            {
                throw new ArgumentNullException();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A2 + A3"));
            s.SetCellContents("A3", new Formula("A4 + A5"));
            s.SetCellContents("A5", new Formula("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void discardChangeCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetCellContents("A1", new Formula("A2 + A3"));
                s.SetCellContents("A3", new Formula("A4 + A5"));
                s.SetCellContents("A2", 3);
                s.SetCellContents("A4", 6);
                s.SetCellContents("A5", 20);
                s.SetCellContents("A5", new Formula("A1"));
            }
            catch (CircularException)
            {
                Assert.AreEqual(20.0, s.GetCellContents("A5"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetDirectDependentInvalidName()
        {
            Spreadsheet s = setUp();
            s.GetCellContents("223");
        }
    }
}