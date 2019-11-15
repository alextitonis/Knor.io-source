using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager getInstance;
    void Awake() { getInstance = this; }

    public string ID = "";

    [SerializeField] Slider healthSlider, staminaSlider;
    [SerializeField] Text scoreText;
    [SerializeField] GameObject respawnPanel;
    [SerializeField] Button respawnAccept, respawnDecline;

    void Update()
    {
        if (NetworkManager.getInstance.isBotClient)
            return;

        if (ID != NetworkManager.getInstance.localID)
            return;

        if (Input.GetKeyDown(KeyCode.Y))
            AnswerRespawnRequest(true);
        else if (Input.GetKeyDown(KeyCode.U))
            AnswerRespawnRequest(false);

        if (NetworkManager.getInstance.localPlayer != null)
        {
            staminaSlider.maxValue = NetworkManager.getInstance.localPlayer.GetComponent<vThirdPersonController>().maxStamina;
            staminaSlider.value = NetworkManager.getInstance.localPlayer.GetComponent<vThirdPersonController>().currentStamina;
        }
    }

    void Start()
    {
        respawnAccept.onClick.AddListener(delegate
        {
            NetworkManager.getInstance.RespawnRequestAnswer(true);
            UpdateRespawnPanel();
        });
        respawnDecline.onClick.AddListener(delegate
        {
            NetworkManager.getInstance.RespawnRequestAnswer(false);
            UpdateRespawnPanel();
        });
    }

    public void AnswerRespawnRequest(bool accept)
    {
        if (respawnPanel.activeSelf)
        {
            NetworkManager.getInstance.RespawnRequestAnswer(accept);
            UpdateRespawnPanel();
        }
    }

    public void SetHealth(Player p)
    {
        if (!p.isLocal)
            return;

        healthSlider.maxValue = p.maxHealth;
        healthSlider.value = p.currentHealth;
    }
    public void SetScore(int k1, int k2)
    {
        scoreText.text = "Knights: " + k1 + "\n " +
             "Orcs: " + k2;
    }
    public void UpdateRespawnPanel()
    {
        respawnPanel.SetActive(!respawnPanel.activeSelf);
    }
}
