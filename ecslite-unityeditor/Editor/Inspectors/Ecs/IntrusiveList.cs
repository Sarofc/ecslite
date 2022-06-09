using UnityEditor;

namespace Saro.Entities.UnityEditor.Inspectors
{
    public class IntrusiveListPropertyEditor : EcsComponentInspectorTyped<Collections.IntrusiveList>
    {
        private bool m_Foldout;

        protected override bool OnGuiTyped(string label, ref Collections.IntrusiveList value,
            EcsEntityDebugView entityView)
        {
            if (value.Count == 0)
            {
                EditorGUILayout.LabelField("List is empty", EditorStyles.boldLabel);
            }
            else
            {
                m_Foldout = EditorGUILayout.Foldout(m_Foldout, $"Count: {value.Count}", true, EditorStyles.foldoutHeader);

                if (m_Foldout)
                {
                    int i = 0;
                    foreach (var item in value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Element {i}");
                        EcsPackedEntityWithWorldInspector.DrawEntity(item, entityView);
                        EditorGUILayout.EndHorizontal();
                        ++i;
                    }
                }
            }

            return false;
        }
    }
}