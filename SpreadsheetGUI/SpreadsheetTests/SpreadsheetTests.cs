/// <summary>
/// Author:    Amber (Phuong) Tran
/// Partner:   -none-
/// Date:      18-Feb-2023
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
///    differnt units for Spreadsheet
///    
/// </summary>
///
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;
using System;
using System.Xml;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTest
    {
        [TestMethod]
        public void TestSaveToCurrentDirectory()
        {
            // Arrange
            string filePath = "save.txt";
            AbstractSpreadsheet sheet = new Spreadsheet();

            // Act
            sheet.Save(filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));

            // Clean up
            File.Delete(filePath);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSaveToNonExistentPath()
        {
            // Arrange
            AbstractSpreadsheet sheet = new Spreadsheet();

            // Act
            sheet.Save("/user/amber/path.xml");
        }

        [TestMethod]
        public void TestConstructorWithDefaultSettings()
        {
            // Arrange & Act
            var sheet = new Spreadsheet();

            // Assert
            Assert.IsNotNull(sheet);
        }

        [TestMethod]
        public void TestConstructorWithCustomSettings()
        {
            // Arrange & Act
            var sheet = new Spreadsheet(
                name => true,
                name => name.ToUpper(),
                "customVersion"
            );

            // Assert
            Assert.IsNotNull(sheet);
        }

        [TestMethod]
        public void TestConstructorWithFile()
        {
            // Arrange
            var filename = "sample.xml";

            // Act & Assert
            Assert.ThrowsException<SpreadsheetReadWriteException>(() =>
            {
                var sheet = new Spreadsheet(filename, name => true, name => name.ToUpper(), "customVersion");
            });
        }

        [TestMethod]
        public void TestSetAndGetCellContentsWithString()
        {
            // Arrange
            var sheet = new Spreadsheet();

            // Act
            sheet.SetContentsOfCell("A1", "hello");

            // Assert
            Assert.AreEqual("hello", sheet.GetCellContents("A1"));
        }

        [TestMethod]
        public void TestSetAndGetCellContentsWithDouble()
        {
            
            var sheet = new Spreadsheet();

            
            sheet.SetContentsOfCell("A1", "42");

            
            Assert.AreEqual(42.0, sheet.GetCellContents("A1"));
        }

        [TestMethod]
        public void TestSetAndGetCellContentsWithFormula()
        {
            // Arrange
            var sheet = new Spreadsheet();

            // Act
            sheet.SetContentsOfCell("A1", "B1+1");

            // Assert
            var cellContent = sheet.GetCellContents("A1");
            Assert.AreEqual("B1+1", cellContent.ToString());
        }

        [TestMethod]
        public void TestSaveAndLoadSpreadsheet()
        {
            // Arrange
            var filename = "test_save.xml";
            var sheet = new Spreadsheet();

            // Act
            sheet.SetContentsOfCell("A1", "hello");
            sheet.SetContentsOfCell("B1", "42");
            sheet.SetContentsOfCell("C1", "B1+1");
            sheet.Save(filename);

            var loadedSheet = new Spreadsheet(filename, name => true, name => name.ToUpper(), "customVersion");

            // Assert
            //Assert.AreEqual("hello", loadedSheet.GetCellValue("A1"));
            //Assert.AreEqual(42.0, loadedSheet.GetCellValue("B1"));
            //Assert.AreEqual("B1+1", loadedSheet.GetCellContents("C1"));
        }

        [TestMethod()]
        public void StressTest()
        {
            using (XmlWriter writer = XmlWriter.Create("save.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "wow");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A2");
                writer.WriteElementString("contents", "car");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A3");
                writer.WriteElementString("contents", "nah");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A4");
                writer.WriteElementString("contents", "bope");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A5");
                writer.WriteElementString("contents", "cow");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A6");
                writer.WriteElementString("contents", "brick");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AbstractSpreadsheet sheet = new Spreadsheet("save.txt", x => true, x => x.ToUpper(), "");

            string ex = "B";
            string zero = "0";
            for (int i = 0; i < 6; i++)
            {
                string ex2 = ex + i.ToString();
                sheet.SetContentsOfCell(ex2, i.ToString());
            }

            Assert.AreEqual(sheet.GetCellContents("A1"), "wow");
            Assert.AreEqual(sheet.GetCellContents("A2"), "car");
            Assert.AreEqual(sheet.GetCellContents("A3"), "nah");
            Assert.AreEqual(sheet.GetCellContents("A4"), "bope");
            Assert.AreEqual(sheet.GetCellContents("A5"), "cow");
            Assert.AreEqual(1.0, sheet.GetCellContents("B1"));
            Assert.AreEqual(2.0, sheet.GetCellContents("B2"));
            Assert.AreEqual(3.0, sheet.GetCellContents("B3"));
            Assert.AreEqual(4.0, sheet.GetCellContents("B4"));
            Assert.AreEqual(5.0, sheet.GetCellContents("B5"));

            for (int i = 0; i < 6; i++)
            {
                string ex2 = ex + i.ToString();
                sheet.SetContentsOfCell(ex2, zero);
            }

            for (int i = 0; i < 6; i++)
                Assert.AreEqual(0.0, sheet.GetCellContents((ex + i).ToString()));
        }


        /// <summary>
        /// Tests the file Constructor using cell's and strings
        /// Three Tests
        /// </summary>
        [TestMethod]
        public void testFileConstructor()
        {
            using (XmlWriter writer = XmlWriter.Create("save.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            AbstractSpreadsheet sheet = new Spreadsheet("save.txt", x => true, x => x.ToUpper(), "1");
            Assert.AreEqual("hello", sheet.GetCellValue("A1"));
            Assert.AreEqual(true, sheet.Changed);

            sheet.SetContentsOfCell("A1", "wow");
            Assert.AreEqual(true, sheet.Changed);
        }

        ///// <summary>
        ///// Tests the file Constructor using cell's with formulas and doubles
        ///// Two Tests
        ///// </summary>
        //[TestMethod]
        public void TestFileConstructorTwo()
        {
            using (XmlWriter writer = XmlWriter.Create("save.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "=1.0+2.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A2");
                writer.WriteElementString("contents", "3");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            AbstractSpreadsheet sheet = new Spreadsheet("version", x => true, x => x.ToUpper(), "save.txt");
            Assert.AreEqual(3.0, sheet.GetCellValue("A1"));
            Assert.AreEqual(3.0, sheet.GetCellValue("A2"));
        }

        /// <summary>
        /// Tests the saveFile method using cell's with strings, also tests the changed
        /// boolean
        /// Seven Tests
        /// </summary>
        [TestMethod]
        public void testFileSave()
        {
            using (XmlWriter writer = XmlWriter.Create("save.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            AbstractSpreadsheet sheet = new Spreadsheet("save.txt", x => true, x => x.ToUpper(), "1");
            Assert.AreEqual("hello", sheet.GetCellValue("A1"));
            Assert.AreEqual(true, sheet.Changed);

            sheet.SetContentsOfCell("A1", "wow");
            Assert.AreEqual(true, sheet.Changed);

            sheet.SetContentsOfCell("A2", "hello");

            sheet.Save("save.txt");
            //Assert.AreEqual(false, sheet.Changed);
            AbstractSpreadsheet sheet2 = new Spreadsheet("save.txt", x => true, x => x.ToUpper(), "1");

            //Assert.AreEqual("wow", sheet2.GetCellValue("A1"));
            //Assert.AreEqual("hello", sheet2.GetCellValue("A2"));
        }
    }

}
