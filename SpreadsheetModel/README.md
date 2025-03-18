Author:     Ha Thu
Partner:    None
Start Date: 14/1/2024
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  miahaa
Repo:       https://github.com/uofu-cs3500-spring24/spreadsheet-miahaa.git
Commit Date:2-18-2024 5:11 pm (of when submission is ready to be evaluated)
Solution:   Spreadsheet
Copyright:  CS 3500 and [Thu Ha] - This work may not be copied for use in Academic Coursework.

# Overview of the Spreadsheet functionality
The Spreadsheet now inherit fromAbstractSpreadsheet and implement its abstract method so that it can control and handle infinite cells.
The Spreadsheet use DependencyGraph class to keep track of the relationships among cells and use
Formula class to represent formulas.
# Time Expenditures:

    Hours Estimated/Worked         Assignment                       Note
          15    /   20    - Assignment 1 - Formula Evaluator     Spent extra 5 hours writing test cases with throw exception and push it to github
          15    /   17    - Assignment 2 - Dependency Graph      Still waste time on posting git as the laptop I borrowed from Marriot Lib got some
                                                                 problems with vs so I need to make appointment with the IT.
          20    /   23-24 - Assignment 3 - Formula               This assignment took me longer than expected as there's a lot of unexpected problems 
                                                                 that I need to deal with
          20    /   21-22 - Assignment 4 - Spreadsheet           Everything is doing great as expected
          20    /   20    - Assignment 5 - Spreadsheet           Did spend a lot of time on get XML and Save and test coverage

# Examples of Good Software Practice (GSP)
  1. Code Reuse: In Spreadsheet class, I used DependencyGraph class to manage dependencies between cells instead of remaking a new one and I also used 
     Formula class to represent Formula when we want to set Cell content with a formula. By using that, I did reduce redundant code and also promotes 
     scalability.
  
  2. Documentation:  My code includes comprehensive XML documentation comments, providing clear explanations of the purpose and behavior of classes, 
     methods, and parameters.

  3. Well-Named Methods: Methods are named descriptively, indicating their purpose and functionally, enhancing code readability and understanding.