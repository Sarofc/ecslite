// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using Saro.Entities.Extension;
using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor.Inspectors
{
    internal sealed class EcsPackedEntityInspector : EcsComponentInspectorTyped<EcsPackedEntity>
    {
        protected override bool OnGuiTyped(string label, ref EcsPackedEntity value, EcsEntityDebugView entityView)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            DrawEntity(ref value, entityView);

            EditorGUILayout.EndHorizontal();
            return false;
        }

        public static void DrawEntity(ref EcsPackedEntity value, EcsEntityDebugView entityView)
        {
            if (value.Unpack(entityView.world, out var unpackedEntity))
            {
                var _entityView = entityView.debugSystem.GetEntityView(unpackedEntity);
                var entityName = Name.GetEntityName(unpackedEntity, entityView.world);
                if (GUILayout.Button($"Ping [{entityName}]"))
                {
                    EditorGUIUtility.PingObject(_entityView);
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