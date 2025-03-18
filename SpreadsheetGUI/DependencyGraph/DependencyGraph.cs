/// <summary>
/// Author:    Amber (Phuong) Tran
/// Partner:   -none-
/// Date:      25-Jan-2023
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
///    DependencyGrpah method that builds relationsip gragh for variable
///    
/// </summary>
///

// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta
// (Clarified meaning of dependent and dependee.)
// (Clarified names in solution/project structure.)
namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
///
/// A DependencyGraph can be modeled as a set of ordered pairs of strings. Two
/// ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1
/// equals t2.
/// Recall that sets never contain duplicates. If an attempt is made to add an
/// element to a
/// set, and the element is already in the set, the set remains unchanged.
///
/// Given a DependencyGraph DG:
///
/// (1) If s is a string, the set of all strings t such that (s,t) is in DG is
/// called dependents(s).
/// (The set of things that depend on s)
///
/// (2) If s is a string, the set of all strings t such that (t,s) is in DG is
/// called dependees(s).
/// (The set of things that s depends on)
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
// dependents("a") = {"b", "c"}
// dependents("b") = {"d"}
// dependents("c") = {}
// dependents("d") = {"d"}
// dependees("a") = {}
// dependees("b") = {"a"}
// dependees("c") = {"a"}
// dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph
{
    //Global variables required for primary functions 
    private Dictionary<String, HashSet<String>> dependents;
    private Dictionary<String, HashSet<String>> dependees;
    private int pairs;


    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    /// 
    public DependencyGraph()
    {
        //create a hashmap for both
        dependees = new Dictionary<String, HashSet<String>>();
        dependents = new Dictionary<String, HashSet<String>>();
        pairs = 0;
    }
    /// <summary>
    /// The number of ordered pairs in the DependencyGraph. 
    /// </summary>
    public int Size
    {
        get
        {
            return pairs;
        }
    }

    /// <summary>
    /// The size of dependees(s).
    /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
    /// invoke it like this:
    /// dg["a"]
    /// It should return the size of dependees("a")
    /// </summary>
    /// <param name="s">
    /// The dependees we want to get the count of
    /// </param>
    /// <returns>
    /// How many pairs are stored in dependees(s).
    /// </returns>
    public int this[string s]
    {
        get
        {
            if (!dependents.ContainsKey(s) | !isLegal(s))
                return 0;

            return dependents[s].Count();
        }
    }

    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        //check if string is legal, and if the dependents(s) set exists
        if (!dependees.ContainsKey(s) | !isLegal(s))
            return false;
        return true;
    }

    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        //Check if string is legal, and if the dependee(s) set exists
        if (!dependents.ContainsKey(s) | !isLegal(s))
            return false;
        return true;
    }

    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        {    //Check if string is legal, and if the dependents(s) set exists
            if (!dependees.ContainsKey(s) | !isLegal(s))
                return new HashSet<String>();
            return dependees[s];
        }
    }

    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {      //check if string is legal, and if the dependee(s) set exists
        if (!isLegal(s) | !dependents.ContainsKey(s))
            return new HashSet<String>();
        return dependents[s];
    }
    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>        /// 
    public void AddDependency(string s, string t)
    {
        //Checks both input strings to make sure they are legal, if not return
        if (!isLegal(s) | !isLegal(t))
            return;

        //If the dependees and dependents dictionaries do not contain the pair we will increment the pairs variable
        if ((!dependents.ContainsKey(t) || !dependees.ContainsKey(s)))
            pairs++;

        //T would be a dependent node since it depends on S, create or place T into appropriate dictionary
        if (!dependents.ContainsKey(t))
            dependents.Add(t, new HashSet<String>() { s });
        else
            //If t exists in the dependents dictionary it will need to be added as a dependees value to the HashSet
            dependents[t].Add(s);

        //S would be a Dependee since T relies on it, create or place S into the appropriate dictionary
        if (!dependees.ContainsKey(s))
            dependees.Add(s, new HashSet<String>() { t });
        else
            dependees[s].Add(t);
    }
    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        //If either strings are not legal return
        if (!isLegal(s) | !isLegal(t))
            return;

        //If the ordered pairs exist remove them
        if (dependents.ContainsKey(t) && dependents[t].Contains(s))
            if (dependees.ContainsKey(s) && dependees[s].Contains(t))
            {
                //Update the Dictionaries, Hashsets, and global variables.
                pairs--;
                dependents[t].Remove(s);
                dependees[s].Remove(t);

                //Remove any newly emptied lists from the dictionaries
                if (dependents[t].Count == 0)
                    dependents.Remove(t);
                if (dependees[s].Count == 0)
                    dependees.Remove(s);
            }

    }
    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        //If string is not legal return
        if (!isLegal(s))
            return;

        //Collect the pairs we want to empty from the graph
        HashSet<String> pairsToRemove = (HashSet<String>)GetDependents(s);

        //Empty the current set we want to replace
        foreach (string removeDependents in pairsToRemove)
            RemoveDependency(s, removeDependents);

        //Add desired elements
        foreach (string addDependent in newDependents)
            AddDependency(s, addDependent);
    }
    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        //If string is not legal return
        if (!isLegal(s))
            return;

        //Collect the set we want to empty from the graph
        HashSet<String> pairsToRemove = (HashSet<String>)GetDependees(s);

        //Empty the set we want to replace
        foreach (string removeDependents in pairsToRemove)
            RemoveDependency(removeDependents, s);

        //Add desired elements
        foreach (string addDependees in newDependees)
            AddDependency(addDependees, s);
    }

    /// <summary>
    /// A helper method which will chech if the string is legal to be placed into a dependencey graph
    /// </summary>
    /// <param name="s"> The string we are evulating 
    /// </param>
    /// <returns> True if the string is legal, false otherwise
    /// </returns>
    private Boolean isLegal(string s)
    {
        //Make sure to check for empty strings
        if (s != " ")
            if (s != "")
                return true;
        return false;
    }
}

