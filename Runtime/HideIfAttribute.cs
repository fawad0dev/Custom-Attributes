using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Hide the field in inspector if the specified condition is met
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HideIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; }
        public object CompareValue { get; }
        public ComparisonType Comparison { get; }
        
        /// <summary>
        /// Hide field if boolean condition is true
        /// </summary>
        /// <param name="conditionFieldName">Name of the boolean field/property/method</param>
        public HideIfAttribute(string conditionFieldName)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = true;
            Comparison = ComparisonType.Equals;
        }
        
        /// <summary>
        /// Hide field if condition matches the specified value
        /// </summary>
        /// <param name="conditionFieldName">Name of the field/property/method</param>
        /// <param name="compareValue">Value to compare against</param>
        public HideIfAttribute(string conditionFieldName, object compareValue)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = compareValue;
            Comparison = ComparisonType.Equals;
        }
        
        /// <summary>
        /// Hide field if condition matches the specified value with custom comparison
        /// </summary>
        /// <param name="conditionFieldName">Name of the field/property/method</param>
        /// <param name="compareValue">Value to compare against</param>
        /// <param name="comparison">Type of comparison to perform</param>
        public HideIfAttribute(string conditionFieldName, object compareValue, ComparisonType comparison)
        {
            ConditionFieldName = conditionFieldName;
            CompareValue = compareValue;
            Comparison = comparison;
        }
    }
}