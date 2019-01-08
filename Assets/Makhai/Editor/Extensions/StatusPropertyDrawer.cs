using Makhai.Core.Data;
using UnityEditor;
using UnityEngine;

namespace Makhai.Editor
{
	[CustomPropertyDrawer(typeof(Status))]
	public class StatusPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty (position, label, property);
			EditorGUI.indentLevel++;

			EditorGUI.LabelField (position, property.ToString ());

			EditorGUI.indentLevel--;
			EditorGUI.EndProperty ();
		}
	}
}
