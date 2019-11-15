using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] List<GameObject> objectsToUnlock = new List<GameObject>();

    public bool enter = false;

    void LateUpdate()
    {
        if (NetworkManager.getInstance.isBotClient)
        {
            UnLock();
            return;
        }

        if (!NetworkManager.getInstance.LocalPlayerExists)
        {
            UnLock();
            return;
        }

        if (enter)
        {
            UnLock();
            return;
        }

        foreach (var i in objectsToUnlock)
        {
            if (i.activeSelf)
            {
                UnLock();
                return;
            }
        }

        Lock();
    }

    void Lock()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void UnLock()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}