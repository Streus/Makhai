using Makhai.Core;
using UnityEditor;
using UnityEngine;

namespace Makhai.Editor
{
	[CustomPropertyDrawer(typeof(Entity))]
	public class EntityPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty (position, label, property);
			EditorGUI.indentLevel++;

			EditorGUI.PropertyField (position, property.FindPropertyRelative("HealthMax"));

			EditorGUI.indentLevel--;
			EditorGUI.EndProperty ();
		}
	}
}
