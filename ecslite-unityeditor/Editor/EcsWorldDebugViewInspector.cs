// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor
{
    [CustomEditor(typeof(EcsWorldDebugView))]
    internal sealed class EcsWorldDebugViewInspector : Editor
    {
        private IEcsSystem[] m_Systems;
        private IEcsPool[] m_Pools;
        private EcsFilter[] m_Filters;
        private bool m_PoolFoldout = false;
        private bool m_FilterFoldout;

        public override void OnInspectorGUI()
        {
            var observer = (EcsWorldDebugView)target;
            if (observer.ecsWorld != null)
            {
                DrawComponents(observer);
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawComponents(EcsWorldDebugView debugView)
        {
            if (debugView.gameObject.activeSelf)
            {
                var world = debugView.ecsWorld;

                EditorGUILayout.LabelField("WorldName: " + world.worldName);
                EditorGUILayout.LabelField("WorldID: " + world.worldId);
                EditorGUILayout.LabelField("WorldSize: " + world.GetWorldSize());
                EditorGUILayout.LabelField("EntitiesCount: " + world.GetEntitiesCount());
                EditorGUILayout.LabelField("AllocatedEntitiesCount: " + world.GetAllocatedEntitiesCount());
                EditorGUILayout.LabelField("RawEntitiesCount: " + world.GetRawEntities().Length);
                EditorGUILayout.LabelField("FreeMaskCount: " + world.GetFreeMaskCount());

                EditorGUILayout.Space();

                var poolCount = world.GetAllPools(ref m_Pools);
                m_PoolFoldout = EditorGUILayout.Foldout(m_PoolFoldout, "PoolsCount: " + poolCount, true);
                if (m_PoolFoldout)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < poolCount; i++)
                    {
                        var pool = m_Pools[i];
                        EditorGUILayout.LabelField(pool.ToString());
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                var filterCount = world.GetAllFilters(ref m_Filters);
                Array.Sort(m_Filters, (x, y) => x.GetMask().hash - y.GetMask().hash);
                m_FilterFoldout = EditorGUILayout.Foldout(m_FilterFoldout, "FilterCount: " + filterCount, true);
                if (m_FilterFoldout)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < filterCount; i++)
                    {
                        var filter = m_Filters[i];
                        EditorGUILayout.LabelField(filter.ToString());
                    }
                    EditorGUI.indentLevel--;
                }

                // TODO Filter 被哪些 System 使用了，似乎反射不太行
            }
        }
    }
}