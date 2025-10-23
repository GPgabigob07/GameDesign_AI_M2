#if UNITY_EDITOR
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Jobs.Types;
using Mechanics.Production;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class IHasJobContextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        base.OnInspectorGUI();

        if (target is not IJobAware hasCtx)
            return; // Only draw if the component implements the interface

        var ctx = hasCtx.JobContext;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Job Context Debug", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (ctx == null)
        {
            EditorGUILayout.HelpBox("No JobContext assigned.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }
        
        switch (ctx)
        {
            case ProductionJobContext job:
                DrawProductionJobContext(job);
                break;
            case BuildJobContext job:
                DrawBuildJobContext(job);
                break;
        }

        EditorGUILayout.EndVertical();
    }

    #region Production Job Context
    private void DrawProductionJobContext(ProductionJobContext job)
    {
        
        EditorGUILayout.LabelField("Worker", job.Worker != null ? job.Worker.Name : "None");
        EditorGUILayout.LabelField("Structure", job.Structure != null ? job.Structure.name : "None");
        EditorGUILayout.LabelField("Task Type", job.TaskType.ToString());
        EditorGUILayout.LabelField("Is Finished", job.IsFinished.ToString());

        if (job.Output == null || job.Output.Count == 0)
        {
            EditorGUILayout.LabelField("(no outputs)");
            return;
        }

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Outputs", EditorStyles.boldLabel);

        foreach (var output in job.Output)
            DrawOutput(output);
    }

    private void DrawOutput(ResourceOutput output)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            if (output.output?.uiSprite != null)
            {
                var texture = output.output.uiSprite.texture;
                var rect = GUILayoutUtility.GetRect(40, 40, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.LabelField(output.output != null ? output.output.name : "Unknown Output",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Expected: {output.expectedAmount} | Effective: {output.effectiveAmount}");

            var progressRect = GUILayoutUtility.GetRect(18, 18);
            var fill = Mathf.Clamp01(output.Progress / 1f);
            EditorGUI.ProgressBar(progressRect, fill, $"{output.Progress * 100:0}%");
        }
    }
    
    #endregion
    
    #region Build Job Context

    private void DrawBuildJobContext(BuildJobContext job)
    {
        EditorGUILayout.LabelField("Structure", job.Structure ? job.Structure.name : "None");
        EditorGUILayout.LabelField("Task Type", job.TaskType.ToString());
        EditorGUILayout.LabelField("Total Time", job.Structure.StructureData.TotalBuildTime.ToString());
        EditorGUILayout.LabelField("AV", job.RemainingAVCost.ToString());
        EditorGUILayout.LabelField("AV", job._progressInternal.ToString());
        EditorGUILayout.LabelField("Step", job.Step().ToString());
        EditorGUILayout.LabelField("Is Finished", job.IsFinished.ToString());
        
        var progressRect = GUILayoutUtility.GetRect(18, 18);
        var fill = Mathf.Clamp01(job.Progress / 1f);
        EditorGUI.ProgressBar(progressRect, fill, $"{job.Progress * 100:0}%");
    }
    #endregion
    
    
}
#endif