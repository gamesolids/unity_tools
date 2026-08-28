using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace gamesolids
{
	/// <summary>
	/// Displays a simple navigation history for the current Unity Inspector selection.
	/// </summary>
	/// <remarks>
	/// This window tracks recently selected Unity objects and allows moving backward
	/// and forward through selection history, similar to browser navigation.
	/// ...I really needed a 'back' button for when I accidentally clicked a material
	/// or prefab I was trying to drag... this is what happened.
	/// </remarks>
	public class InspectorHistory : EditorWindow
	{
		private const int MaxHistory = 100; //adjust if needed

		private static readonly List<Object> _back = new List<Object>();
		private static readonly List<Object> _fwd = new List<Object>();
		private static Object _current;

		/// <summary>
		/// Opens the Inspector History editor window.
		/// </summary>
		[MenuItem("Tools/GameSolids/Inspector History &h")] // Alt + H
		public static void ShowWindow() => GetWindow<InspectorHistory>("Inspector History");

		/// <summary>
		/// Subscribes to Unity selection change events when the window is enabled.
		/// </summary>
		private void OnEnable() => Selection.selectionChanged += OnSelectionChanged;

		/// <summary>
		/// Unsubscribes from Unity selection change events when the window is disabled.
		/// </summary>
		private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

		/// <summary>
		/// Updates history stacks when the active Unity selection changes.
		/// </summary>
		private void OnSelectionChanged()
		{
			var selected = Selection.activeObject;
			if (!selected || selected == _current) return;

			PruneDeadEntries();

			if (_current && (_back.Count == 0 || _back[_back.Count - 1] != _current))
				_back.Add(_current);

			TrimIfNeeded(_back);
			_current = selected;
			_fwd.Clear();
			Repaint();
		}

		/// <summary>
		/// Draws the editor window interface.
		/// </summary>
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			using (new EditorGUI.DisabledScope(_back.Count == 0))
				if (GUILayout.Button("← Back")) NavigateBack();

			using (new EditorGUI.DisabledScope(_fwd.Count == 0))
				if (GUILayout.Button("Forward →")) NavigateForward();

			if (_current)
				EditorGUILayout.LabelField("Current:", _current.name);

			GUILayout.EndHorizontal();

			DrawStack("Back History", _back);
			DrawStack("Forward History", _fwd);
		}

		/// <summary>
		/// Draws a selectable history stack in reverse order.
		/// </summary>
		/// <param name="label">The section label to display.</param>
		/// <param name="stack">The history stack to render.</param>
		private void DrawStack(string label, List<Object> stack)
		{
			if (stack.Count == 0) return;
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			for (int i = stack.Count - 1; i >= 0; --i)
			{
				var o = stack[i];
				if (!o) continue;
				if (GUILayout.Button(o.name, EditorStyles.miniButton))
				{
					// Jump directly to an arbitrary point in history.
					JumpTo(stack, i);
					break;
				}
			}
		}

		/// <summary>
		/// Navigates to the previous object in the back history.
		/// </summary>
		private void NavigateBack()
		{
			if (_back.Count == 0) return;
			if (_current) _fwd.Add(_current);
			_current = _back[_back.Count - 1];
			_back.RemoveAt(_back.Count - 1);
			Selection.activeObject = _current;
		}

		/// <summary>
		/// Navigates to the next object in the forward history.
		/// </summary>
		private void NavigateForward()
		{
			if (_fwd.Count == 0) return;
			if (_current) _back.Add(_current);
			_current = _fwd[_fwd.Count - 1];
			_fwd.RemoveAt(_fwd.Count - 1);
			Selection.activeObject = _current;
		}

		/// <summary>
		/// Jumps directly to a selected history item and rebalances the history stacks.
		/// </summary>
		/// <param name="sourceStack">The stack containing the selected target item.</param>
		/// <param name="index">The index of the target item in the source stack.</param>
		private void JumpTo(List<Object> sourceStack, int index)
		{
			// Move current into opposite stack, then reposition stacks based on jump target.
			List<Object> targetStack = ReferenceEquals(sourceStack, _back) ? _fwd : _back;
			if (_current) targetStack.Add(_current);

			// Everything after index becomes the new opposite stack extension.
			for (int i = sourceStack.Count - 1; i > index; --i)
				targetStack.Add(sourceStack[i]);
			_current = sourceStack[index];
			sourceStack.RemoveRange(index, sourceStack.Count - index);
			Selection.activeObject = _current;
		}

		/// <summary>
		/// Removes destroyed or invalid Unity object references from history.
		/// </summary>
		private static void PruneDeadEntries()
		{
			_back.RemoveAll(o => !o);
			_fwd.RemoveAll(o => !o);
		}

		/// <summary>
		/// Trims a history list so it does not exceed the configured maximum size.
		/// </summary>
		/// <param name="list">The history list to trim.</param>
		private static void TrimIfNeeded(List<Object> list)
		{
			if (list.Count <= MaxHistory) return;
			int excess = list.Count - MaxHistory;
			if (excess > 0)
				list.RemoveRange(0, excess);
		}
	}
}