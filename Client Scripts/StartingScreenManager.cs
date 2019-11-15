using UnityEngine;
using UnityEngine.UI;

public class StartingScreenManager : MonoBehaviour
{
    [SerializeField] InputField nameInput;

    public void SetName()
    {
        string _name = nameInput.text;
        nameInput.text = "";

        if (string.IsNullOrEmpty(_name))
            return;

        NetworkManager.getInstance.SetName(_name);
    }
}