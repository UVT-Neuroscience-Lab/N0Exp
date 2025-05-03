using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public bool isLocked;
    public float openingSpeed = 5f;
    private PlayerController controller;
    Vector3 doorOpenPosition;

    void Start()
    {
        doorOpenPosition = transform.position + new Vector3(0f ,-3.2f, 0f);
        controller = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocked)
        {
            transform.position = Vector3.Lerp(transform.position, doorOpenPosition, openingSpeed*Time.deltaTime);
            
        }
    }

    public void UnlockDoor()
    {
        if(isLocked)
        {
            isLocked = false;
        }
    }
}
