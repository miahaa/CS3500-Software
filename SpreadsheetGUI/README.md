```
Author:     Mia Ha
Partner:    Amber (Phuong) Tran
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  Amberchen8892
Repo:       https://github.com/uofu-cs3500-spring24/spreadsheet-Amberchen8892
Date:       3-3-2024 5:11  (when submission was completed) 
Project:    Spreadsheet
Copyright:  CS 3500 and [Thu Ha, Amber Tran] - This work may not be copied for use in Academic Coursework.
```

# Comments to Evaluators:

Me and my partner met up and wrote code on the same laptop so we did not have a lot of commit from different user.
In this assignment, we kept running into some bugs that made our spreadsheet crash. It took us a lot of time trying
to fix but we still could not figure out.

# Assignment Specific Topics
Any additional feedback or write up required by the assignment.

Leave a space for a new paragraph.

Make sure you spell check your file (did you install the VS spell checker?) and use your best writing ability.

# Consulted Peers:

None

# Partnership
We did work well and efficiently with each other (in my opinion). Almost all of the code was created via pair programming 
as my partner cannot run the code on her computer so we met up and worked on the same laptop. Amber did more work with 
setting up spreadsheet, creating grid and check for valid name and handle exceptions. Mia worked on save, open and create file 
methods as well as fixing, debugging and optimizing the code.

# Branching
Most of the time we worked on the same computer. However, sometimes when we got a new idea, we splitted up to avoid effecting 
previous code. In general, we both worked via pulling work from Amber's branch, created a new branch based on this code to debug and 
optimize code. Then after done with the code, we merged it back to the main branch to avoid conflicting branches and messed up everything.

# Additional Features and Design Decisions
Even though we did note down our progress when we working on this assignment, it's hard to describing all of our creation process.
- For the spreadsheet representation: we decided to use the grid layout ('Grid') with rows and columns for cells. Each cell is represented 
  by an Editor control within a Border control for styling.
- Upon app start, the InitializeSpreadsheet() method sets up the initial state of the spreadsheet. Creates rows and columns with Labels and
  Editors, organized within Borders for structure. Initializing a new Spreadsheet object to handle cell formulas and values.
- When a user types into a cell (Editor), the GridCellTextChanged event triggers. It updates the corresponding cell in the Spreadsheet object 
  with the new text. It then updates the UI to display the result of any formulas or errors in a separate Label (selectedCell).
- Added a feature to show a save button when changes are made to the spreadsheet. Utilize this by calling IsChanged(true) when a cell's text is 
  changed, indicating unsaved changes. The button should become visible to prompt the user to save their work.
- Open Existing File: Click on a menu item linked to FileMenuOpenAsync. This will open the file picker to choose a spreadsheet file. 
  Selected file data will load into the grid, updating the UI with the spreadsheet's contents.
- Create New File: Click on a menu item linked to FileMenuNew.This will clear all cells in the grid and create a new, empty spreadsheet.Useful for 
  starting fresh work or clearing the current sheet for new data.
- Adding Search box and button
- Autofill value of selected column

# Time Tracking (Personal Software Practice)
- Initially, we thought we would finish in 15 or 20 hours and we even assumed that it was whole lot of time.
- Then it turned out to much more hour we need to spend on it (like around 24 hours). We have been chill in previous assignments until
  this one. We need to read and researched a bunch of documents, posts and websites to figure out how to connect everything smoothly.
  We did learn a lot during this assignment.
- I cannot tell exactly the time spent of each as we both worked together. Even when we are at home, we still facetime and screensharing to discuss 
  and fix code together. 

# Best (Team) pratice
The partnership proved most effective when we divided the tasks based on our strengths and areas of expertise. Like I said, Amber was more 
comfortable with UI design and XAML, while I had a stronger grasp of the Spreadsheet logic and backend workings. By assigning and splitting parts for each, I
work on implementing the UI-related features such as adding a button for edited spreadsheets and designing the save function UI, and Amber
focus on debugging the formula evaluator and implementing the "Update cell info from textbox" feature, we were able to work concurrently, effectively 
utilizing our individual skills. This division of labor not only made the coding process faster but also ensured that each aspect of the code received 
focused attention from the partner most suited to the task.

One area where we recognized the need for improvement was in setting clearer deadlines and task dependencies. There were instances where we both worked on 
separate features assuming they could be integrated seamlessly, only to find out later that some dependencies were overlooked. This led to some backtracking 
and adjustments in the code, which could have been avoided with better planning. Moving forward, we aim to establish a more structured task allocation system, 
with clearly defined deadlines for each component. Additionally, we plan to discuss potential dependencies before starting individual tasks to ensure smoother 
integration and minimize rework. This will not only improve our efficiency but also enhance the overall quality of the codebase.

# References:

    1. Lecture slides and Reference link
    2. Create XML in C# - https://www.c-sharpcorner.com/UploadFile/mahesh/create-xml-in-C-Sharp/
    3. Recommend XML tag for C# - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
    4. Encoding UTF-8 - https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.utf8?view=net-8.0
    5. Function lookup<> - https://stackoverflow.com/questions/5261846/a-function-lookup-table-in-place-of-switches
