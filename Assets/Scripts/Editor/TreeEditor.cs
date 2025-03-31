using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TreeConstructor))]
    public class TreeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                var treeConstructor = (TreeConstructor)target;
                treeConstructor.Start();
            }
        }
    }
}
