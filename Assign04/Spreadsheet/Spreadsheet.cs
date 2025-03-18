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

using SpreadsheetUtilities;
using System.Text.RegularExpressions;

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
        private DependencyGraph CellDependencyGraph = new DependencyGraph();

        // I chose dictionary to store cell name and its contents
        private Dictionary<string, Cell> cell = new Dictionary<string, Cell>();

        /// <summary>
        /// Returns an Enumerable that can be used to enumerates 
        /// the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (var kvp in cell)
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
        public override object GetCellContents(string name)
        {
            // Throw exception if name is null or invalid
            if (!isValid(name) || name == null)
                throw new InvalidNameException();

            // Return cell content
            if (cell.ContainsKey(name))
            {
                return cell[name].content;
            }
            return "";
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
        public override ISet<string> SetCellContents(string name, double number)
        {
            // Throw exception
            if (!isValid(name) || name == null)
                throw new InvalidNameException();

            if (cell.ContainsKey(name))
            {
                cell[name].content = number;
            }
            else
            {
                cell.Add(name, new Cell(number));
            }

            // Update cell dependencies and return list of dependent cells
            CellDependencyGraph.ReplaceDependents(name, new HashSet<string>());
            var RecalculateCells = new HashSet<string>(GetCellsToRecalculate(name));
            return RecalculateCells;
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
        public override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
                throw new ArgumentNullException();

            if (!isValid(name) || name == null)
                throw new InvalidNameException();

            // Replace dependents and update cell contents
            CellDependencyGraph.ReplaceDependents(name, new HashSet<string>());

            if (text == "")
            {
                cell.Remove(name); // If text is "", the cell is empty, hence remove this named cell
            }
            else
            {
                if (cell.ContainsKey(name))
                    cell[name].content = text;
                else
                {
                    cell.Add(name, new Cell(text));
                }
            }

            var RecalculateCells = new HashSet<string>(GetCellsToRecalculate(name));
            return RecalculateCells;
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
        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            // In my Formula class, I already throw FormulaFormatException when formula is null
            // so when I used Formula class to represents formula, the input cannot be null.
            // Also, I did try many different way to throw ArgumentNullException when FormulaFormatException
            // is thrown but I cannot test this case and cannot get 100% test coverage.
            if (!isValid(name) || name == null)
                throw new InvalidNameException();

            // Store original dependents to restore when catching CircularException
            IEnumerable<string> originalDependee = GetDirectDependents(name);

            // Replace dependents with new formula
            CellDependencyGraph.ReplaceDependents(name, formula.GetVariables());

            try
            {
                // Recalculate to detect circular dependency
                GetCellsToRecalculate(name);
            }
            catch (CircularException)
            {
                // Discard changes
                CellDependencyGraph.ReplaceDependents(name, originalDependee);
                throw;
            }

            if (cell.ContainsKey(name))
                cell[name].content = formula;
            else
            {
                cell.Add(name, new Cell(formula));
            }

            var RecalculateCells = new HashSet<string>(GetCellsToRecalculate(name));
            return RecalculateCells;
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
            return CellDependencyGraph.GetDependees(name);
        }

        /// <summary>
        /// A private Cell class to present an individual cell 
        /// </summary>
        private class Cell
        {
            public object content { get; set; }

            public Cell(object content)
            {
                this.content = content;
            }
        }

        /// <summary>
        /// A private method to check the validation of Cell name.
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private Boolean isValid(string cellName)
        {
            string pattern = @"^[a-zA-Z]+[0-9]+$";
            Regex nameIdentifier = new Regex(pattern);
            Match match = nameIdentifier.Match(cellName);
            bool validNameFound = match.Success;
            return validNameFound;
        }
    }
}
