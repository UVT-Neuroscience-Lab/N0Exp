using System.Security.Cryptography.X509Certificates;
using EZCameraShake;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isFinalDoor;
    public bool isLocked;
    public float openingSpeed = 5f;
    public Vector3 moveOffset;
    private PlayerController controller;
    Vector3 doorOpenPosition;

    void Start()
    {
        doorOpenPosition = transform.position + moveOffset;
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
            if(CameraShaker.Instance)
                CameraShaker.Instance.ShakeOnce(1f, 3f, 0.1f, 5f);
        }
    }
}