using UnityEditor;
using UnityEngine;

public class FloorGenerator : EditorWindow {
    GameObject prefab;
    Vector2 prefabSize;
    int generatedSize;

    [MenuItem("Sennik/" + nameof(FloorGenerator))]
    public static void ShowSelf()
        => GetWindow<FloorGenerator>(nameof(FloorGenerator)).Show();

    void OnGUI() {
        prefab = EditorGUILayout.ObjectField(label: nameof(prefab), prefab, typeof(GameObject), allowSceneObjects: false) as GameObject;
        prefabSize = EditorGUILayout.Vector2Field(label: nameof(prefabSize), prefabSize);
        generatedSize = EditorGUILayout.IntField(label: nameof(generatedSize), generatedSize);
        if (GUILayout.Button("Generate"))
            Generate();
    }

    void Generate() {
        var parent = new GameObject("Generated floor").transform;
        var offset = new Vector2(
            -prefabSize.x / 2 * generatedSize,
            -prefabSize.y / 2 * generatedSize
        );
        for (var x = 0; x < generatedSize; x++)
            for (var y = 0; y < generatedSize; y++) {
                var position = new Vector3(
                    offset.x + x * prefabSize.x,
                    0,
                    offset.y + y * prefabSize.y
                );
                Instantiate(prefab, position, rotation: Quaternion.identity, parent);
            }
    }
}
