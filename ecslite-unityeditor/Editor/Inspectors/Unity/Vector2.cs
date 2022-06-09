// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Saro.Entities.UnityEditor.Inspectors
{
    internal sealed class Vector2Inspector : EcsComponentInspectorTyped<Vector2>
    {
        protected override bool OnGuiTyped(string label, ref Vector2 value, EcsEntityDebugView entityView)
        {
            var newValue = EditorGUILayout.Vector2Field(label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}