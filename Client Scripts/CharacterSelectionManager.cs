using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public enum PlayerFaction
    {
        Warrior = 0,
        Orc = 1,
    }

    public void PlayAsWarrior()
    {
        NetworkManager.getInstance.Play((int)PlayerFaction.Warrior);
    }
    public void PlayAsOrc()
    {
        NetworkManager.getInstance.Play((int)PlayerFaction.Orc);
    }
}