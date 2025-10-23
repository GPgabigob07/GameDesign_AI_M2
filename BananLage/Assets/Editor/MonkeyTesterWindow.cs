using AI.Monkey;
using UnityEditor;
using UnityEngine;

namespace Mechanics.Editor
{
    public class MonkeyTesterWindow : EditorWindow
    {
        private MonkeyGlobalConfig config;
        private MonkeyCharacterBT monkeyBt;
        private MonkeyData testMonkey;
        private TaskType selectedTask = TaskType.Combat;
        private int nominalCost = 10;
        private int points = 10;
        private Vector2 scroll;

        [MenuItem("Tools/Monkey Tester")]
        public static void OpenWindow()
        {
            GetWindow<MonkeyTesterWindow>("Monkey Tester");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            GUILayout.Label("Monkey Global Config", EditorStyles.boldLabel);
            config = (MonkeyGlobalConfig)EditorGUILayout.ObjectField(config, typeof(MonkeyGlobalConfig), false);

            monkeyBt = (MonkeyCharacterBT)EditorGUILayout.ObjectField(monkeyBt, typeof(MonkeyCharacterBT), true);

            if (config == null)
            {
                EditorGUILayout.HelpBox("Assign a MonkeyGlobalConfig asset first.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("Create Test Monkey", EditorStyles.boldLabel);
            points = EditorGUILayout.IntField("Prowess Points", points);

            if (GUILayout.Button("Create Monkey"))
            {
                testMonkey = config.CreateMonkey(points);
            }

            if (testMonkey != null)
            {
                if (monkeyBt)
                {
                    monkeyBt.OriginalData = testMonkey;
                    monkeyBt.CycleData = testMonkey;
                }
                
                GUILayout.Space(10);
                GUILayout.Label("Monkey Stats", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name", testMonkey.Name);
                EditorGUILayout.LabelField("Gender", testMonkey.Gender.ToString());
                EditorGUILayout.LabelField("UUID", testMonkey.UUID.ToString());
                EditorGUILayout.LabelField("HP", testMonkey.Hp.ToString());
                EditorGUILayout.LabelField("Action Value", testMonkey.ActionValue.ToString());
                EditorGUILayout.LabelField("Age", testMonkey.Age.ToString());
                GUILayout.Label("Prowess", EditorStyles.boldLabel);
                foreach (TaskType type in typeof(TaskType).GetEnumValues())
                {
                    EditorGUILayout.LabelField(type.ToString(), testMonkey.Prowess[type].ToString());
                }

                GUILayout.Space(10);
                GUILayout.Label("Test Task Execution", EditorStyles.boldLabel);
                selectedTask = (TaskType)EditorGUILayout.EnumPopup("Task", selectedTask);
                nominalCost = EditorGUILayout.IntField("Nominal Cost", nominalCost);

                /*if (GUILayout.Button("Check Can Execute"))
                {
                    bool can = config.CanExecuteTask(testMonkey, selectedTask, nominalCost, out int effectiveCost);
                    EditorUtility.DisplayDialog(
                        "Task Execution",
                        $"Can Execute: {can}\nEffective Cost: {effectiveCost}",
                        "OK"
                    );
                }*/
            }

            EditorGUILayout.EndScrollView();
        }
    }
}