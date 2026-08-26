// Assets/Editor/FindMaterialSubmeshMismatches.cs

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace gamesolids
{
	/// <summary>
	/// Provides a utility for finding renderers whose material count exceeds their mesh submesh count.
	/// </summary>
	/// <remarks>
	/// This scan checks all currently loaded scenes and reports renderers that have
	/// more assigned materials than available mesh submeshes.
	/// I don't know why this is a thing. But, more than one asset I've downloaded has had this issue.
	/// </remarks>
	public static class FindMaterialSubmeshMismatches
	{
		/// <summary>
		/// Scans all currently loaded scenes for renderers with material-to-submesh mismatches.
		/// </summary>
		[MenuItem("Tools/GameSolids/Find Material-Submesh Mismatches In Open Scenes")]
		public static void FindInOpenScenes()
		{
			int found = 0;

			for (int s = 0; s < SceneManager.sceneCount; s++)
			{
				var scene = SceneManager.GetSceneAt(s);
				if (!scene.isLoaded) continue;

				foreach (var root in scene.GetRootGameObjects())
				{
					foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
					{
						CheckRenderer(renderer, ref found);
					}
				}
			}

			Debug.Log($"Material/Submesh mismatch scan complete. Found {found} issue(s).");
		}

		/// <summary>
		/// Checks a single renderer for a material count greater than its mesh submesh count.
		/// </summary>
		/// <param name="renderer">The renderer to inspect.</param>
		/// <param name="found">The running mismatch count.</param>
		private static void CheckRenderer(Renderer renderer, ref int found)
		{
			Mesh mesh = null;

			if (renderer is MeshRenderer)
			{
				var mf = renderer.GetComponent<MeshFilter>();
				if (mf != null)
					mesh = mf.sharedMesh;
			}
			else if (renderer is SkinnedMeshRenderer smr)
			{
				mesh = smr.sharedMesh;
			}

			if (mesh == null)
				return;

			int materialCount = renderer.sharedMaterials != null
				? renderer.sharedMaterials.Length
				: 0;

			int subMeshCount = mesh.subMeshCount;

			if (materialCount > subMeshCount)
			{
				found++;

				Debug.LogWarning(
					$"Material count > submesh count:\n" +
					$"Object: {GetHierarchyPath(renderer.transform)}\n" +
					$"Renderer: {renderer.GetType().Name}\n" +
					$"Mesh: {mesh.name}\n" +
					$"Materials: {materialCount}\n" +
					$"Submeshes: {subMeshCount}\n" +
					$"Scene: {renderer.gameObject.scene.name}",
					renderer.gameObject
				);

				if (found == 1)
				{
					Selection.activeObject = renderer.gameObject;
					EditorGUIUtility.PingObject(renderer.gameObject);
				}
			}
		}

		/// <summary>
		/// Builds a slash-delimited hierarchy path for a transform.
		/// </summary>
		/// <param name="t">The transform whose hierarchy path should be created.</param>
		/// <returns>The full hierarchy path from root to the target transform.</returns>
		private static string GetHierarchyPath(Transform t)
		{
			string path = t.name;

			while (t.parent != null)
			{
				t = t.parent;
				path = t.name + "/" + path;
			}

			return path;
		}
	}
}