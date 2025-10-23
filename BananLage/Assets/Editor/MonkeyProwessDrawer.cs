#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using Mechanics;

[CustomPropertyDrawer(typeof(MonkeyProwess))]
public class MonkeyProwessDrawer : PropertyDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
    private const float BarHeight = 8f;
    private const float Padding = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Height = header + each task line
        var listProp = property.FindPropertyRelative("taskEntries");
        int count = listProp != null ? listProp.arraySize : 0;
        return LineHeight + Padding + (LineHeight + BarHeight + Padding) * count;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var rect = new Rect(position.x, position.y, position.width, LineHeight);

        // Header
        EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        rect.y += LineHeight + Padding;

        var listProp = property.FindPropertyRelative("taskEntries");
        if (listProp == null || listProp.arraySize == 0)
        {
            EditorGUI.HelpBox(rect, "No task entries found", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        // Get runtime MonkeyProwess if possible (to calculate Most/Least)
        var target = property.serializedObject.targetObject;
        MonkeyProwess runtime = null;
        try { runtime = fieldInfo.GetValue(target) as MonkeyProwess; } catch { }

        TaskType most = runtime?.Most() ?? default;
        TaskType least = runtime?.Least() ?? default;

        // Iterate over serialized list
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var entryProp = listProp.GetArrayElementAtIndex(i);
            var taskProp = entryProp.FindPropertyRelative("task");
            var pointsProp = entryProp.FindPropertyRelative("points");
            var enabledProp = entryProp.FindPropertyRelative("enabled");

            if (taskProp == null || pointsProp == null || enabledProp == null) continue;

            TaskType task = (TaskType)taskProp.enumValueIndex;
            int points = pointsProp.intValue;
            bool enabled = enabledProp.boolValue;

            // Layout
            float toggleWidth = 18f;
            float inputWidth = 40f;
            float labelWidth = 80f;

            var labelRect = new Rect(rect.x, rect.y, labelWidth, LineHeight);
            var toggleRect = new Rect(labelRect.xMax + 4, rect.y, toggleWidth, LineHeight);
            var inputRect = new Rect(toggleRect.xMax + 4, rect.y, inputWidth, LineHeight);
            var barRect = new Rect(inputRect.xMax + 6, rect.y + 2, rect.width - inputRect.xMax - 10, BarHeight);

            // Highlight Most/Least
            Color barColor = enabled ? Color.green : Color.red;
            if (task == most) barColor = Color.yellow;
            else if (task == least) barColor = new Color(0.5f, 0.5f, 0.5f);

            // Draw task label
            EditorGUI.LabelField(labelRect, task.ToString(), task == most ? EditorStyles.boldLabel : EditorStyles.label);

            // Enabled toggle
            enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabled);

            // Points input field
            pointsProp.intValue = EditorGUI.IntField(inputRect, pointsProp.intValue);

            // Draw bar (normalized to 0-100)
            float fill = Mathf.Clamp01(pointsProp.intValue / 100f);
            EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));
            EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height), barColor);

            rect.y += LineHeight + BarHeight + Padding;
        }

        EditorGUI.EndProperty();
    }
}
#endif
