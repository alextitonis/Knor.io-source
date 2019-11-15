using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager getInstance;
    void Awake() { getInstance = this; }

    [System.Serializable]
    public class ScreenObject
    {
        public screen sceen;
        public GameObject[] objects;
    }
    public enum screen
    {
        Starting = 0,
        CharacterSelection,
        InGame,
        Disconnected,
    }

    [SerializeField] screen currentScreen;
    [SerializeField] screen startingScreen;
    [SerializeField] List<ScreenObject> Screens = new List<ScreenObject>();

    void Start()
    {
        Change(startingScreen);
    }

    public void Change(screen _screen)
    {
        if (currentScreen == _screen)
            return;

        ScreenObject obj = Screens.Find(x => x.sceen == _screen);

        if (obj != null)
        {
            foreach (var i in Screens)
                foreach (var j in i.objects)
                    j.SetActive(false);

            currentScreen = _screen;

            foreach (var i in obj.objects)
                i.SetActive(true);
        }
    }
}