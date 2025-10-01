using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Advanced ShowIf attribute that supports more complex conditions
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAdvancedAttribute : PropertyAttribute
    {
        public string[] ConditionNames { get; }
        public object[] CompareValues { get; }
        public ComparisonType Comparison { get; }
        public LogicalOperator LogicalOperator { get; }

        /// <summary>
        /// Show field if all conditions are true (AND logic)
        /// </summary>
        /// <param name="conditionNames">Names of the fields/properties/methods to check</param>
        public ShowIfAdvancedAttribute(params string[] conditionNames)
        {
            ConditionNames = conditionNames;
            CompareValues = new object[conditionNames.Length];
            for (int i = 0; i < CompareValues.Length; i++)
            {
                CompareValues[i] = true; // Default to true for boolean checks
            }
            Comparison = ComparisonType.Equals;
            LogicalOperator = LogicalOperator.And;
        }

        /// <summary>
        /// Show field with custom logical operator
        /// </summary>
        /// <param name="logicalOperator">How to combine multiple conditions</param>
        /// <param name="conditionNames">Names of the fields/properties/methods to check</param>
        public ShowIfAdvancedAttribute(LogicalOperator logicalOperator, params string[] conditionNames)
        {
            ConditionNames = conditionNames;
            CompareValues = new object[conditionNames.Length];
            for (int i = 0; i < CompareValues.Length; i++)
            {
                CompareValues[i] = true;
            }
            Comparison = ComparisonType.Equals;
            LogicalOperator = logicalOperator;
        }

        /// <summary>
        /// Show field if condition matches value
        /// </summary>
        /// <param name="conditionName">Name of the field/property/method</param>
        /// <param name="compareValue">Value to compare against</param>
        /// <param name="comparison">Type of comparison</param>
        public ShowIfAdvancedAttribute(string conditionName, object compareValue, ComparisonType comparison = ComparisonType.Equals)
        {
            ConditionNames = new[] { conditionName };
            CompareValues = new[] { compareValue };
            Comparison = comparison;
            LogicalOperator = LogicalOperator.And;
        }
    }
}