// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor
{
    using Saro.SEditor;
    using Sirenix.Utilities;

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

                EditorGUI.LabelField(entityInfoRect, debugView.entity.GetEntityInfo());

                bool guiEnable = UnityEngine.GUI.enabled;
                UnityEngine.GUI.enabled = debugView.entity.IsAlive();
                if (UnityEngine.GUI.Button(buttonRect, "-"))
                {
                    debugView.entity.Destroy();
                }
                UnityEngine.GUI.enabled = guiEnable;

                DrawComponents(debugView);
                //EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
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
            var pool = debugView.entity.World.GetOrAddPool(compType);

            var rect = EditorGUILayout.GetControlRect();
            var headerRect = rect;
            headerRect.xMin += EditorGUIUtility.singleLineHeight;
            headerRect.width -= 18f;

            var buttonRect = rect.AlignRight(18f);

            var foldout = SEditorUtility.GetEditorFoldout(compType.FullName);
            //var newFoldout = EditorGUI.Foldout(headerRect, foldout, typeName, true, EditorStyles.foldoutHeader);
            var newFoldout = EditorGUI.BeginFoldoutHeaderGroup(headerRect, foldout, typeName);
            if (newFoldout != foldout)
            {
                foldout = newFoldout;
                SEditorUtility.SetEditorFoldout(compType.FullName, foldout);
            }
            if (UnityEngine.GUI.Button(buttonRect, "-"))
            {
                pool.Del(debugView.entity.id);
                //GUIUtility.ExitGUI();
            }

            if (foldout)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUI.BeginChangeCheck();
                    SEditorUtility.ShowAutoEditorGUI(component);
                    if (EditorGUI.EndChangeCheck())
                        pool.SetRaw(debugView.entity.id, component);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();

            GUILayout.EndVertical();
        }
    }
}