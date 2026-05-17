using UnityEngine;

public class CarSwitching : MonoBehaviour
{
   [SerializeField] GameObject[] cars;
   [SerializeField] GameObject[] cars2;
   Animator carAnim;
    private void Start()
    {
        carAnim = GetComponent<Animator>();
        switchCar();
    }
    private void Update()
    {
        AnimatorStateInfo animStateInfo = carAnim.GetCurrentAnimatorStateInfo(0);
        float NTime = animStateInfo.normalizedTime;
        if (NTime >= 0.99f)
        {
            switchCar();
        }
    }
    void switchCar()
    {
        int car1 = Random.Range(0, cars.Length);
        int car2 = Random.Range(0, cars2.Length);
        for (int i = 0; i < cars.Length; i++)
        {
            if (i == car1)
            {
                cars[i].SetActive(true);
            }
            else
            {
                cars[i].SetActive(false);
            }
        }
        for (int i = 0; i < cars2.Length; i++)
        {
            if (i == car2)
            {
                cars2[i].SetActive(true);
            }
            else
            {
                cars2[i].SetActive(false);
            }
        }
    }

}
