#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace TobyFredson.Drawers
{
    public class TTFE_DrawerTitle : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position.width -= 24;
            position.height = EditorGUIUtility.singleLineHeight + 4;
            position.x += 12;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 5, 10)
            };

            EditorGUI.LabelField(position, label.ToUpper(), titleStyle);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }
}
#endif
