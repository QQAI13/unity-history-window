using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    public class NewAssets : ScriptableObject
    {
        [Serializable]
        public class Assets
        {
            public Object reference;
        }

        public event Action<NewAssets> OnFavoritesUpdated;

        [SerializeField]
        public List<Assets> favoritesList = new List<Assets>();

        public static NewAssets CreateAndSave(string folderPath, string assetName)
        {
            var newAsset = CreateInstance<NewAssets>();

            // Ensure the folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = "Assets";
                foreach (var folder in folders.Skip(1)) // Skip "Assets" as it always exists
                {
                    if (!AssetDatabase.IsValidFolder($"{currentPath}/{folder}"))
                    {
                        AssetDatabase.CreateFolder(currentPath, folder);
                    }
                    currentPath += $"/{folder}";
                }
            }

            // Create and save the asset
            string assetPath = $"{folderPath}/{assetName}.asset";
            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newAsset;
        }

        public void AddFavorite(Assets favorite)
        {
            favoritesList.Add(favorite);
            OnFavoritesUpdated?.Invoke(this);
            EditorUtility.SetDirty(this); // Mark the asset as dirty for saving
        }

        public bool IsFavorite(Object reference)
        {
            return favoritesList.Any(f => f.reference == reference);
        }

        public void RemoveFavorite(Object reference)
        {
            favoritesList.RemoveAll(f => f.reference == reference);
            OnFavoritesUpdated?.Invoke(this);
            EditorUtility.SetDirty(this); // Mark the asset as dirty for saving
        }
    }
}
