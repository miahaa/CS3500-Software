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
/// This file contains a spreadsheet that managed a collection of cells where each cell can either a string,
/// a double, or a formula.
/// </summary>

using Microsoft.VisualBasic;
using SpreadsheetUtilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SS
{
    /// <summary>
    /// <para>
    ///     A spreadsheet inherits from an AbstractSpreadsheet object which represents the state of a 
    ///     simple spreadsheet.  A spreadsheet consists of an infinite number of named cells.
    /// </para>
    /// <para>
    ///     A string is a valid cell name if and only if:
    /// </para>
    /// <list type="number">
    ///      <item> its first character is an underscore or a letter</item>
    ///      <item> its remaining characters (if any) are underscores and/or letters and/or digits</item>
    /// </list>   
    /// <para>
    ///     Note that this is the same as the definition of valid variable from the Formula class assignment.
    /// </para>
    /// 
    /// <para>
    ///     For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    ///     "25", "2x", and "&amp;" are not.  Cell names are case sensitive, so "x" and "X" are
    ///     different cell names.
    /// </para>
    /// 
    /// <para>
    ///     A spreadsheet contains a cell corresponding to every possible cell name.  (This
    ///     means that a spreadsheet contains an infinite number of cells.)  In addition to 
    ///     a name, each cell has a contents and a value.  The distinction is important.
    /// </para>
    /// 
    /// <para>
    ///     The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    ///     contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    ///     of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// </para>
    /// 
    /// <para>
    ///     In a new spreadsheet, the contents of every cell is the empty string. Note: 
    ///     this is by definition (it is IMPLIED, not stored).
    /// </para>
    /// 
    /// <para>
    ///     The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    ///     (By analogy, the value of an Excel cell is what is displayed in that cell's position
    ///     in the grid.)
    /// </para>
    /// 
    /// <list type="number">
    ///   <item>If a cell's contents is a string, its value is that string.</item>
    /// 
    ///   <item>If a cell's contents is a double, its value is that double.</item>
    /// 
    ///   <item>
    ///      If a cell's contents is a Formula, its value is either a double or a FormulaError,
    ///      as reported by the Evaluate method of the Formula class.  The value of a Formula,
    ///      of course, can depend on the values of variables.  The value of a variable is the 
    ///      value of the spreadsheet cell it names (if that cell's value is a double) or 
    ///      is undefined (otherwise).
    ///   </item>
    /// 
    /// </list>
    /// 
    /// <para>
    ///     Spreadsheets are never allowed to contain a combination of Formulas that establish
    ///     a circular dependency.  A circular dependency exists when a cell depends on itself.
    ///     For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    ///     A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    ///     dependency.
    /// </para>
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        // Use DependencyGraph class to manage cell dependencies
        private DependencyGraph CellDependencyGraph;

        // I chose dictionary to store cell name and its contents
        private Dictionary<string, Cell> cells;

        public override bool Changed { get; protected set; }

        /// <summary>
        /// Constructor with zero-argument to create an empty spreadsheet
        /// </summary>
        public Spreadsheet()
            : this(s => true, s => s, "default")
        { }

        /// <summary>
        /// Constructor with three arguments
        /// </summary>
        /// <param name="IsValid"></param>
        /// <param name="Normalize"></param>
        /// <param name="Version"></param>
        public Spreadsheet(Func<String, bool> IsValid, Func<string, string> Normalize, string Version)
            : base(IsValid, Normalize, Version)
        {
            cells = new Dictionary<string, Cell>();
            CellDependencyGraph = new DependencyGraph();
            Changed = false;
            this.Version = Version;
        }

        /// <summary>
        /// Constructor with four arguments 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="IsValid"></param>
        /// <param name="Normalize"></param>
        /// <param name="Version"></param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public Spreadsheet(String filePath, Func<string, bool> IsValid, Func<String, string> Normalize, string Version)
            : base(IsValid, Normalize, Version)
        {
            cells = new Dictionary<string, Cell>();
            CellDependencyGraph = new DependencyGraph();
            Changed = false;
            this.Version = Version;

            if (GetSavedVersion(filePath) != Version)
            {
                throw new SpreadsheetReadWriteException("The version of the file does not match");
            }
            try
            {
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "cell")
                        {
                            reader.ReadToFollowing("name");
                            string name = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("contents");
                            string content = reader.ReadElementContentAsString();
                            SetContentsOfCell(name, content);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException("Cannot get file");
            }
        }


        /// <summary>
        /// Returns an Enumerable that can be used to enumerates 
        /// the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (var kvp in cells)
            {
                if (kvp.Value.content != null && kvp.Key != "")
                {
                    yield return kvp.Key;
                }
            }
        }

        /// <summary>
        ///   Returns the contents (as opposed to the value) of the named cell.
        /// </summary>
        /// 
        /// <exception cref="InvalidNameException"> 
        ///   Thrown if the name is null or invalid
        /// </exception>
        /// 
        /// <param name="name">The name of the spreadsheet cell to query</param>
        /// 
        /// <returns>
        ///   The return value should be either a string, a double, or a Formula.
        ///   See the class header summary 
        /// </returns>
        public override object GetCellContents(String name)
        {
            string NormalizedName = Normalize(name);
            // Throw exception if name is null or invalid
            if (!isValidCellName(name) || name == null)
                throw new InvalidNameException();

            // Return cell content
            if (cells.TryGetValue(NormalizedName, out var cell))
            {
                return cell.content;
            }
            else return "";
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// </summary>
        ///
        /// <exception cref="InvalidNameException"> 
        ///   If the name is invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <param name="name"> The name of the cell that we want the value of (will be normalized)</param>
        /// 
        /// <returns>
        ///   Returns the value (as opposed to the contents) of the named cell.  The return
        ///   value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </returns>
        public override object GetCellValue(string name)
        {
            if (name == null || !isValidCellName(name))
                throw new InvalidNameException();

            object content = GetCellContents(name);
            if (content is Formula)
            {
                Formula f = (Formula)content;
                content = f.Evaluate(s => (double)GetCellValue(s));
            }
            return content;
        }

        /// <summary>
        ///  Set the contents of the named cell to the given number.  
        /// </summary>
        /// 
        /// <exception cref="InvalidNameException"> 
        ///   If the name is null or invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <param name="name"> The name of the cell </param>
        /// <param name="number"> The new contents/value </param>
        /// 
        /// <returns>
        ///   <para>
        ///      The method returns a set consisting of name plus the names of all other cells whose value depends, 
        ///      directly or indirectly, on the named cell.
        ///   </para>
        /// 
        ///   <para>
        ///      For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        ///      set {A1, B1, C1} is returned.
        ///   </para>
        /// </returns>
        protected override IList<String> SetCellContents(String name, double number)
        {
            CellDependencyGraph.ReplaceDependees(name, new HashSet<string>());

            if (cells.ContainsKey(name))
            {
                cells[name].content = number;
                Changed = true;
            }
            else
            {
                cells.Add(name, new Cell(number));
                Changed = true;
            }

            HashSet<string> DependentList = new HashSet<string>(GetCellsToRecalculate(name)) { name };
            return DependentList.ToList();
        }

        /// <summary>
        /// The contents of the named cell becomes the text.  
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> 
        ///   If text is null, throw an ArgumentNullException.
        /// </exception>
        /// 
        /// <exception cref="InvalidNameException"> 
        ///   If the name is null or invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <param name="name"> The name of the cell </param>
        /// <param name="text"> The new content/value of the cell</param>
        /// 
        /// <returns>
        ///   The method returns a set consisting of name plus the names of all 
        ///   other cells whose value depends, directly or indirectly, on the 
        ///   named cell.
        /// 
        ///   <para>
        ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        ///     set {A1, B1, C1} is returned.
        ///   </para>
        /// </returns>
        protected override IList<String> SetCellContents(String name, String text)
        {
            // Replace dependents and update cell contents
            CellDependencyGraph.ReplaceDependents(name, new HashSet<string>());

            if (cells.ContainsKey(name))
            {
                cells[name].content = text;
                Changed = true;
            }
            else
            {
                cells.Add(name, new Cell(text));
                Changed = true;
            }

            HashSet<string> DependentList = new HashSet<string>(GetCellsToRecalculate(name)) { name };
            return DependentList.ToList();
        }

        /// <summary>
        /// Set the contents of the named cell to the formula.  
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> 
        ///   If formula parameter is null, throw an ArgumentNullException.
        /// </exception>
        /// 
        /// <exception cref="InvalidNameException"> 
        ///   If the name is null or invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <exception cref="CircularException"> 
        ///   If changing the contents of the named cell to be the formula would 
        ///   cause a circular dependency, throw a CircularException.  
        ///   (NOTE: No change is made to the spreadsheet.)
        /// </exception>
        /// 
        /// <param name="name"> The cell name</param>
        /// <param name="formula"> The content of the cell</param>
        /// 
        /// <returns>
        ///   <para>
        ///     The method returns a Set consisting of name plus the names of all other 
        ///     cells whose value depends, directly or indirectly, on the named cell.
        ///   </para>
        ///   <para> 
        ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        ///     set {A1, B1, C1} is returned.
        ///   </para>
        /// 
        /// </returns>
        protected override IList<String> SetCellContents(String name, Formula formula)
        {
            var originalDependents = GetDirectDependents(name);
            CellDependencyGraph.ReplaceDependees(name, formula.GetVariables());

            try
            {
                // Recalculate to detect circular dependency
                GetCellsToRecalculate(name);
            }
            catch (CircularException)
            {
                throw;
            }

            if (cells.ContainsKey(name))
            {
                cells[name].content = formula;
                Changed = true;
            }
            else
            {
                cells.Add(name, new Cell(formula));
                Changed = true;
            }

            HashSet<string> DependentList = new HashSet<string>(GetCellsToRecalculate(name)) { name };
            return DependentList.ToList();
        }

        /// <summary>
        ///   <para>Sets the contents of the named cell to the appropriate value. </para>
        ///   <para>
        ///       First, if the content parses as a double, the contents of the named
        ///       cell becomes that double.
        ///   </para>
        ///
        ///   <para>
        ///       Otherwise, if content begins with the character '=', an attempt is made
        ///       to parse the remainder of content into a Formula.  
        ///       There are then three possible outcomes:
        ///   </para>
        ///
        ///   <list type="number">
        ///       <item>
        ///           If the remainder of content cannot be parsed into a Formula, a 
        ///           SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       </item>
        /// 
        ///       <item>
        ///           If changing the contents of the named cell to be f
        ///           would cause a circular dependency, a CircularException is thrown,
        ///           and no change is made to the spreadsheet.
        ///       </item>
        ///
        ///       <item>
        ///           Otherwise, the contents of the named cell becomes f.
        ///       </item>
        ///   </list>
        ///
        ///   <para>
        ///       Finally, if the content is a string that is not a double and does not
        ///       begin with an "=" (equal sign), save the content as a string.
        ///   </para>
        /// </summary>
        ///
        /// <exception cref="InvalidNameException"> 
        ///   If the name parameter is invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <exception cref="SpreadsheetUtilities.FormulaFormatException"> 
        ///   If the content is "=XYZ" where XYZ is an invalid formula, throw a FormulaFormatException.
        /// </exception>
        /// 
        /// <exception cref="CircularException"> 
        ///   If changing the contents of the named cell to be the formula would 
        ///   cause a circular dependency, throw a CircularException.  
        ///   (NOTE: No change is made to the spreadsheet.)
        /// </exception>
        /// 
        /// <param name="name"> The cell name that is being changed</param>
        /// <param name="content"> The new content of the cell</param>
        /// 
        /// <returns>
        ///       <para>
        ///           This method returns a list consisting of the passed in cell name,
        ///           followed by the names of all other cells whose value depends, directly
        ///           or indirectly, on the named cell. The order of the list MUST BE any
        ///           order such that if cells are re-evaluated in that order, their dependencies 
        ///           are satisfied by the time they are evaluated.
        ///       </para>
        ///
        ///       <para>
        ///           For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        ///           list {A1, B1, C1} is returned.  If the cells are then evaluate din the order:
        ///           A1, then B1, then C1, the integrity of the Spreadsheet is maintained.
        ///       </para>
        /// </returns>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            string NormalizedName = Normalize(name);
            if (!isValidCellName(name) || name == null || !IsValid(NormalizedName))
                throw new InvalidNameException();

            if (content == null)
                throw new ArgumentNullException();

            if (Double.TryParse(content, out double result))
            {
                return SetCellContents(name, result);
            }
            if (content.StartsWith('='))
            {
                Formula formula = new Formula(content.Substring(1), Normalize, IsValid);
                return SetCellContents(NormalizedName, formula);
            }
            return SetCellContents(name, content);
        }


        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell. 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> 
        ///   If the name is null, throw an ArgumentNullException.
        /// </exception>
        /// 
        /// <exception cref="InvalidNameException"> 
        ///   If the name is null or invalid, throw an InvalidNameException
        /// </exception>
        /// 
        /// <param name="name"></param>
        /// <returns>
        ///   Returns an enumeration, without duplicates, of the names of all cells that contain
        ///   formulas containing name.
        /// 
        ///   <para>For example, suppose that: </para>
        ///   <list type="bullet">
        ///      <item>A1 contains 3</item>
        ///      <item>B1 contains the formula A1 * A1</item>
        ///      <item>C1 contains the formula B1 + A1</item>
        ///      <item>D1 contains the formula B1 - C1</item>
        ///   </list>
        /// 
        ///   <para>The direct dependents of A1 are B1 and C1</para>
        /// 
        /// </returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            // This case is same as above, the name input in this method cannot be null 
            // as it need to pass valid and null check first to be passed to this method.
            return CellDependencyGraph.GetDependents(name);
        }

        ////////////////////////////// HELPER METHOD AND CELL CLASS /////////////////////////////////////////////////////////


        /// <summary>
        /// A private Cell class to present an individual cell 
        /// </summary>
        private class Cell
        {
            public object content { get; set; }
            public object value { get; set; }

            public Cell(object content)
            {
                this.content = content;
                value = content;
            }
        }


        /// <summary>
        /// A private method to check the validation of Cell name.
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private Boolean isValidCellName(string cellName)
        {
            cellName = Normalize(cellName);
            string pattern = @"[a-zA-Z]+\d+";
            Regex nameIdentifier = new Regex(pattern);
            Match match = nameIdentifier.Match(cellName);
            bool validNameFound = match.Success;
            return validNameFound && IsValid(cellName);
        }

        ///////////////////////////// GET SAVED VERSION AND SAVE FILE /////////////////////////////////////////////////////


        /// <summary>
        ///   Look up the version information in the given file. If there are any problems opening, reading, 
        ///   or closing the file, the method should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// 
        /// <remarks>
        ///   In an ideal world, this method would be marked static as it does not rely on an existing SpreadSheet
        ///   object to work; indeed it should simply open a file, lookup the version, and return it.  Because
        ///   C# does not support this syntax, we abused the system and simply create a "regular" method to
        ///   be implemented by the base class.
        /// </remarks>
        /// 
        /// <exception cref="SpreadsheetReadWriteException"> 
        ///   1Thrown if any problem occurs while reading the file or looking up the version information.
        /// </exception>
        /// 
        /// <param name="filename"> The name of the file (including path, if necessary)</param>
        /// <returns>Returns the version information of the spreadsheet saved in the named file.</returns>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.Name.Equals("spreadsheet"))
                            return reader["version"];
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SpreadsheetReadWriteException("Couldn't retrieve the version");
            }

            throw new SpreadsheetReadWriteException("Get saved version failed");
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>cell name goes here</name>
        /// <contents>cell contents goes here</contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>

        public override void Save(string filename)
        {
            try
            {
                // Get the XML content from GetXML method
                string text = GetXML();

                // Write the XML content to the file
                File.WriteAllText(filename, text);
            }
            catch (IOException)
            {
                throw new SpreadsheetReadWriteException("Cannot save file.");
            }
            Changed = false;
        }

        /// <summary>
        ///   Return an XML representation of the spreadsheet's contents
        /// </summary>
        /// <returns> contents in XML form </returns>
        public override string GetXML()
        {
            // Create a MemoryStream to hold the XML data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Create a StreamWriter with UTF-8 encoding and attach it to the MemoryStream
                using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "  ";

                    // Create an XmlWriter using the StreamWriter and settings
                    using (XmlWriter writer = XmlWriter.Create(streamWriter, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("spreadsheet");

                        // Add the 'version' attribute to the Spreadsheet element
                        writer.WriteAttributeString("version", Version);
                        foreach (string name in cells.Keys)
                        {
                            writer.WriteStartElement("cell");
                            writer.WriteElementString("name", name);

                            var cellContent = cells[name].content;

                            if (cellContent is double)
                            {
                                writer.WriteElementString("contents", cellContent.ToString());
                            }
                            else if (cellContent is string)
                            {
                                writer.WriteElementString("contents", cellContent.ToString());
                            }
                            else if (cellContent is Formula)
                            {
                                writer.WriteElementString("contents", "=" + cellContent.ToString());
                            }

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }

                // Convert the MemoryStream to a string with UTF-8 encoding
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
