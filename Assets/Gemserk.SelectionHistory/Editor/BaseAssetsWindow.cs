using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public abstract class BaseAssetsWindow : EditorWindow
    {
        protected string windowTitle;
        protected VisualElement contentParent;
        protected ScrollView contentScrollView;
        protected TextField searchField;

        protected abstract string GetWindowTitle(); // Must be implemented by derived classes.
        protected abstract void ReloadContent(string searchText);

        public void OnEnable()
        {
            // Set the title
            windowTitle = GetWindowTitle();
            titleContent = new GUIContent(windowTitle);

            var root = rootVisualElement;

            // Search Field
            searchField = new TextField("Search");
            searchField.RegisterValueChangedCallback(evt => ReloadContent(evt.newValue));
            root.Add(searchField);

            // Content ScrollView
            contentScrollView = new ScrollView();
            root.Add(contentScrollView);

            // Initialize Content
            ReloadContent("");
        }

        protected VisualElement CreateAssetElement(Object asset)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;

            // Asset Icon
            var icon = new Image
            {
                image = AssetPreview.GetMiniThumbnail(asset),
                style = { width = 20, height = 20, marginRight = 10 }
            };
            container.Add(icon);

            // Asset Name
            var label = new Label(asset.name);
            label.style.flexGrow = 1;
            container.Add(label);

            // Remove Button
            var removeButton = new Button(() => OnRemoveAsset(asset)) { text = "X" };
            container.Add(removeButton);

            return container;
        }

        protected virtual void OnRemoveAsset(Object asset)
        {
            Debug.Log($"Removed {asset.name} from {windowTitle}");
        }
    }
}
