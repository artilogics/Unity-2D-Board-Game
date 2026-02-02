using UnityEngine;

public class CharSelectButton : MonoBehaviour
{
    public int index;
    public GameSetupManager manager;

    public void OnClick()
    {
        Debug.Log($"CharSelectButton clicked: Index {index}");
        if (manager != null)
        {
            manager.OnCharacterSelected(index);
        }
        else
        {
            Debug.LogError("CharSelectButton: Manager is null!");
        }
    }
}
