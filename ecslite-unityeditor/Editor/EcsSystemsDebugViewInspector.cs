// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor
{
    [CustomEditor(typeof(EcsSystemsDebugView))]
    internal sealed class EcsSystemsDebugViewInspector : Editor
    {
        private IEcsSystem[] m_Systems;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var observer = (EcsSystemsDebugView)target;
            if (observer.ecsSystemsList != null)
            {
                DrawSystems(observer);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSystems(EcsSystemsDebugView debugView)
        {
            if (debugView.gameObject.activeSelf)
            {
                var ecsSytemsList = debugView.ecsSystemsList;

                for (int ii = 0; ii < ecsSytemsList.Count; ii++)
                {
                    var systems = ecsSytemsList[ii];

                    EditorGUILayout.BeginVertical("helpbox");
                    {
                        EditorGUILayout.LabelField($"{systems.SystemsName}:", EditorStyles.boldLabel);

                        var systemNum = systems.GetAllSystems(ref m_Systems);
                        for (int i = 0; i < systemNum; i++)
                        {
                            var system = m_Systems[i];
                            // feature
                            if (system is EcsSystemFeature _feature)
                            {
                                EditorGUILayout.BeginVertical("box");
                                {
                                    DrawSystem(_feature, EditorStyles.boldLabel);

                                    //EditorGUI.indentLevel += 1;
                                    for (int k = 0; k < _feature.Systems.Count; k++)
                                    {
                                        var subSystem = _feature.Systems[k];
                                        DrawSystem(subSystem, EditorStyles.label, 1);
                                    }
                                    //EditorGUI.indentLevel -= 1;
                                }
                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                DrawSystem(system, EditorStyles.label);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();

                    if (ii < ecsSytemsList.Count - 1)
                    {
                        EditorGUILayout.Space();
                    }
                }
            }
        }

        private void DrawSystem(IEcsSystem system, GUIStyle style, int indentLevel = 0)
        {
            if (system is IEcsRunSystem runSystem)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.xMin += indentLevel * 15f;
                var toggeRect = rect;
                const float k_ToggleWidth = 15f;
                toggeRect.width = k_ToggleWidth;
                runSystem.Enable = EditorGUI.Toggle(toggeRect, runSystem.Enable);

                var systemRect = rect;
                systemRect.x += k_ToggleWidth;
                systemRect.width = rect.width - k_ToggleWidth;
                EditorGUI.LabelField(systemRect, runSystem.GetType().Name, style: style);
            }
            else
            {
                EditorGUILayout.LabelField(system.GetType().Name, style: style);
            }
        }
    }
}