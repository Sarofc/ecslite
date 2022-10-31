// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using Saro.Entities.Extension;
using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor.Inspectors
{
    internal sealed class EcsEntityInspector : EcsComponentInspectorTyped<EcsEntity>
    {
        protected override bool OnGuiTyped(string label, ref EcsEntity value,
            EcsEntityDebugView entityView)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            DrawEntity(ref value, entityView);

            EditorGUILayout.EndHorizontal();
            return false;
        }

        public static void DrawEntity(EcsEntity value, EcsEntityDebugView entityView)
        {
            DrawEntity(ref value, entityView);
        }

        public static void DrawEntity(ref EcsEntity value, EcsEntityDebugView entityView)
        {
            if (value.IsAlive())
            {
                if (value.World == entityView.world)
                {
                    var _entityView = entityView.debugSystem.GetEntityView(value.id);
                    var entityName = Name.GetEntityName(value.id, value.World);
                    if (GUILayout.Button($"Ping [{entityName}]"))
                    {
                        EditorGUIUtility.PingObject(_entityView);
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