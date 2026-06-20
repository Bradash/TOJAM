using TMPro;
using UnityEngine;

public class Subtitles : MonoBehaviour
{
    TextMeshProUGUI text;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = null;
    }
    public void SetSubtitle(string subtitle)
    {
        if(GameSettings.subtitle) text.text = subtitle;
    }
    private void Update()
    {
        if(!GameSettings.subtitle && text.text != null) text.text = null;
    }
}
