using Saro.Entities.UnityEditor;
using Saro.SEditor;
using UnityEditor;
using UnityEngine;

namespace Saro.Entities.Inspectors
{
    internal sealed class EcsEntityInspector : ObjectDrawer<EcsEntity>
    {
        public override void OnGUI(string label, ref EcsEntity instance, object context)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            DrawEntity(ref instance);

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawEntity(EcsEntity entity)
        {
            DrawEntity(ref entity);
        }

        public static void DrawEntity(ref EcsEntity value)
        {
            if (value.IsAlive())
            {
                if (value.World == value.World)
                {
                    var entityName = EntityName.GetEntityName(value.id, value.World);
                    if (GUILayout.Button($"Ping [{entityName}]"))
                    {
                        var debugViews = GameObject.FindObjectsOfType<EcsEntityDebugView>(true);
                        for (int i = 0; i < debugViews.Length; i++)
                        {
                            if (debugViews[i].entity == value)
                                EditorGUIUtility.PingObject(debugViews[i].gameObject);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.SelectableLabel("<External entity>",
                        GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                }
            }
            else
            {
                if (value == default)
                {
                    EditorGUILayout.SelectableLabel("<Empty entity>",
                        GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                }
                else
                {
                    EditorGUILayout.SelectableLabel("<Invalid entity>",
                        GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                }
            }
        }
    }
}