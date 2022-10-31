// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor
{
    using Saro.SEditor;

    [CustomEditor(typeof(EcsEntityDebugView))]
    public sealed class EcsEntityDebugViewInspector : Editor
    {
        private static object[] s_ComponentsCache = new object[32];

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var debugView = (EcsEntityDebugView)target;
            if (debugView.entity.World != null)
            {
                var rect = EditorGUILayout.GetControlRect();

                const float buttonWidth = 24f;

                var entityInfoRect = rect;
                entityInfoRect.width -= buttonWidth;

                var buttonRect = rect;
                buttonRect.x += entityInfoRect.width;
                buttonRect.width = buttonWidth;

                EditorGUI.LabelField(entityInfoRect, Name.GetEntityInfo(debugView.entity));

                bool guiEnable = GUI.enabled;
                GUI.enabled = debugView.entity.IsAlive();
                if (GUI.Button(buttonRect, "-"))
                {
                    debugView.entity.Destroy();
                }
                GUI.enabled = guiEnable;

                DrawComponents(debugView);
                //EditorUtility.SetDirty(target);
            }
        }

        private static void DrawComponents(EcsEntityDebugView debugView)
        {
            if (debugView.gameObject.activeSelf)
            {
                var count = debugView.entity.GetComponents(ref s_ComponentsCache);
                for (var i = 0; i < count; i++)
                {
                    DrawComponent(s_ComponentsCache[i], debugView);

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