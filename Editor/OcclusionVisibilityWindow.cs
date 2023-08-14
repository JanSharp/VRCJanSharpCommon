using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace JanSharp
{
    public class OcclusionVisibilityWindow : EditorWindow
    {
        [MenuItem("Tools/Occlusion Visibility")]
        public static void ShowVisibilityWindow()
        {
            // This method is called when the user selects the menu item in the Editor
            EditorWindow wnd = GetWindow<OcclusionVisibilityWindow>();
            wnd.titleContent = new GUIContent("Occlusion Visibility");
        }

        // private static void UpdateVisibility()
        // {
        //     SceneVisibilityManager.instance.HideAll();
        //     SceneVisibilityManager.instance.Show(Selection.gameObjects, true);
        // }

        private static void SetFlag(GameObject[] gos, StaticEditorFlags flag)
        {
            foreach (GameObject go in gos.Where(go => (GameObjectUtility.GetStaticEditorFlags(go) & flag) == 0))
            {
                SerializedObject goProxy = new SerializedObject(go);
                goProxy.FindProperty("m_StaticEditorFlags").intValue = (int)(GameObjectUtility.GetStaticEditorFlags(go) | flag);
                goProxy.ApplyModifiedProperties();
            }
        }

        private static void UnsetFlag(GameObject[] gos, StaticEditorFlags flag)
        {
            foreach (GameObject go in gos.Where(go => (GameObjectUtility.GetStaticEditorFlags(go) & flag) != 0))
            {
                SerializedObject goProxy = new SerializedObject(go);
                goProxy.FindProperty("m_StaticEditorFlags").intValue = (int)(GameObjectUtility.GetStaticEditorFlags(go) & (~flag));
                goProxy.ApplyModifiedProperties();
            }
        }

        public void CreateGUI()
        {
            // rootVisualElement.Add(new Label("Hello"));

            // var toggle = new Toggle("Only Show Selected");
            // toggle.RegisterValueChangedCallback(e => {
            //     if (e.newValue)
            //     {
            //         Selection.selectionChanged += UpdateVisibility;
            //         UpdateVisibility();
            //     }
            //     else
            //     {
            //         Selection.selectionChanged -= UpdateVisibility;
            //         SceneVisibilityManager.instance.ShowAll();
            //     }
            // });
            // rootVisualElement.Add(toggle);

            ///cSpell:ignore occluder, occludee, occluders, occludees

            var occluderToggle = new Toggle("Show Occluders");
            var nonOccluderToggle = new Toggle("Show non-Occluders");
            var occludeeToggle = new Toggle("Show Occludees");
            var nonOccludeeToggle = new Toggle("Show non-Occludees");
            var batchingToggle = new Toggle("Show Batchers");
            var nonBatchingToggle = new Toggle("Show non-Batchers");
            rootVisualElement.Add(occluderToggle);
            rootVisualElement.Add(nonOccluderToggle);
            rootVisualElement.Add(occludeeToggle);
            rootVisualElement.Add(nonOccludeeToggle);
            rootVisualElement.Add(batchingToggle);
            rootVisualElement.Add(nonBatchingToggle);

            rootVisualElement.Add(new Button(() => SetFlag(Selection.gameObjects, StaticEditorFlags.OccluderStatic)) { text = "Make Occluder" });
            rootVisualElement.Add(new Button(() => UnsetFlag(Selection.gameObjects, StaticEditorFlags.OccluderStatic)) { text = "Make non-Occluder" });
            rootVisualElement.Add(new Button(() => SetFlag(Selection.gameObjects, StaticEditorFlags.OccludeeStatic)) { text = "Make Occludee" });
            rootVisualElement.Add(new Button(() => UnsetFlag(Selection.gameObjects, StaticEditorFlags.OccludeeStatic)) { text = "Make non-Occludee" });
            rootVisualElement.Add(new Button(() => SetFlag(Selection.gameObjects, StaticEditorFlags.BatchingStatic)) { text = "Make Batching" });
            rootVisualElement.Add(new Button(() => UnsetFlag(Selection.gameObjects, StaticEditorFlags.BatchingStatic)) { text = "Make non-Batching" });

            var updateButton = new Button() { text = "Update Visibility" };
            updateButton.clicked += () =>
            {
                SceneVisibilityManager.instance.HideAll();
                void WalkGameObject(GameObject go)
                {
                    foreach (Transform transform in go.transform)
                        WalkGameObject(transform.gameObject);
                    var flags = GameObjectUtility.GetStaticEditorFlags(go);

                    if ((flags.HasFlag(StaticEditorFlags.OccluderStatic)
                            ? occluderToggle.value
                            : nonOccluderToggle.value)
                        || (flags.HasFlag(StaticEditorFlags.OccludeeStatic)
                            ? occludeeToggle.value
                            : nonOccludeeToggle.value)
                        || (flags.HasFlag(StaticEditorFlags.BatchingStatic)
                            ? batchingToggle.value
                            : nonBatchingToggle.value))
                    {
                        SceneVisibilityManager.instance.Show(go, false);
                    }
                }
                foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    WalkGameObject(go);
                }
            };
            rootVisualElement.Add(updateButton);
        }
    }
}
.
