using Invector.vCharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager getInstance;
    void Awake() { getInstance = this; }
    
    [Header("Chat")]
    [SerializeField] InputField messageInput;
    [SerializeField] int maxMsgs = 6;
    [SerializeField] GameObject textPrefab;
    [SerializeField] Transform spawnLocation;
    [SerializeField] int maxMessageLength = 60;

    List<_Message> textMessages = new List<_Message>();

    void Start()
    {
        messageInput.characterLimit = maxMessageLength;
    }

    public void HandleMessage(ChatPacket packet)
    {
        Color color = Color.white;
        string msg = " [" + packet.sender + "]: " + packet.msg;

        if (textMessages.Count >= maxMsgs)
        {
            Destroy(textMessages[0].gameObject);
            textMessages.Remove(textMessages[0]);
        }

        GameObject obj = Instantiate(textPrefab, spawnLocation);
        _Message _msg = obj.GetComponent<_Message>();
        _msg.SetUp(msg, color);

        textMessages.Add(_msg);
    }

    public void SendChatMessage(bool fromEnter = false)
    {
        if (string.IsNullOrEmpty(messageInput.text))
            return;
        
        string msg = messageInput.text;
        messageInput.text = "";

        NetworkManager.getInstance.SendChatMessage(msg);

        if (!fromEnter)
            CursorManager.getInstance.enter = !CursorManager.getInstance.enter;
    }

    public void Enter()
    {
        if (!string.IsNullOrEmpty(messageInput.text))
        {
            SendChatMessage(true);

            CursorManager.getInstance.enter = !CursorManager.getInstance.enter;
        }
        else
        {
            if (!CursorManager.getInstance.enter)
            {
                messageInput.ActivateInputField();
            }
            
            CursorManager.getInstance.enter = !CursorManager.getInstance.enter;
        }
    }
}