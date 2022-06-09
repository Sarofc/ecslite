// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using Saro.Entities.Extension;
using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor.Inspectors
{
    internal sealed class EcsPackedEntityWithWorldInspector : EcsComponentInspectorTyped<EcsPackedEntityWithWorld>
    {
        protected override bool OnGuiTyped(string label, ref EcsPackedEntityWithWorld value,
            EcsEntityDebugView entityView)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            DrawEntity(ref value, entityView);

            EditorGUILayout.EndHorizontal();
            return false;
        }

        public static void DrawEntity(EcsPackedEntityWithWorld value, EcsEntityDebugView entityView)
        {
            DrawEntity(ref value, entityView);
        }

        public static void DrawEntity(ref EcsPackedEntityWithWorld value, EcsEntityDebugView entityView)
        {
            if (value.Unpack(out var unpackedWorld, out var unpackedEntity))
            {
                if (unpackedWorld == entityView.world)
                {
                    var _entityView = entityView.debugSystem.GetEntityView(unpackedEntity);
                    var entityName = Name.GetEntityName(unpackedEntity, unpackedWorld);
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
                if (value.EqualsTo(default))
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