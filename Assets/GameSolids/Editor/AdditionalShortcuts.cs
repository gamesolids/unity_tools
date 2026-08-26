using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace gamesolids
{
	/// <summary>
	/// Registers additional editor shortcuts for custom actions in the Unity Editor.
	/// </summary>
	/// <remarks>
	/// These shortcuts operate on the current selection in the Scene View and provide
	/// quick reset actions for local rotation and local scale. 
	/// Also a convenient place to add more keyboard shortcuts in the future.
	/// </remarks>
	public class AdditionalShortcuts
	{
		// The [Shortcut] attribute registers directly with Unity's Shortcuts Manager.
		// Specifying typeof(SceneView) ensures it only triggers when you are working in the Scene View window.

		/// <summary>
		/// Resets the rotation of all selected game objects.
		/// </summary>
		[Shortcut("Clear Rotation", typeof(SceneView), KeyCode.R, ShortcutModifiers.Alt)] // Alt + R
		private static void ResetRotation()
		{
			Debug.Log("Reset Rotation triggered via Alt+R");

			if (Selection.gameObjects.Length == 0) return;

			Undo.RecordObjects(Selection.transforms, "Reset Rotation");
			foreach (GameObject obj in Selection.gameObjects)
			{
				obj.transform.localRotation = Quaternion.identity;
			}
		}

		/// <summary>
		/// Resets the local scale of all selected game objects.
		/// </summary>
		[Shortcut("Clear Scale", typeof(SceneView), KeyCode.S, ShortcutModifiers.Alt)] // Alt + S
		private static void ResetScale()
		{
			Debug.Log("Reset Scale triggered via Alt+S");

			if (Selection.gameObjects.Length == 0) return;

			Undo.RecordObjects(Selection.transforms, "Reset Scale");
			foreach (GameObject obj in Selection.gameObjects)
			{
				obj.transform.localScale = Vector3.one;
			}
		}

		/// <summary>
		/// Add more here.
		/// </summary>

	}
}