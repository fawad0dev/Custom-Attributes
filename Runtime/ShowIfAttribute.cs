using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Show the field in inspector if the specified condition is met
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; }
        public object CompareValue { get; }
        public ComparisonType Comparison { get; }
        
        /// <summary>
        /// Show field if boolean condition is true
        /// </summary>
        /// <param name="conditionFieldName">Name of the boolean field/property/method</param>
        public ShowIfAttribute(string conditionFieldName)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = true;
            Comparison = ComparisonType.Equals;
        }
        
        /// <summary>
        /// Show field if condition matches the specified value
        /// </summary>
        /// <param name="conditionFieldName">Name of the field/property/method</param>
        /// <param name="compareValue">Value to compare against</param>
        public ShowIfAttribute(string conditionFieldName, object compareValue)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = compareValue;
            Comparison = ComparisonType.Equals;
        }
        
        /// <summary>
        /// Show field if condition matches the specified value with custom comparison
        /// </summary>
        /// <param name="conditionFieldName">Name of the field/property/method</param>
        /// <param name="compareValue">Value to compare against</param>
        /// <param name="comparison">Type of comparison to perform</param>
        public ShowIfAttribute(string conditionFieldName, object compareValue, ComparisonType comparison)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = compareValue;
            Comparison = comparison;
        }
    }
}