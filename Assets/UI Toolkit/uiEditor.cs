using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/*
public class uiEditor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/uiEditor")]
    public static void ShowExample()
    {
        uiEditor wnd = GetWindow<uiEditor>();
        wnd.titleContent = new GUIContent("uiEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/uiEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
    }
}*/