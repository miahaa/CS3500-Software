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
using System.Xml;

namespace SS
{
    [TestClass]
    public class SpreadsheetTests
    {
        static Spreadsheet setUp()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "10.5");
            s.SetContentsOfCell("A2", "Apple");
            s.SetContentsOfCell("A3", "=2");
            s.SetContentsOfCell("A4", "=3 * 2");

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
            s.SetContentsOfCell("A1", "10.5");

            object cellContents = s.GetCellContents("A1");

            Assert.AreEqual(10.5, cellContents);
        }

        [TestMethod]
        public void TestGetCellContentsText()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Apple");

            object cellContents = s.GetCellContents("A1");

            Assert.AreEqual("Apple", cellContents);
        }

        [TestMethod]
        public void TestGetCellContentsFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A3", "=A1 + A2");

            object cellContents = s.GetCellContents("A3");

            Assert.IsInstanceOfType(cellContents, typeof(Formula));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentNumberInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("123", "2.0");
        }

        [TestMethod]
        public void TestSetExistingCellContentNumber()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("A1", "12");
            Assert.AreNotEqual(10.5, s.GetCellContents("A1"));
            Assert.AreEqual(12.0, s.GetCellContents("A1"));
        }

        [TestMethod]
        public void TestSetExistingCellContentText()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("A2", "Banana");
            Assert.IsFalse(s.GetCellContents("A1").Equals("Apple"));
            Assert.IsTrue("Banana".Equals(s.GetCellContents("A2")));
        }

        [TestMethod]
        public void TestSetExistingCellContentFormula()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("A4", "=2 * 2");
            Assert.IsTrue("=3 * 2" != s.GetCellContents("A4"));
            Assert.IsTrue("=2 * 2" != s.GetCellContents("A4"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetCellContentTextNull()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("A1", (string)null);
        }

        [TestMethod]
        public void TestRemoveEmptyCell()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("A1", "");
            IEnumerable<string> nonEmptyCells = s.GetNamesOfAllNonemptyCells();

            CollectionAssert.AreEquivalent(new List<string> { "A1", "A2", "A3", "A4" }, nonEmptyCells.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentTextInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("123", "Banana");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetCellContentFormulaInvalidName()
        {
            Spreadsheet s = setUp();
            s.SetContentsOfCell("123", "=2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetCellContentNullFormula()
        {
            Spreadsheet s = setUp();
            try
            {
                s.SetContentsOfCell("A1", null);
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
            s.SetContentsOfCell("A1", "=A2 + A3");
            s.SetContentsOfCell("A3", "=A4 + A5");
            s.SetContentsOfCell("A5", "=A1");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void discardChangeCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2 + A3");
                s.SetContentsOfCell("A3", "=A4 + A5");
                s.SetContentsOfCell("A2", "3");
                s.SetContentsOfCell("A4", "6");
                s.SetContentsOfCell("A5", "20");
                s.SetContentsOfCell("A5", "=A1");
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

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void BadFilePath()
        {
            Spreadsheet spreadSheet = new Spreadsheet("", x => x.Equals("A"), x => x, "test");
        }

        /////////////////////////////////// ASSIGNMENT 4 GRADING TESTS //////////////////////////////////////////////////
        [TestMethod(), Timeout(2000)]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetContents()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("1AA");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("3")]
        public void TestGetEmptyContents()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod(), Timeout(2000)]
        [TestCategory("5")]
        public void TestSetNameDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1A1A", "1.5");
            Assert.AreEqual(1.5, s.GetCellContents("1A1A"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("6")]
        public void TestSimpleSetDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod(), Timeout(2000)]
        [TestCategory("9")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "hello");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("10")]
        public void TestSetGetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod(), Timeout(2000)]
        [TestCategory("13")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "=2");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("14")]
        public void TestSetGetForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "=3");
            var f = s.GetCellContents("Z7");
            Assert.AreEqual(new Formula("3"), f);
            Assert.AreNotEqual(new Formula("2"), f);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod(), Timeout(2000)]
        [TestCategory("15")]
        [ExpectedException(typeof(CircularException))]
        public void TestSimpleCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("16")]
        [ExpectedException(typeof(CircularException))]
        public void TestComplexCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A3", "=A4+A5");
            s.SetContentsOfCell("A5", "=A6+A7");
            s.SetContentsOfCell("A7", "=A1+A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2+A3");
                s.SetContentsOfCell("A2", "15");
                s.SetContentsOfCell("A3", "30");
                s.SetContentsOfCell("A2", "=A3*A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17b")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCellsCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2");
                s.SetContentsOfCell("A2", "=A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual("", s.GetCellContents("A2"));
                Assert.IsTrue(new HashSet<string> { "A1" }.SetEquals(s.GetNamesOfAllNonemptyCells()));
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("18")]
        public void TestEmptyNames()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("19")]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("B1"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("20")]
        public void TestSimpleNamesString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("21")]
        public void TestSimpleNamesDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("22")]
        public void TestSimpleNamesFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("23")]
        public void TestMixedNames()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("24")]
        public void TestSetSingletonDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SequenceEqual(new List<string>() { "A1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("25")]
        public void TestSetSingletonString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SequenceEqual(new List<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("26")]
        public void TestSetSingletonFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SequenceEqual(new List<string>() { "C1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("27")]
        public void TestSetChain()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
        }

        // CHANGING CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("28")]
        public void TestChangeFtoD()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("29")]
        public void TestChangeFtoS()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("30")]
        public void TestChangeStoF()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", "=23");
            Assert.AreEqual(new Formula("23"), s.GetCellContents("A1"));
            Assert.AreNotEqual(new Formula("24"), s.GetCellContents("A1"));
        }

        // STRESS TESTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("31")]
        public void TestStress1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            IList<String> cells = s.SetContentsOfCell("E1", "0");
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
        }

        // Repeated for extra weight
        [TestMethod(), Timeout(2000)]
        [TestCategory("32")]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("33")]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("34")]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("35")]
        public void TestStress2()
        {
            Spreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i, "=A" + (i + 1))));
            }
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("36")]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("37")]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("38")]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("39")]
        public void TestStress3()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, "=A" + (i + 1));
            }
            try
            {
                s.SetContentsOfCell("A150", "=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("40")]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("41")]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("42")]
        public void TestStress3c()
        {
            TestStress3();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("43")]
        public void TestStress4()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
            }
            LinkedList<string> firstCells = new LinkedList<string>();
            LinkedList<string> lastCells = new LinkedList<string>();
            for (int i = 0; i < 250; i++)
            {
                firstCells.AddFirst("A1" + i);
                lastCells.AddFirst("A1" + (i + 250));
            }
            Assert.IsTrue(s.SetContentsOfCell("A1249", "25.0").SequenceEqual(firstCells));
            Assert.IsTrue(s.SetContentsOfCell("A1499", "0").SequenceEqual(lastCells));
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("44")]
        public void TestStress4a()
        {
            TestStress4();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("45")]
        public void TestStress4b()
        {
            TestStress4();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("46")]
        public void TestStress4c()
        {
            TestStress4();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("47")]
        public void TestStress5()
        {
            RunRandomizedTest(47, 2519);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("48")]
        public void TestStress6()
        {
            RunRandomizedTest(48, 2521);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("49")]
        public void TestStress7()
        {
            RunRandomizedTest(49, 2526);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("50")]
        public void TestStress8()
        {
            RunRandomizedTest(50, 2521);
        }

        /// <summary>
        /// Sets random contents for a random cell 10000 times
        /// </summary>
        /// <param name="seed">Random seed</param>
        /// <param name="size">The known resulting spreadsheet size, given the seed</param>
        public void RunRandomizedTest(int seed, int size)
        {
            Spreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.14");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        /// <summary>
        /// Generates a random cell name with a capital letter and number between 1 - 99
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        /// <summary>
        /// Generates a random Formula
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }

        
        /// ////////////////////////////////// AS5 More tests /////////////////////////////////////////////////////////////////
        
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestConstructor()
        {
            Spreadsheet s = new Spreadsheet("validFile.xml", (s) => true, (s) => s.ToUpper(), "default");
            Assert.IsNotNull(s);
            
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestConstructorMiMatchedVersion()
        {
            // Arrange
            string filePath = "validFileWithMismatchingVersion.xml";
            Func<string, bool> IsValid = (s) => true;
            Func<string, string> Normalize = (s) => s.ToUpper();
            string version = "expectedVersion";

            // Act & Assert
            Spreadsheet s = new Spreadsheet(filePath, IsValid, Normalize, version);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestInvalidFilePath()
        {
            // Arrange
            string filePath = "nonExistentFile.xml";
            Func<string, bool> IsValid = (s) => true;
            Func<string, string> Normalize = (s) => s.ToUpper();
            string version = "default";

            // Act & Assert
            Spreadsheet s = new Spreadsheet(filePath, IsValid, Normalize, version);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void ReadFromUnknownFileTest()
        {
            // should not be able to read 
            AbstractSpreadsheet ss = new Spreadsheet("missing.txt", s => true, s => s, "");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void InvalidVersionTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save5.txt");
            ss = new Spreadsheet("save5.txt", s => true, s => s, "version");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void FileNotFoundTest()
        {
            Spreadsheet s = new Spreadsheet("svae.txt", (x) => true, (x) => x, "default");
        }


        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetSavedVersionException()
        {
            Spreadsheet s = new Spreadsheet("save.txt", (x) => true, (x) => x, "default");
            s.GetSavedVersion("svae.txt");
        }

        [TestMethod]
        public void GetCellValue()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "10.5");
            Assert.AreEqual(10.5, s.GetCellValue("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValueInvalidCellName()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellValue("Invalid");
        }

        [TestMethod]
        public void GetCellValueString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "banana");
            Assert.AreEqual("banana", s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "10");
            s.SetContentsOfCell("C1", "5");
            s.SetContentsOfCell("A1", "= B1 * C1");

            Assert.AreEqual(50.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void ReadFromMissingFile()
        {
            AbstractSpreadsheet ss = new Spreadsheet("q:\\missing\\save.txt", s => true, s => s, "");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void InvalidXML()
        {
            using (StreamWriter writer = new StreamWriter("save2.txt"))
            {
                writer.WriteLine("Banana");
                writer.WriteLine("Helo");
                writer.WriteLine("Apple");
                writer.WriteLine("World");
            }
            AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
        }

        [TestMethod]
        public void Save()
        {
            Spreadsheet spreadSheet = new Spreadsheet(x => true, x => x, "tests");
            spreadSheet.SetContentsOfCell("A1", "=B2+B3");
            spreadSheet.SetContentsOfCell("A2", "2.5");
            spreadSheet.SetContentsOfCell("A3", "test");
            spreadSheet.Save("file2.txt");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void MisMatchedVersion()
        {
            Spreadsheet spreadSheet = new Spreadsheet(x => true, x => x, "test");
            spreadSheet.Save("file1.txt");
            Spreadsheet tester = new Spreadsheet("Fail", x => true, x => x, "test1");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TryToStartMismatchVersions()
        {
            Spreadsheet spreadSheet = new Spreadsheet("v1", x => true, x => x, "test");
        }

        [TestMethod, Timeout(2000)]
        public void GetSavedVersion()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "2022");
            ss.Save("save2.txt");
            Assert.AreEqual("2022", new Spreadsheet().GetSavedVersion("save2.txt"));
        }

        [TestMethod, Timeout(2000)]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetFileWrongVersion()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save2.txt");
            ss = new Spreadsheet("save2.txt", s => true, s => s, "version");
        }

        //Incorrect verion test
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestIncorrectVersion()
        {
            Spreadsheet ss = new Spreadsheet("save1.txt", s => true, s => s, "v1");
        }

        [TestMethod]
        public void ParseFileTest()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("A1", "banana");
            s1.Save("file1.txt");
            s1 = new Spreadsheet("file1.txt", s => true, s => s, "default");
            Assert.AreEqual("banana", s1.GetCellContents("A1"));
        }

        [TestMethod]
        public void testSaveAndReadMethod()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            IList<String> cells = s.SetContentsOfCell("E1", "0");

            s.Save("file1.txt");

            s = new Spreadsheet("file1.txt", s => true, s => s, "default");
            Assert.AreEqual(0.0, s.GetCellValue("D1"));
            Assert.AreEqual(0.0, s.GetCellValue("A1"));
            Assert.AreEqual(0.0, s.GetCellValue("C2"));
        }
    }
}