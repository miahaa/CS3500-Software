/// <summary>
/// Author:    Amber (Phuong) Tran 
/// Partner:   Thu Ha
/// Date:      3-March-2024
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
/// Basic requirement for assignment six. To be more specific, this file holds all
///  methods that connect GUI with spreadsheet class
///    
/// </summary>
///
using SpreadsheetUtilities;
using SS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GUI
{
    /// <summary>
    /// Partial class representing the main page of the application
    /// </summary>
    public partial class MainPage : ContentPage
    {
        // Initialize the spreadsheet and related variables
        Spreadsheet spreadsheet = new Spreadsheet();
        int boxHeight = 25;
        int boxWidth = 50;
        int numRows = 26;
        int numCols = 10;
        int asciiStart = 64;
        Editor selectedEditor;

        /// <summary>
        /// Constructor for the MainPage class
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            InitializeSpreadsheet();
        }

        /// <summary>
        /// Method to initialize the spreadsheet layout
        /// </summary>
        void InitializeSpreadsheet()
        {
            for (int i = 0; i <= numRows; i++)
            {
                String label = (i == 0 ? "" : Convert.ToChar(asciiStart + i).ToString());

                TopLabels.Add(
                    new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = boxHeight,
                        WidthRequest = boxWidth,
                        HorizontalOptions = LayoutOptions.Center,
                        Content =
                            new Label
                            {
                                Text = $"{label}",
                                TextColor = Color.FromRgb(0, 0, 0),
                                BackgroundColor = Color.FromRgb(200, 200, 250),
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                    }
                );
            }

            // Setup row headers
            for (int i = 0; i < numCols; i++)
            {
                var rowHeaders = new HorizontalStackLayout();

                // Heading of the row
                Border rowHeader = new Border
                {
                    Stroke = Color.FromRgb(0, 0, 0),
                    StrokeThickness = 1,
                    HeightRequest = boxHeight,
                    WidthRequest = boxWidth,
                    HorizontalOptions = LayoutOptions.Center,
                    Content =
                            new Label
                            {
                                Text = $"{i + 1}",
                                TextColor = Color.FromRgb(0, 0, 0),
                                BackgroundColor = Color.FromRgb(200, 200, 250),
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                };
                rowHeaders.Add(rowHeader);

                // Row text boxes
                for (int j = 0; j <= numRows - 1; j++)
                {
                    String label = $"{Convert.ToChar(asciiStart + j + 1)}{i + 1}";

                    Editor gridCell = new Editor
                    {
                        BackgroundColor = Color.FromRgb(240, 240, 260),
                        TextColor = Color.FromRgb(0, 0, 0),
                        HorizontalTextAlignment = TextAlignment.Start,
                        ClassId = label,
                    };

                    // Initialize the selected editor
                    if (label == "A1")
                    {
                        gridCell = new Editor
                        {
                            BackgroundColor = Color.FromRgb(215, 215, 245),
                            TextColor = Color.FromRgb(0, 0, 0),
                            HorizontalTextAlignment = TextAlignment.Start,
                            ClassId = $"{Convert.ToChar(asciiStart + j + 1)}{i + 1}"
                        };
                        selectedEditor = gridCell;
                    }

                    gridCell.TextChanged += GridCellTextChanged;

                    Border border = new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = boxHeight,
                        WidthRequest = boxWidth,
                        HorizontalOptions = LayoutOptions.Start,
                        Content = gridCell,
                        ClassId = $"{Convert.ToChar(asciiStart + j + 1)}{i + 1}",
                    };


                    rowHeaders.Add(border);
                };
                Grid.Children.Add(rowHeaders);
            }

            spreadsheet = new Spreadsheet(s => ValidCellName(s), s => s.ToUpper(), "six");
        }

        // Event handler for when the contents of the grid change
        async void GridContentsChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    string cellName = selectedEditor.ClassId;
                    string newCellContents = cellContentsEditor.Text;
                    var insertCell = spreadsheet.SetContentsOfCell(cellName, newCellContents);
                    var cellValue = spreadsheet.GetCellValue(cellName);
                    int currentRow = int.Parse(cellName.Substring(1, cellName.Length - 1)) - 1;

                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        selectedEditor.Text = newCellContents;
                        selectedCell.Text = $"{cellName} f(c)=${cellValue}";
                    });

                });
            }
            catch (Exception except)
            {

            }
        }

        /// <summary>
        /// Event handler for when the text in a grid cell changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void GridCellTextChanged(object sender, TextChangedEventArgs e)
        {
            string changedText = e.NewTextValue;
            cellContentsEditor.IsEnabled = true;
            selectedEditor.BackgroundColor = Color.FromRgb(240, 240, 260);
            selectedEditor = (Editor)sender;
            selectedEditor.BackgroundColor = Color.FromRgb(215, 215, 245);

            String cellName = selectedEditor.ClassId;
            try
            {
                await Task.Run(() =>
                {
                    var insertCell = spreadsheet.SetContentsOfCell(cellName, changedText);
                    var addedCell = spreadsheet.GetCellValue(cellName);
                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        if (addedCell is FormulaError)
                        {
                            selectedCell.Text = $"{cellName} !Formula Error";
                        }
                        else
                        {
                            selectedCell.Text = $"{cellName} f(x)= {addedCell}";
                            cellContentsEditor.Text = changedText;
                            selectedEditor.Text = changedText;
                        }
                    });
                });
            }
            catch (Exception except)
            {
                selectedCell.Text = $"Formula format error {except.Message} ";
            }
        }

        /// <summary>
        /// Event handler for the autofill feature
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void AutoFillFeature(object sender, EventArgs e)
        {
            string row = AutoFillColumnName.Text;
            string val = AutoFillColumnValue.Text;

            if (row == "" || val == "")
                return;

            if (row.Length > 1)
                return;

            spreadsheet.SetContentsOfCell(row + "1", val);
            string cellValue = spreadsheet.GetCellValue(row + "1").ToString();
            int colNum = ((int)row[0] - asciiStart);

            await Task.Run(() =>
            {
                for (int i = 0; i < numRows; i++)
                {
                    if (i > 1)
                        spreadsheet.SetContentsOfCell(row + i.ToString(), val);

                    HorizontalStackLayout gridRow = (HorizontalStackLayout)Grid.Children.ElementAt(i);
                    object element = gridRow.ElementAt(colNum);
                    if (element is Border border && border.Content is Editor editor)
                    {
                        Device.InvokeOnMainThreadAsync(() =>
                        {
                            editor.Text = cellValue;
                        });
                    }
                }
            });

        }

        /// <summary>
        /// Method to validate cell names using regex
        /// </summary>
        /// <param name="name">The cell name to validate</param>
        /// <returns>True if the cell name is valid, otherwise false</returns>
        private Boolean ValidCellName(string name)
        {
            Regex legalName = new Regex(@"^[a-zA-Z]\d{1,2}$");
            return legalName.IsMatch(name);
        }

        // ---------File Methods

        // Method to clear the grid
        void clearGrid()
        {
            foreach (var child in Grid.Children)
            {
                if (child is HorizontalStackLayout rowLayout)
                {
                    foreach (var element in rowLayout.Children)
                    {
                        if (element is Border border && border.Content is Editor editor)
                        {
                            editor.Text = ""; // Clear the text in the editor
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to handle saving the file asynchronously
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void FileSaveAsync(object sender, EventArgs e)
        {
            //Check if data needs to be saved
            if (spreadsheet.Changed == false)
                return;

            try
            {
                string fileName = await DisplayPromptAsync("File name", "Enter file name", "Save", "Cancel");

                if (fileName == "")
                {
                    return;
                }
                if (!fileName.EndsWith(".sprd"))
                    fileName += ".sprd";
                if (File.Exists(fileName))
                {
                    bool overwrite = await DisplayAlert("Overwritting file", "Do you want to overwrite this file " + fileName, "Yes", "No");
                    if (!overwrite)
                    {
                        return;
                    }
                }
                string FilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileName;
                spreadsheet.Save(FilePath);
            }
            catch (Exception ex)
            {
                await DisplayAlert("File save error", ex.Message, "Continue");
            }
        }

        /// <summary>
        /// Method to handle opening a file asynchronously
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void FileMenuOpenAsync(object sender, EventArgs e)
        {
            if (spreadsheet.Changed)
            {
                bool alert = await DisplayAlert("Unsaved Data", "Do you want to save your changes?", "Save", "Don't save");
                if (!alert)
                    return;
            }
            clearGrid();
            var fileSys = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose spreadsheet file",
            });
            var spreadSheetFile = fileSys.FullPath.ToString();

            try
            {
                spreadsheet = new Spreadsheet(spreadSheetFile, s => ValidCellName(s), s => s, "six");
                IEnumerable<string> cellNames = spreadsheet.GetNamesOfAllNonemptyCells();
                Dictionary<string, string> cellMapping = new Dictionary<string, string>();
                //Put all grid values into a dictionary
                foreach (string cellName in cellNames)
                {
                    string cellValue = spreadsheet.GetCellValue(cellName).ToString();
                    int currentRow = int.Parse(cellName.Substring(1, cellName.Length - 1)) - 1;

                    HorizontalStackLayout row = (HorizontalStackLayout)Grid.Children.ElementAt(currentRow);

                    foreach (var element in row.Children)
                    {
                        if (element is Border border && border.Content is Editor editor)
                        {
                            if (editor.ClassId == cellName)
                            {
                                border.Content = new Editor
                                {
                                    BackgroundColor = Color.FromRgb(240, 240, 260),
                                    TextColor = Color.FromRgb(0, 0, 0),
                                    HorizontalTextAlignment = TextAlignment.Start,
                                    ClassId = cellName,
                                    Text = cellValue
                                };
                            }
                        }

                    }
                }

            }
            catch (Exception)
            {
                await DisplayAlert("File reading error", "", "Continue");
            }
            return;
        }

        /// <summary>
        /// Method to handle creating a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void FileMenuNew(object sender, EventArgs e)
        {
            if (spreadsheet.Changed)
            {
                bool alert = await DisplayAlert("Unsaved Data", "Do you want to save your changes?", "Save", "Don't save");
                if (!alert)
                    return;
            }
            // Clear the content of all grid cells
            clearGrid();

            // Clear the content of the spreadsheet
            spreadsheet = new Spreadsheet(s => ValidCellName(s), s => s.ToUpper(), "six");
        }

        /// <summary>
        /// Searches for a cell value based on the provided cell name and updates the UI accordingly.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">Event arguments</param>
        void SearchValue(object sender, EventArgs e)
        {
            string cellName = SearchCellName.Text;
            if (cellName == "") return;

            try
            {
                string cellValue = spreadsheet.GetCellValue(cellName).ToString();
                string cellContents = spreadsheet.GetCellContents(cellName).ToString();

                int colNum = ((int)cellName[0] - asciiStart);
                int rowNum = int.Parse(cellName.Substring(1));
                HorizontalStackLayout gridRow = (HorizontalStackLayout)Grid.Children.ElementAt(rowNum - 1);
                object element = gridRow.ElementAt(colNum);
                if (element is Border border && border.Content is Editor editor)
                {
                    // SETUP THE SELECTED EDITOR CELL
                    //editor.Text = cellValue;
                    selectedEditor.BackgroundColor = Color.FromRgb(240, 240, 260);
                    selectedEditor = editor;
                    selectedEditor.BackgroundColor = Color.FromRgb(210, 210, 245);
                }
                selectedCell.Text = $"{cellName} f(x)= {cellValue}";
                cellContentsEditor.Text = cellContents;
            }
            catch (Exception except)
            {

            }
        }

        /// <summary>
        /// Displays help document
        /// </summary>
        public void HelpClicked(Object sender, EventArgs e)
        {
            DisplayAlert("Guide",
                "To begin a new spreadsheet, navigate to the \"File\" menu and select \"New\".\r\n" +
                "To open an existing spreadsheet, access the \"File\" menu and choose \"Load\".\r\n" +
                "To save your work, simply click on \"File\" and then select \"Save\".\r\n\r\n" +
                "To modify a selected cell, hover your cursor over the desired cell and left-click to select it.\r\n" +
                "To edit the contents of a cell, locate the \"f(x)\" entry at the top-middle, input your desired value, and press Enter.\r\n\r\n" +
                "An advanced feature enables precise value searches within the spreadsheet. To utilize this tool, navigate to the \"Find\" option in the menu bar, then select \"Find Value\". A new window will appear, prompting you to enter the target value. After inputting the value, clicking the find button will highlight all corresponding cells. To remove the highlights, access the \"Find\" menu again and choose \"Clear Find\".", "OK");
        }

    }
}
