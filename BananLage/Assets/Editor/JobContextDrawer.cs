#if UNITY_EDITOR
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Jobs.Types;
using Mechanics.Production;
using Structures.Types;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class IHasJobContextEditor : Editor
{
    private bool multiShow = true;
    
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        base.OnInspectorGUI();

        if (target is not IJobContainer hasCtx)
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
        DrawBasics(ctx);
        
        switch (ctx)
        {
            case ProductionJobContext job:
                DrawProductionJobContext(job);
                break;
            case BuildJobContext job:
                DrawBuildJobContext(job);
                break;
            case IdlingJobContext job:
                DrawIdleJobContext(job);
                break;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBasics(JobContext job)
    {
        switch (job)
        {
            case SingleWorkerJobContext single:
                EditorGUILayout.LabelField("Worker", single.Worker != null ? single.Worker.Name : "None");
                break;
            case MultiWorkerJobContext multi:
            {
                multiShow = EditorGUILayout.BeginFoldoutHeaderGroup(multiShow,  "Workers");

                if (multiShow)
                {
                    var (_, avs, times) = multi;
                    EditorGUI.indentLevel++;
                    foreach (var m in multi.Monkeys.Values)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        EditorGUILayout.LabelField(m.Name, GUILayout.MaxWidth(100));
                        EditorGUILayout.LabelField(m.Self.transform.position.ToString(), GUILayout.MaxWidth(100));
                        EditorGUILayout.LabelField($"AV: {(avs.TryGetValue(m.Id, out var av) ? av.ToString() : "none")}", GUILayout.MaxWidth(100));
                        EditorGUILayout.LabelField($"Time: {(times.TryGetValue(m.Id, out var t) ? t.ToString() : "none")}", GUILayout.MaxWidth(100));
                        
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                break;
            }
        }

        EditorGUILayout.LabelField("Structure", job.Structure != null ? job.Structure.name : "None");
        EditorGUILayout.LabelField("Task Type", job.TaskType.ToString());
        EditorGUILayout.LabelField("Is Finished", job.IsFinished.ToString());
        EditorGUILayout.LabelField("Has Begun", job.HasBegun.ToString());
        EditorGUILayout.LabelField("Has Ended", job.HasEnded.ToString());
    }
    
    #region Production Job Context
    private void DrawProductionJobContext(ProductionJobContext job)
    {

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
        EditorGUILayout.LabelField("Total Time", job.Structure.StructureData.TotalBuildTime.ToString());
        EditorGUILayout.LabelField("AV", job.RemainingAVCost.ToString());
        EditorGUILayout.LabelField("AV", job._progressInternal.ToString());
        EditorGUILayout.LabelField("Step", job.Step().ToString());
        EditorGUILayout.LabelField("Is Finished", job.IsFinished.ToString());
        
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Available?", job.HasSpace.ToString());
        EditorGUILayout.Separator();
        
        var progressRect = GUILayoutUtility.GetRect(18, 18);
        var fill = Mathf.Clamp01(job.Progress / 1f);
        EditorGUI.ProgressBar(progressRect, fill, $"{job.Progress * 100:0}%");
    }
    #endregion
    
    #region Idle Job Context

    private void DrawIdleJobContext(IdlingJobContext job)
    {
        EditorGUILayout.LabelField("Elapsed Time", job.ElapsedTime.ToString());
        EditorGUILayout.LabelField("Created Time", job.CreateTime.ToString());
    }
    #endregion
    
    
}
#endif