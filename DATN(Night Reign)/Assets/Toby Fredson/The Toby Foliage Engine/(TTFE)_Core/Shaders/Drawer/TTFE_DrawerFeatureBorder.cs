#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace TobyFredson.Drawers
{
    public class TTFE_DrawerFeatureBorder : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position.width -= 24;
            position.height = EditorGUIUtility.singleLineHeight + 12;

            Color rectColor = new Color(0.229f, 0.504f, 0.229f, 1f);
            EditorGUI.DrawRect(position, rectColor);

            Rect textRect = new Rect(position.x + 12, position.y + 3, position.width - 24, EditorGUIUtility.singleLineHeight + 6);
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(textRect, label, textStyle);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return EditorGUIUtility.singleLineHeight + 12;
        }
    }
}
#endif
