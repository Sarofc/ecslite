using Saro.Entities.Collections;
using Saro.SEditor;
using UnityEditor;

namespace Saro.Entities.Inspectors
{
    public class IntrusiveListPropertyEditor : ObjectDrawer<IntrusiveList>
    {
        private bool m_Foldout = true;
        public override void OnGUI(string label, ref IntrusiveList instance, object context)
        {
            if (instance.Count == 0)
            {
                EditorGUILayout.LabelField("List is empty", EditorStyles.boldLabel);
            }
            else
            {
                m_Foldout = EditorGUILayout.Foldout(this.m_Foldout, $"Count: {instance.Count}", true, EditorStyles.foldoutHeader);
                if (m_Foldout)
                {
                    int i = 0;
                    foreach (var item in instance)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Element {i}");
                        EcsEntityInspector.DrawEntity(item);
                        EditorGUILayout.EndHorizontal();
                        ++i;
                    }
                }
            }
        }
    }
}