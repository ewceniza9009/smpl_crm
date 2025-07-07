using System;
using System.Linq.Expressions;

/// <summary>
/// Contains reusable expressions that can be translated into SQL by LINQ.
/// </summary>
public static class SqlReadyExpressions
{
    /// <summary>
    /// An expression that extracts text from within parentheses or returns the original string.
    /// This can be executed on the database server via LINQ.
    /// </summary>
    public static readonly Expression<Func<string, string>> GetTextInParenthesesOrFullString =
        (input) =>
            // This ternary expression is translated by LINQ into a SQL CASE statement.
            (input.Contains("(") && input.IndexOf(")") > input.IndexOf("("))
                // This part runs if the condition is true
                ? input.Substring(
                    input.IndexOf('(') + 1,
                    input.IndexOf(')') - input.IndexOf('(') - 1
                  )
                // This part runs if the condition is false
                : input;
}