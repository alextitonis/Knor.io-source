using UnityEngine;
using UnityEngine.UI;

public class _Message : MonoBehaviour
{
    [SerializeField] Text text;

    [HideInInspector] public string Msg;
    [HideInInspector] public Color Color;

    public void SetUp(string Msg, Color Color)
    {
        this.Msg = Msg;
        this.Color = Color;

        text.text = Msg;
        text.color = Color;
    }
}