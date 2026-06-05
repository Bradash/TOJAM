using TMPro;
using UnityEngine;

public class Subtitles : MonoBehaviour
{
    TextMeshProUGUI text;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void SetSubtitle(string subtitle)
    {
        text.text = subtitle;
    }
}
