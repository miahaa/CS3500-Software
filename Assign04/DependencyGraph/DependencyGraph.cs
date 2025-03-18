/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [01/11/2024]
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
/// This class evaluates integer arithmetic expressions written using
/// standard infix notation (following precedence rules and
/// integer arithmetic).
/// </summary>
/// 
// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        // Create a private dictionary that map a string to the set of its dependents
        private Dictionary<string, HashSet<string>> dependents;
        // Create another private dictionary map a tring with the set that it depends on
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                int count = 0;
                foreach (var key in dependents)
                {
                    count += key.Value.Count;
                }
                return count;
            }

        }

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer. If dg is a DependencyGraph, you
        /// would invoke it like this: dg["a"]. It should return the size of dependees("a")
        /// </summary>    
        public int this[string s]
        {
            get
            {
                if (dependents.ContainsKey(s))
                    return dependents[s].Count;
                return 0;
            }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return dependees.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return dependents.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            List<string> dependentsList = new List<string>();
            //Check if s has dependents in DependencyGraph or not.
            if (dependees.ContainsKey(s))
            {
                HashSet<String> dependentsSet = dependees[s];
                dependentsList.AddRange(dependentsSet);
                return dependentsList;
            }
            else
                return dependentsList;
        }
        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            List<string> dependeesList = new List<string>();
            //Check if s has dependents in DependencyGraph or not.
            if (dependents.ContainsKey(s))
            {
                HashSet<String> dependeesSet = dependents[s];
                dependeesList.AddRange(dependeesSet);
                return dependeesList;
            }
            else
                return dependeesList;
        }
        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        ///
        /// <para>This should be thought of as:</para>
        ///
        /// t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param> ///
        public void AddDependency(string s, string t)
        {
            // if key elready exists then just add it to its set. Otherwise, creates
            // a new entry for s with a set containing t.
            if (dependees.ContainsKey(s))
            {
                dependees[s].Add(t);
            }
            else
            {
                //Add the ordered pair (s,t) to the list
                dependees.Add(s, new HashSet<string> { t });
            }

            // Similarly, if dependees contains t, adds s to its set. Otherwise,
            // creates a new entry for t with a set containing s.
            if (dependents.ContainsKey(t))
            {
                dependents[t].Add(s);
            }
            else
            {
                dependents.Add(t, new HashSet<string>() { s });
            }
        }
        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            // Removes the ordered pair (s, t) if it exists.
            // If dependents dictionary contains t, removes s from its set. If
            // the set becomes empty, removes the entry for t.
            if (dependents.ContainsKey(t))
            {
                dependents[t].Remove(s);
                if (dependents[t].Count == 0)
                    dependents.Remove(t);
            }

            // Similarly, if dependees dictionary contains s, removes t from its
            // set. If the set becomes empty, removes the entry for s.
            if (dependees.ContainsKey(s))
            {
                dependees[s].Remove(t);
                if (dependees[s].Count == 0)
                    dependees.Remove(s);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r). Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            IEnumerable<string> tempList = GetDependents(s).ToList();
            // Removes all existing dependents of s.
            foreach (string dependent in tempList)
            {
                RemoveDependency(s, dependent);
            }
            // Adds each string in newDependents as a dependent of s.
            foreach (string str in newDependents)
            {
                AddDependency(s, str);
            }
        }
        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s). Then, for each
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            IEnumerable<string> tempList = GetDependees(s).ToList();
            // Removes all existing dependees of s.
            foreach (string dependee in tempList)
            {
                RemoveDependency(dependee, s);
            }
            // Adds each string in newDependees as a dependee of s.
            foreach (string str in newDependees)
            {
                AddDependency(str, s);
            }
        }
    }
}