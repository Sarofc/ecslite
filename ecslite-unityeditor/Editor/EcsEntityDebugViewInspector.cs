// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Saro.Entities.UnityEditor
{
    using System.ComponentModel;
    using Extension;
    using Saro.SEditor;
    using static UnityEngine.GridBrushBase;

    [CustomEditor(typeof(EcsEntityDebugView))]
    public sealed class EcsEntityDebugViewInspector : Editor
    {
        private const int k_MaxFieldToStringLength = 128;
        private static object[] s_ComponentsCache = new object[32];

        public override void OnInspectorGUI()
        {
            var observer = (EcsEntityDebugView)target;
            if (observer.entity.World != null)
            {
                var rect = EditorGUILayout.GetControlRect();

                const float buttonWidth = 24f;

                var entityInfoRect = rect;
                entityInfoRect.width -= buttonWidth;

                var buttonRect = rect;
                buttonRect.x += entityInfoRect.width;
                buttonRect.width = buttonWidth;

                EditorGUI.LabelField(entityInfoRect, Name.GetEntityInfo(observer.entity));

                bool guiEnable = GUI.enabled;
                GUI.enabled = observer.entity.IsAlive();
                if (GUI.Button(buttonRect, "-"))
                {
                    observer.entity.Destroy();
                }
                GUI.enabled = guiEnable;

                DrawComponents(observer);
                EditorUtility.SetDirty(target);
            }
        }

        private static void DrawComponents(EcsEntityDebugView debugView)
        {
            if (debugView.gameObject.activeSelf)
            {
                var count = debugView.entity.GetComponents(ref s_ComponentsCache);
                for (var i = 0; i < count; i++)
                {
                    var component = s_ComponentsCache[i];
                    s_ComponentsCache[i] = null;

                    DrawComponent(component, debugView);

                    EditorGUILayout.Space();
                }
            }
        }

        private static void DrawComponent(object component, EcsEntityDebugView debugView)
        {
            var compType = component.GetType();
            GUILayout.BeginVertical("helpbox");
            var typeName = EditorExtensions.GetCleanGenericTypeName(compType);
            var pool = debugView.entity.World.GetPoolByType(compType);

            var headerRect = EditorGUILayout.GetControlRect();
            headerRect.xMin += EditorGUIUtility.singleLineHeight;

            var buttonRect = headerRect;
            buttonRect.xMin += headerRect.width - 24;

            var foldout = SEditorUtility.GetEditorFoldout(compType.FullName);
            //var newFoldout = EditorGUI.Foldout(headerRect, foldout, typeName, true, EditorStyles.foldoutHeader);
            var newFoldout = EditorGUI.BeginFoldoutHeaderGroup(headerRect, foldout, typeName);
            if (newFoldout != foldout)
            {
                foldout = newFoldout;
                SEditorUtility.SetEditorFoldout(compType.FullName, foldout);
            }
            if (GUI.Button(buttonRect, "-"))
            {
                pool.Del(debugView.entity.id);
            }

            if (foldout)
            {
                EditorGUI.indentLevel++;
                SEditorUtility.ShowAutoEditorGUI(component);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();

            GUILayout.EndVertical();
        }
    }
}