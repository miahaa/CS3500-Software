
///// <summary>
///// Author:    Amber (Phuong) Tran
///// Partner:   -none-
///// Date:      18-Feb-2024
///// Course:    CS 3500, University of Utah, School of Computing
///// Copyright: CS 3500 and Phuong Tran - This work may not
/////            be copied for use in Academic Coursework.
/////
///// I, Amber Tran, certify that I wrote this code from scratch and
///// did not copy it in part or whole from another source.  All
///// references used in the completion of the assignments are cited
///// in my README file.
/////
///// File Contents
///// Full function of spreadsheet
/////
///// </summary>

using SpreadsheetUtilities;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS
{
    /// <summary>
    /// This class represents the simple spreadsheet that consists of infinite number of named cells
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {

        private DependencyGraph graph;
        private Dictionary<string, Cell> cells;

        /// <summary>
        /// This class represents a spreadsheet cell that holds a number, string, Formula, or empty string
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// Stores a number in a cell
            /// </summary>
            /// <param name="number"> number to store </param>
            public Cell(double number)
            {
                CellContent = number;
                CellValue = number;
            }

            /// <summary>
            /// Stores a string or empty string in a cell
            /// </summary>
            /// <param name="text"> text to store </param>
            public Cell(string text)
            {
                CellContent = text;
                CellValue = text;
            }

            /// <summary>
            /// Stores a formula in a cell
            /// </summary>
            /// <param name="formula"> formula object to store </param>
            public Cell(Formula formula, Dictionary<string, Cell> cells)
            {
                CellContent = formula;
                CellValue = formula.Evaluate(variable => cells[variable].CellValue is double ?
                (double)cells[variable].CellValue : throw new ArgumentException("Invalid Variable"));
            }

            /// <summary>
            /// Gets and sets the current cell content
            /// </summary>
            public object CellContent { get; set; }

            /// <summary>
            /// Gets and sets the current cell value
            /// </summary>
            public object CellValue { get; set; }
        }

        /// <summary>
        /// Creates a new Spreadsheet with a dependency graph and set of cells
        /// </summary>
        public Spreadsheet() : base(validator => true, normalizer => normalizer, "default")
        {
            graph = new DependencyGraph();
            cells = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Creates a new Spreadsheet with a dependency graph, set of cells, 
        /// user validity function, user normalize variable function, and Spreadsheet version
        /// </summary>
        /// <param name="isValid"> user's definition of valid variable </param>
        /// <param name="normalize"> user's definition of normalized variable </param>
        /// <param name="version"> version name of the spreadsheet </param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            graph = new DependencyGraph();
            cells = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Creates a new Spreadsheet with a dependency graph, set of cells, 
        /// filename, user variable validity function, user variable normalize function, and Spreadsheet version
        /// </summary>
        /// <param name="filename"> file path to create spreadsheet from </param>
        /// <param name="isValid"> user's definition of valid variable </param>
        /// <param name="normalize"> user's definition of normalized variable </param>
        /// <param name="version"> version name of the spreadsheet </param>
        public Spreadsheet(string filename, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            graph = new DependencyGraph();
            cells = new Dictionary<string, Cell>();
            Changed = false;
            if (Version != GetSavedVersion(filename))
                throw new SpreadsheetReadWriteException("Not Right Version");
            ReadSavedFile(filename);
        }

        /// <summary>
        /// Gets and Sets the state of spreadsheet modification
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// Returns the cell content of either a number, string, Formula, empty string
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <returns> Either a number, string, Formula, or empty string </returns>
        /// <exception cref="InvalidNameException"> Thrown when the cell name is invalid </exception>
        public override object GetCellContents(string name)
        {
            if (!IsValidCellName(name) || !IsValid(name))
                throw new InvalidNameException();

            string normalizedName = Normalize(name);
            return cells.ContainsKey(normalizedName) ? cells[normalizedName].CellContent : "";
        }

        /// <summary>
        /// Gets all the cells names that don't contain empty strings
        /// </summary>
        /// <returns> List of cell names that don't have empty strings </returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (string cell in cells.Keys)
            {
                if (!cells[cell].CellContent.Equals(""))
                {
                    yield return cell;
                }
            }
        }

        /// <summary>
        /// Makes a new cell for a string, number, or Formula
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <param name="content"> content is either string, number, or Formula </param>
        /// <returns> dependee list of the current cell name </returns>
        /// <exception cref="InvalidNameException"> Thrown if name is invalid </exception>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);
            if (!IsValidCellName(name) | !IsValid(name))
                throw new InvalidNameException();

            //Check if content is a double
            double isNumber;
            if (Double.TryParse(content, out isNumber))
                return SetCellContents(name, isNumber);

            //Check if content is a legal formula
            if (content != "")
                if (content[0] == '=')
                    return SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid));
            //Assume content is legal string
            return SetCellContents(name, content);
        }

        /// <summary>
        /// Sets the cell name with a number
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <param name="number"> number to store in the cell name </param>
        /// <returns> dependee list of the current cell name </returns>
        protected override IList<string> SetCellContents(string name, double number)
        {
            // Checks if cell number replacement is needed
            if (cells.ContainsKey(name))
            {
                cells[name].CellContent = number;
                cells[name].CellValue = number;

                // Gets all of the dependees that need to be recalculated
                List<string> cellsToRecalculate = new List<string>(GetCellsToRecalculate(name));

                // Replace all the current cell name's dependents as empty
                graph.ReplaceDependents(name, new List<string>());
                Changed = true;
                if (graph.HasDependees(name))
                    CellRecalculation(name);

                return cellsToRecalculate;
            }

            else
            {
                // Checks if this cell is the only non-Formula object cell
                if (cells.Values.All<Cell>(cell => cell.CellContent is Formula))
                {
                    cells.Add(name, new Cell(number));
                    Changed = true;
                    if (graph.HasDependees(name))
                        CellRecalculation(name);

                    return cells.Keys.ToList();
                }

                // Creates a new number cell
                cells.Add(name, new Cell(number));
                Changed = true;
                if (graph.HasDependees(name))
                    CellRecalculation(name);

                return DependencyListGenerator(name);
            }
        }

        /// <summary>
        /// Sets the cell name with a string or empty string
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <param name="text"> string or empty string to store in the cell name </param>
        /// <returns> dependee list of the cell name </returns>
        protected override IList<string> SetCellContents(string name, string text)
        {
            if (cells.ContainsKey(name))
            {
                cells[name].CellContent = text;
                cells[name].CellValue = text;

                // Gets all of the dependees that need to be recalculated
                List<string> cellsToRecalculate = new List<string>(GetCellsToRecalculate(name));

                // Replace all the current cell name's dependents as empty
                graph.ReplaceDependents(name, new List<string>());
                Changed = true;
                if (graph.HasDependees(name))
                    CellRecalculation(name);

                return cellsToRecalculate;
            }

            else
            {
                // Checks if this cell is the only non-Formula object cell
                if (cells.Values.All<Cell>(cell => cell.CellContent is Formula))
                {
                    cells.Add(name, new Cell(text));
                    Changed = true;
                    if (graph.HasDependees(name))
                        CellRecalculation(name);

                    return cells.Keys.ToList();
                }

                // Creates a new text cell
                cells.Add(name, new Cell(text));
                Changed = true;
                if (graph.HasDependees(name))
                    CellRecalculation(name);

                return DependencyListGenerator(name);
            }
        }

        /// <summary>
        /// Sets the cell name with a Formula object
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <param name="formula"> Formula object to store in cell name </param>
        /// <returns> dependent list of cell name </returns>
        /// <exception cref="CircularException"> Thrown when Formula object has the valid cell name in the formula </exception>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            if (cells.ContainsKey(name))
            {
                try
                {
                    graph.ReplaceDependents(name, formula.GetVariables());

                    // Gets all of the dependees that need to be recalculated
                    List<string> cellsToRecalculate = new List<string>(GetCellsToRecalculate(name));
                    cells[name].CellContent = formula;
                    Changed = true;
                    if (graph.HasDependees(name))
                        CellRecalculation(name);

                    return cellsToRecalculate;
                }

                catch (CircularException e)
                {
                    throw e;
                }
            }

            else
            {
                // Creates new Formula object cell
                foreach (string variable in formula.GetVariables())
                {
                    // Checks if there is a circular dependency
                    if (graph.GetDependees(name).Contains(variable))
                        throw new CircularException();

                    graph.AddDependency(name, variable);
                }
                cells.Add(name, new Cell(formula, cells));
                Changed = true;
                return DependencyListGenerator(name);
            }
        }

        /// <summary>
        /// Gets the dependees of the cell name in the dependency graph
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <returns> dependee list of the cell name </returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependees(name);
        }

        /// <summary>
        /// Generates a dependency list of the cell name which includes the dependees
        /// </summary>
        /// <param name="name"> List of cell names to create dependency list </param>
        /// <returns> dependency list of the cell name </returns>
        private IList<string> DependencyListGenerator(string name)
        {
            List<string> dependentList = new List<string> { name };

            // Checks if current name has any dependees
            while (graph.HasDependees(name))
            {
                foreach (string dependee in graph.GetDependees(name))
                {
                    dependentList.Add(dependee);
                    name = dependee;
                }
            }

            return dependentList;
        }

        /// <summary>
        /// Gets the spreadsheet version that was saved
        /// </summary>
        /// <param name="filename"> file path to get spreadsheet version </param>
        /// <returns> spreadsheet version </returns>
        /// <exception cref="SpreadsheetReadWriteException"> Thrown if reading is disrupted or the version name wasn't correct </exception>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    string version = "";
                    if (reader.IsStartElement())
                    {
                        version = reader.GetAttribute(0);
                    }

                    return version;
                }
            }
            catch (Exception er)
            {
                throw new SpreadsheetReadWriteException(er.ToString());
            }

        }

        /// <summary>
        /// Formats spreadsheet object into file using XML
        /// </summary>
        /// <param name="filename"> file path to save it as </param>
        /// <exception cref="SpreadsheetReadWriteException"> Thrown if writing is disrupted </exception>
        public override string GetXML()
        {

            using (var sw = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);

                    // Puts each non-empty cell as its own XML node
                    foreach (string cell in GetNamesOfAllNonemptyCells())
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", cell);
                        if (cells[cell].CellContent is Formula)
                            writer.WriteElementString("contents", "=" + cells[cell].CellContent.ToString());

                        else
                            writer.WriteElementString("contents", cells[cell].CellContent.ToString());
                        writer.WriteEndElement();
                    }

                    // Ends spreadsheet node
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    Changed = false;


                }
                return sw.ToString();
            }
        }


        /// <summary>
        /// Formats spreadsheet object into file using XML
        /// </summary>
        /// <param name="filename"> file path to save it as </param>
        /// <exception cref="SpreadsheetReadWriteException"> Thrown if writing is disrupted </exception>
        public override void Save(string filename)
        {
            try
            {
                string xmlContent = GetXML();
                File.WriteAllText(filename, xmlContent, Encoding.ASCII);
                Changed = false;
            }
            
            catch(Exception er)
            {
                throw new SpreadsheetReadWriteException( er.ToString());
            }
        }

        /// <summary>
        /// Reads spreadsheet file to create a new spreadsheet object
        /// </summary>
        /// <param name="filename"> file path to read </param>
        /// <exception cref="SpreadsheetReadWriteException"> Thrown if reading is disrupted </exception>
        private void ReadSavedFile(string filename)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                using (XmlReader reader = XmlReader.Create(filename, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.ReadToDescendant("name"))
                        {
                            string name = reader.ReadElementContentAsString();
                            string content = reader.ReadElementContentAsString();
                            SetContentsOfCell(name, content);
                        }
                    }
                }
            }
            catch (Exception er)
            {
                throw new SpreadsheetReadWriteException(er.ToString());
            }
        }

        /// <summary>
        /// Retrieves the value of the cell instead of the contents
        /// </summary>
        /// <param name="name"> valid cell name </param>
        /// <returns> value of cell in form of string, double, or FormulaError </returns>
        /// <exception cref="InvalidNameException"> Thrown if the name is invalid </exception>
        /// <exception cref="ArgumentException"> Thrown if variable is not a number </exception>
        public override object GetCellValue(string name)
        {
            if (!IsValidCellName(name) || !IsValid(name))
            {
                throw new InvalidNameException();
            }

            string normalizedName = Normalize(name);
            if (cells.ContainsKey(normalizedName))
            {
                if (cells[normalizedName].CellContent is Formula)
                {
                    cells[normalizedName].CellValue = FormulaEvaluation((Formula)cells[normalizedName].CellContent);
                    return cells[normalizedName].CellValue;
                }

                else if (cells[normalizedName].CellContent is double)
                    return (double)cells[normalizedName].CellValue;

                else
                    return cells[normalizedName].CellValue;
            }

            return "";
        }

        /// <summary>
        /// Checks if cell name is valid
        /// </summary>
        /// <param name="name"> cell name to check for validity </param>
        /// <returns> true if definition is met otherwise false </returns>
        private static Boolean IsValidCellName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z_]+\d+$");
        }

        /// <summary>
        /// Recalculates the 
        /// </summary>
        /// <param name="variablePlaceholder"> cell name to start evaluating formulas </param>
        private void CellRecalculation(string variablePlaceholder)
        {
            string newVariable = "";
            while (graph.HasDependees(variablePlaceholder))
            {
                foreach (string dependee in graph.GetDependees(variablePlaceholder))
                {
                    cells[dependee].CellValue = FormulaEvaluation((Formula)cells[dependee].CellContent);
                    newVariable = dependee;
                }
                variablePlaceholder = newVariable;
            }
        }
        /// <summary>
        /// Evaluates the formula
        /// </summary>
        /// <param name="formula"> formula to evaluate </param>
        /// <returns> double or FormulaError cell value </returns>
        /// <exception cref="ArgumentException"> Thrown when variable cell value isn't a number </exception>
        private object FormulaEvaluation(Formula formula)
        {
            return formula.Evaluate(variable => cells[variable].CellValue is double ?
                        (double)cells[variable].CellValue : throw new ArgumentException("Invalid Variable"));
        }
    }
}

