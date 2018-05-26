using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AutocompleteSearchField
{
	public class GitHubSearchExample : EditorWindow
	{
		[MenuItem("Window/Autocomplete Searchbar/GitHub Search Example")]
		static void Init()
		{
			GetWindow<GitHubSearchExample>("GitHub Example").Show();
		}

		[SerializeField]
		AutocompleteSearchField autocompleteSearchField;

		WWW activeWWW;

		void OnEnable()
		{
			if (autocompleteSearchField == null) autocompleteSearchField = new AutocompleteSearchField();
			autocompleteSearchField.onInputChanged = OnInputChanged;
			autocompleteSearchField.onConfirm = OnConfirm;
			EditorApplication.update += OnUpdate;
		}

		void OnDisable()
		{
			EditorApplication.update -= OnUpdate;
		}

		void OnGUI ()
		{
			GUILayout.Label("Search GitHub", EditorStyles.boldLabel);
			autocompleteSearchField.OnGUI();
		}

		void OnInputChanged(string searchString)
		{
			if(string.IsNullOrEmpty(searchString)) return;

			autocompleteSearchField.ClearResults();
			var query = string.Format("https://api.github.com/search/repositories?q={0}&sort=stars&order=desc", searchString);
			activeWWW = new WWW(query);
		}

		void OnConfirm(string result)
		{
			var obj = AssetDatabase.LoadMainAssetAtPath(autocompleteSearchField.searchString);
			Selection.activeObject = obj;
			EditorGUIUtility.PingObject(obj);
		}

		void OnUpdate()
		{
			if(activeWWW != null && activeWWW.isDone)
			{
				if(string.IsNullOrEmpty(activeWWW.error))
				{
					const string url = "html_url";

					autocompleteSearchField.ClearResults();
					var text = activeWWW.text;

					// Hacky json "parsing"
					foreach (var line in text.Split('\n'))
					{
						var nameIndex = line.IndexOf(url, StringComparison.InvariantCulture);
						if(nameIndex > 0)
						{
							var result = line.Substring(nameIndex + url.Length + 1).Split('"')[1];
							autocompleteSearchField.AddResult(result);
						}
					}

					Debug.Log(text.Split('\n').Length);
				}
				else
				{
					Debug.LogError("Error: " + activeWWW.error);
				}
				activeWWW = null;
			}
		}
	}
}