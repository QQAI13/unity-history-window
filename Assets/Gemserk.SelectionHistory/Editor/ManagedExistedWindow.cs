using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    public class ManageExistedWindows : EditorWindow
    {
        // List to maintain folder names
        public static List<string> ExistedTabs = new List<string>();

        private static string targetFolder = "Assets/Gemserk/";

        [MenuItem("Window/Gemserk/Manage Existed Windows")]
        public static void OpenManageWindow()
        {
            var window = GetWindow<ManageExistedWindows>();
            window.titleContent = new GUIContent("Manage Existed Windows");
            window.minSize = new Vector2(400, 300);
            window.Show();

            RefreshExistedTabs();
        }

        private void OnGUI()
        {
            GUILayout.Label("Manage Existed Windows", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Refresh Tabs", GUILayout.Width(200)))
            {
                RefreshExistedTabs();
            }

            GUILayout.Space(10);

            // Display the list of tabs with options
            List<string> tabsToRemove = new List<string>();

            foreach (var tab in ExistedTabs) // Use a copy of the list to avoid enumeration issues
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(tab, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Open", GUILayout.Width(75)))
                {
                    OpenWindow(tab);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(75)))
                {
                    if (EditorUtility.DisplayDialog("Delete Window", $"Are you sure you want to delete '{tab}'?", "Yes", "No"))
                    {
                        tabsToRemove.Add(tab); // Add tab to the removal list
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            // Remove tabs after the iteration
            foreach (var tabToRemove in tabsToRemove)
            {
                DeleteTab(tabToRemove);
            }


            GUILayout.FlexibleSpace();

            // Remove tabs after iteration
            foreach (var tabToRemove in tabsToRemove)
            {
                DeleteTab(tabToRemove);
            }
        }


        // Updates the ExistedTabs list based on the assets in the folder
        private static void RefreshExistedTabs()
        {
            ExistedTabs.Clear();

            string[] guids = AssetDatabase.FindAssets("", new[] { targetFolder });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (!ExistedTabs.Contains(assetName))
                {
                    ExistedTabs.Add(assetName);
                    Debug.Log($"Added tab '{assetName}' from asset: {assetPath}");
                }
            }

            Debug.Log($"Updated ExistedTabs with {ExistedTabs.Count} assets from '{targetFolder}'");
        }

        private void OpenWindow(string tabName)
        {
            // If the window is not open, recreate it from the asset
            string assetFilePath = $"Assets/Gemserk/{tabName}.asset";
            Debug.Log(assetFilePath);
            var asset = AssetDatabase.LoadAssetAtPath<NewAssets>(assetFilePath);

            if (asset != null)
            {
                var newWindow = EditorWindow.CreateInstance<NewAssetsWindow>();

                newWindow.Initianlize(tabName, targetFolder, true);

                newWindow.Reload();
                newWindow.Show();
                newWindow.Repaint();
            }
            else
            {
                Debug.LogWarning($"Asset not found for tab: {tabName}. Expected at path: {assetFilePath}");
            }
        }


        // Deletes the tab and the corresponding asset file
        private static void DeleteTab(string tabName)
        {
            ExistedTabs.Remove(tabName);
            // Construct the path to the corresponding asset file
            string assetFilePath = $"{targetFolder}{tabName}.asset";
            if (AssetDatabase.DeleteAsset(assetFilePath))
            {
                Debug.Log($"Deleted tab '{tabName}' and corresponding asset file: {assetFilePath}");
            }
            else
            {
                Debug.LogWarning($"Failed to delete asset file for tab '{tabName}'. File may not exist: {assetFilePath}");
            }
            // Refresh the asset database and the tabs list
            AssetDatabase.Refresh();
        }
    }
}
