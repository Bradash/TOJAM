using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] string nextSceneName;
    [SerializeField] Animator animation;
    private void Update()
    {
        AnimatorStateInfo animStateInfo = animation.GetCurrentAnimatorStateInfo(0);
        float NTime = animStateInfo.normalizedTime;
        if (NTime >= 0.99f)
        {
            endCutScene();
        }
    }
    public void endCutScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
