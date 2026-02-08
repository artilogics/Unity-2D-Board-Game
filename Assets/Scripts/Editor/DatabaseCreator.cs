using UnityEngine;
using UnityEditor;

public class DatabaseCreator
{
    [MenuItem("Tools/Board Game/Generate Database Asset")]
    public static void CreateAsset()
    {
        CharacterDatabase asset = ScriptableObject.CreateInstance<CharacterDatabase>();

        string path = "Assets/Resources/CharacterDatabase.asset";
        
        // Ensure Resources folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("Created CharacterDatabase at " + path);
    }
}
