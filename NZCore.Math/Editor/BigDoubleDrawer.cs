using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace NZCore.Editor
{
    [CustomPropertyDrawer(typeof(BigDouble))]
    public class BigDoubleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mantissaProp = property.FindPropertyRelative("_mantissa");
            var exponentProp = property.FindPropertyRelative("_exponent");

            double mantissa = mantissaProp.doubleValue;
            long exponent = exponentProp.longValue;

            string display = FormatBigDouble(mantissa, exponent);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            string input = EditorGUI.DelayedTextField(position, label, display);

            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    var parsed = BigDouble.Parse(input);
                    mantissaProp.doubleValue = parsed.Mantissa;
                    exponentProp.longValue = parsed.Exponent;
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Could not parse '{input}' as BigDouble.");
                }
            }

            EditorGUI.EndProperty();
        }

        private static string FormatBigDouble(double mantissa, long exponent)
        {
            if (mantissa == 0)
                return "0";

            if (exponent == long.MaxValue)
                return mantissa < 0 ? "-Inf" : "Inf";

            if (exponent == 0)
                return mantissa.ToString("G", CultureInfo.InvariantCulture);

            return $"{mantissa.ToString("G", CultureInfo.InvariantCulture)}e{exponent}";
        }
    }
}