
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class NameInputDialog : EditorWindow
    {
        private string windowName = "";
        private System.Action<string> onNameEntered;

        [MenuItem("Window/Gemserk/New Assets Window")]
        public static void ShowDialog()
        {
            var dialog = GetWindow<NameInputDialog>("Name Your Window");
            dialog.minSize = new Vector2(300, 100);
            dialog.maxSize = new Vector2(300, 100);
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter a name for your new window:", EditorStyles.label);

            windowName = EditorGUILayout.TextField("Window Name:", windowName);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            if (GUILayout.Button("Create"))
            {
                if (ManageExistedWindows.ExistedTabs.Contains(windowName))
                {
                    Debug.LogWarning($"Tab '{windowName}' already exists.");
                    return;
                }
                
                var window = EditorWindow.CreateInstance<NewAssetsWindow>();

                window.Initianlize(windowName, "Assets/Gemserk/" + windowName + ".asset");
                
                Close();
            }

            GUILayout.EndHorizontal();
        }
    }
    public class NewAssetsWindow : EditorWindow
    {
        public NewAssets _newAssets;

        public string assetName = "NewAssets";
        public string assetFilePath = "Assets/Gemserk/"; 

        private StyleSheet styleSheet;
        private VisualTreeAsset AssetsElementTreeAsset;
        private ToolbarSearchField searchToolbar;
        private VisualElement newAssetsParent;
        private string searchText;

        public void Initianlize(string windowName, string filePath, bool open = false)
        {
            titleContent = new GUIContent(name, EditorGUIUtility.IconContent(UnityBuiltInIcons.pickObjectIconName).image);

            assetName = windowName;
            assetFilePath = filePath;


            if(open){

                _newAssets = AssetDatabase.LoadAssetAtPath<NewAssets>(assetFilePath + assetName + ".asset");

                foreach (var o in _newAssets.favoritesList)
                {
                    AssetsElements(new Object[] { o.reference });
                }

                ReloadRoot();

            }else{

                ManageExistedWindows.ExistedTabs.Add(windowName);

                 _newAssets = NewAssets.CreateAndSave(assetFilePath, windowName);
            }

            titleContent.text = windowName;
            titleContent.tooltip = windowName + "assets window";
                
            Show();
            ReloadRoot();
        }

        public static void NewAsset()
        {
            var selectedObjects = Selection.objects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                foreach (var obj in selectedObjects)
                {
                    if (CanBeNewAsset(obj))
                    {
                        Debug.Log($"Added new asset: {obj.name}");
                    }else{
                        Debug.Log($"Can't add new asset: {obj.name}");
                    }
                }
            }
        }



        private void GetDefaultElements()
        {
            if (styleSheet == null)
            {
                styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                    .OfType<StyleSheet>().FirstOrDefault();
            }
            
            if (AssetsElementTreeAsset == null)
            {
                var treeAsset = AssetsElementTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoriteElement")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }else{
                Debug.Log("AssetsElementTreeAsset already loaded");
            }
        }

        private static bool CanBeNewAsset(Object reference)
        {
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(reference));
        }

        private void AssetsElements(Object[] references)
        {
            
            _newAssets = AssetDatabase.LoadAssetAtPath<NewAssets>(assetFilePath + assetName + ".asset");

            if (_newAssets == null)
            {
                _newAssets = NewAssets.CreateAndSave(assetFilePath, assetName);

            }else{
                Debug.Log("NewAssetsWindow loaded existing asset");
            }

            foreach (var reference in references)
            {
                if (_newAssets.IsFavorite(reference))
                    continue;

                if (CanBeNewAsset(reference))
                {

                    _newAssets.AddFavorite(new NewAssets.Assets
                    {
                        reference = reference
                    });
                    
                    ReloadRoot();

                }else{
                    Debug.Log("Can't add new asset: " + reference.name);
                }
            }
        }

        public void Reload()
        {
            GetDefaultElements();
            ReloadRoot();
        }


        private void OnEnable()
        {
            GetDefaultElements();

            _newAssets = AssetDatabase.LoadAssetAtPath<NewAssets>(assetFilePath + assetName + ".asset");
            

            if (_newAssets == null)
            {
                _newAssets = NewAssets.CreateAndSave(assetFilePath, assetName);
                Debug.Log("NewAssetsWindow created new asset");

            }else{
                Debug.Log("NewAssetsWindow loaded existing asset");
            }

            _newAssets.OnFavoritesUpdated += OnFavoritesUpdated;

            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);

            root.Add(CreateSearchToolbar());

            root.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();
                AssetsElements(DragAndDrop.objectReferences);
            });
            
            root.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            });
            

            ReloadRoot();
        }

        private void OnFavoritesUpdated(NewAssets newAssets)
        {
            ReloadRoot();
        }


        private void OnDisable()
        {
            if (_newAssets != null)
            {
                _newAssets.OnFavoritesUpdated -= OnFavoritesUpdated;
            }
            
            styleSheet = null;
            AssetsElementTreeAsset = null;
        }


        private VisualElement CreateSearchToolbar()
        {
            searchToolbar = new ToolbarSearchField();
            searchToolbar.AddToClassList("searchToolbar");
            searchToolbar.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                ReloadRoot();
            });

            return searchToolbar;
        }

        private void ReloadRoot()
        {
            var root = rootVisualElement;


            if (newAssetsParent == null)
            {
                newAssetsParent = new ScrollView(ScrollViewMode.Vertical);
                root.Add(newAssetsParent);
            }
            else
            {
                newAssetsParent.Clear();
            }

            // Parse search text into multiple search terms
            string[] searchTexts = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchTexts = searchText.Split(' ');
                }
            }

            // Iterate through all assets
            for (var i = 0; i < _newAssets.favoritesList.Count; i++)
            {
                var assetReference = _newAssets.favoritesList[i].reference;

                if (assetReference == null)
                    continue;

                var testName = assetReference.name.ToLower();

                // Check if the asset matches all search terms
                if (searchTexts != null && searchTexts.Length > 0)
                {
                    var match = true;

                    foreach (var text in searchTexts)
                    {
                        if (!testName.Contains(text.ToLower()))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }

                // Clone the UI template and populate its data
                var elementTree = AssetsElementTreeAsset.CloneTree();
                var newAssetRoot = elementTree.Q<VisualElement>("Root");

                var dragArea = elementTree.Q<VisualElement>("DragArea");
                
                if (dragArea != null)
                {
                    dragArea.AddManipulator(new NewAssetsElementDragManipulator(assetReference));
                }

                var icon = elementTree.Q<Image>("Icon");
                if (icon != null)
                {
                    icon.image = AssetPreview.GetMiniThumbnail(assetReference);
                }

                var removeIcon = elementTree.Q<Image>("RemoveIcon");
                if (removeIcon != null)
                {
                    removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.removeIconName).image;
                    removeIcon.tooltip = "Remove";

                    removeIcon.RegisterCallback<MouseUpEvent>(e =>
                    {
                        _newAssets.RemoveFavorite(assetReference);
                        ReloadRoot();
                    });
                }

                var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
                if (openPrefabIcon != null)
                {
                    openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openAssetIconName).image;
                    openPrefabIcon.tooltip = "Open";

                    openPrefabIcon.RemoveFromClassList("hidden");

                    openPrefabIcon.RegisterCallback<MouseUpEvent>(e =>
                    {
                        AssetDatabase.OpenAsset(assetReference);
                    });
                }

                var label = elementTree.Q<Label>("Favorite");
                if (label != null)
                {
                    label.text = assetReference.name;
                }else{
                    label.text = testName;
                }

                newAssetsParent.Add(newAssetRoot);
            }

            EditorUtility.SetDirty(_newAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Add a flexible space at the bottom to improve UI spacing
            var receiveDragArea = new VisualElement();
            receiveDragArea.style.flexGrow = 1;
            root.Add(receiveDragArea);
        }
    }

}