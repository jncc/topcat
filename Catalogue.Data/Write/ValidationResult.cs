using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Model;
using Catalogue.Utilities.Expressions;
using Catalogue.Utilities.Text;
using System.Text.RegularExpressions;

namespace Catalogue.Data.Write
{
    public class ValidationResult<T>
    {
        public ValidationResult()
        {
            Errors = new ValidationIssueSet<T>();
            Warnings = new ValidationIssueSet<T>();
        }

        public ValidationIssueSet<T> Errors { get; private set; }
        public ValidationIssueSet<T> Warnings { get; private set; }

    }

    public class ValidationIssueSet<T> : Collection<ValidationIssue<T>>
    {
        public void Add(string message, params Expression<Func<T, object>>[] fields)
        {
            Add(new ValidationIssue<T>(message, fields.ToList()));
        }

        public void Append(ValidationIssueSet<T> source)
        {
            foreach (var issue in source)
            {
                Add(issue);
            }
        }
    }

    public class ValidationIssue<T>
    {
        public ValidationIssue(string message, List<Expression<Func<T, object>>> fields)
        {
            Message = message;
            FieldExpressions = fields;
        }

        public string Message { get; private set; }

        private List<Expression<Func<T, object>>> FieldExpressions { get; set; }

        /// <summary>
        ///     A representation of the property accessor expression(s) suitable for eg a json client.
        /// </summary>
        public List<string> Fields
        {
            get
            {
                return (from e in FieldExpressions
                        let expression = e.Body.RemoveUnary()
                        let fullDottedPath = Regex.Replace(expression.ToString(), @"^\w+\.", "",RegexOptions.IgnoreCase)
                        let camelCasedProperties = fullDottedPath.Split('.').Select(StringUtility.ToCamelCase)
                        select String.Join(".", camelCasedProperties))
                    .ToList();
            }
        }
    }
}