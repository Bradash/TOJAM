using UnityEngine;
using System.Collections;

public class PromptDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text promptText;
    private GameObject prompt;
    [SerializeField] private float fadeDuration = 0.5f; 
    [SerializeField] private float displayTime = 1.0f;
    [SerializeField] private float alpha;

    private void Start()
    {
        alpha = promptText.alpha > 0 ? promptText.alpha : 1f;
        prompt = promptText.gameObject;
        prompt.SetActive(false);
        
    }

    public void ShowPrompt()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // 1. Make it fully visible immediately
        promptText.alpha = alpha;
        prompt.SetActive(true);
        
        // 2. Wait for the 'solid' duration
        yield return new WaitForSeconds(displayTime);

        // 3. Gradually fade out
        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Linearly interpolate alpha from 1 to 0
            promptText.alpha = Mathf.Lerp(alpha, 0f, currentTime / fadeDuration);
            yield return null; 
        }

        // Ensure it's completely invisible
        prompt.SetActive(false);
        promptText.alpha = 0f;
    }
}