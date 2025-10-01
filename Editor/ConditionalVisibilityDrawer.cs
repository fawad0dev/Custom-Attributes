using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomAttributes.Editor {
    /// <summary>
    /// Property drawer for ShowIf and HideIf attributes using SaintsField-inspired property navigation
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class ConditionalVisibilityDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            bool shouldShow = ShouldShowProperty(property);

            if (shouldShow) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (ShouldShowProperty(property)) {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool ShouldShowProperty(SerializedProperty property) {
            var showIfAttribute = attribute as ShowIfAttribute;
            var hideIfAttribute = attribute as HideIfAttribute;

            if (showIfAttribute != null) {
                bool conditionMet = EvaluateCondition(property, showIfAttribute.ConditionFieldName,
                    showIfAttribute.CompareValue, showIfAttribute.Comparison);
                return conditionMet;
            }

            if (hideIfAttribute != null) {
                bool conditionMet = EvaluateCondition(property, hideIfAttribute.ConditionFieldName,
                    hideIfAttribute.CompareValue, hideIfAttribute.Comparison);
                return !conditionMet;
            }

            return true;
        }

        private bool EvaluateCondition(SerializedProperty property, string conditionFieldName, object compareValue, ComparisonType comparison) {
            object conditionValue = GetConditionValue(property, conditionFieldName);

            if (conditionValue == null && compareValue == null)
                return comparison == ComparisonType.Equals;

            if (conditionValue == null || compareValue == null)
                return comparison == ComparisonType.NotEquals;

            switch (comparison) {
                case ComparisonType.Equals:
                    return conditionValue.Equals(compareValue);

                case ComparisonType.NotEquals:
                    return !conditionValue.Equals(compareValue);

                case ComparisonType.GreaterThan:
                    return CompareNumeric(conditionValue, compareValue) > 0;

                case ComparisonType.LessThan:
                    return CompareNumeric(conditionValue, compareValue) < 0;

                case ComparisonType.GreaterThanOrEqual:
                    return CompareNumeric(conditionValue, compareValue) >= 0;

                case ComparisonType.LessThanOrEqual:
                    return CompareNumeric(conditionValue, compareValue) <= 0;

                default:
                    return false;
            }
        }

        private object GetConditionValue(SerializedProperty property, string conditionFieldName) {
            try {
                // Get the target object and field info using SaintsField approach
                var (fieldOrProp, parent) = GetFieldInfoAndDirectParent(property);

                if (parent == null) {
                    return null;
                }

                // Look for the condition field on the parent object
                Type parentType = parent.GetType();
                FieldInfo conditionField = parentType.GetField(conditionFieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (conditionField != null) {
                    return conditionField.GetValue(parent);
                }

                PropertyInfo conditionProperty = parentType.GetProperty(conditionFieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (conditionProperty != null) {
                    return conditionProperty.GetValue(parent);
                }

                return null;
            } catch (Exception ex) {
                Debug.LogError($"Error getting condition value for {conditionFieldName}: {ex.Message}");
                return null;
            }
        }

        // SaintsField-inspired property navigation system
        private (FieldOrProp fieldOrProp, object parent) GetFieldInfoAndDirectParent(SerializedProperty property) {
            string originPath = property.propertyPath;
            string[] propPaths = originPath.Split('.');
            var (arrayTrim, propPathSegments) = TrimEndArray(propPaths);
            if (arrayTrim) {
                propPaths = propPathSegments;
            }

            return GetFieldInfoAndDirectParentByPathSegments(property, propPaths);
        }

        private (FieldOrProp fieldOrProp, object parent) GetFieldInfoAndDirectParentByPathSegments(
            SerializedProperty property, string[] pathSegments) {
            object sourceObj = property.serializedObject.targetObject;
            FieldOrProp fieldOrProp = default;

            bool preNameIsArray = false;
            foreach (string propSegName in pathSegments) {
                if (propSegName == "Array") {
                    preNameIsArray = true;
                    continue;
                }

                if (propSegName.StartsWith("data[") && propSegName.EndsWith("]")) {
                    if (!preNameIsArray) {
                        // This shouldn't happen with proper Unity property paths
                        Debug.LogWarning($"Unexpected array data segment without Array prefix: {propSegName}");
                    }
                    preNameIsArray = false;

                    int elemIndex = Convert.ToInt32(propSegName.Substring(5, propSegName.Length - 6));

                    object useObject;
                    if (fieldOrProp.FieldInfo == null && fieldOrProp.PropertyInfo == null) {
                        useObject = sourceObj;
                    } else {
                        useObject = fieldOrProp.IsField
                            ? fieldOrProp.FieldInfo.GetValue(sourceObj)
                            : fieldOrProp.PropertyInfo.GetValue(sourceObj);
                    }

                    sourceObj = GetValueAtIndex(useObject, elemIndex);
                    fieldOrProp = default;
                    continue;
                }

                preNameIsArray = false;

                if (sourceObj == null) {
                    return (default, null);
                }

                if (fieldOrProp.FieldInfo != null || fieldOrProp.PropertyInfo != null) {
                    sourceObj = fieldOrProp.IsField
                        ? fieldOrProp.FieldInfo.GetValue(sourceObj)
                        : fieldOrProp.PropertyInfo.GetValue(sourceObj);
                }

                fieldOrProp = GetFieldOrProp(sourceObj, propSegName);
            }

            return (fieldOrProp, sourceObj);
        }

        private (bool trimed, string[] propPathSegs) TrimEndArray(string[] propPathSegments) {
            int usePathLength = propPathSegments.Length;

            if (usePathLength <= 2) {
                return (false, propPathSegments);
            }

            string lastPart = propPathSegments[usePathLength - 1];
            string secLastPart = propPathSegments[usePathLength - 2];
            bool isArray = secLastPart == "Array" && lastPart.StartsWith("data[") && lastPart.EndsWith("]");
            if (!isArray) {
                return (false, propPathSegments);
            }

            string[] propPaths = new string[usePathLength - 2];
            Array.Copy(propPathSegments, 0, propPaths, 0, usePathLength - 2);
            return (true, propPaths);
        }

        private FieldOrProp GetFieldOrProp(object source, string name) {
            Type type = source.GetType();

            while (type != null) {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null) {
                    return new FieldOrProp(field);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (property != null) {
                    return new FieldOrProp(property);
                }

                type = type.BaseType;
            }

            throw new Exception($"Unable to get {name} from {source}");
        }

        private object GetValueAtIndex(object source, int index) {
            if (source is IList list) {
                if (index >= 0 && index < list.Count) {
                    return list[index];
                }
            }

            return null;
        }

        private int CompareNumeric(object value1, object value2) {
            if (value1 is IComparable comparable1 && value2 is IComparable comparable2) {
                return comparable1.CompareTo(comparable2);
            }

            return 0;
        }
    }

    // Helper struct for field/property reflection
    public readonly struct FieldOrProp {
        public readonly bool IsField;
        public readonly FieldInfo FieldInfo;
        public readonly PropertyInfo PropertyInfo;

        public FieldOrProp(FieldInfo fieldInfo) {
            IsField = true;
            FieldInfo = fieldInfo;
            PropertyInfo = null;
        }

        public FieldOrProp(PropertyInfo propertyInfo) {
            IsField = false;
            FieldInfo = null;
            PropertyInfo = propertyInfo;
        }

        public override string ToString() {
            return IsField ? FieldInfo.ToString() : PropertyInfo.ToString();
        }
    }
}