using UnityEngine;

public class Lever : MonoBehaviour
{
    PlayerController player;
    Quaternion startRotation;
    Quaternion targetRotation;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        startRotation = transform.localRotation;
        targetRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if (player.pulledLever)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, 5f * Time.deltaTime);
        }
    }
}
