using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float rotationAmount = 90f;
    public float moveDistance = 1f;
    public float rotationSmoothness = 5f;
    public float moveSmoothness = 5f;
    public float raySphereRadius = 0.2f;
    public float jumpForce = 10f;

    [Header("References")]
    public Transform cam;
    public Transform groundCheck;
    public Transform defaultPoint;
    public LayerMask groundLayer;
    public GameObject movePoints;
    public bool isOnDefault = true;
    public bool hasKey = false;

    [Header("Buttons")]
    public GameObject rotateButton;
    public GameObject teleportButton;

    private Transform hitObject;
    private Transform interactObject;
    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isJumping = false;
    private bool isGrounded, canAct = true;
    private Rigidbody rb;

    void Start()
    {
        hitObject = null;
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;
        targetRotation = transform.rotation;

    }

    void Update()
    {
        HandleMovement();
        HandleTeleportToLocation();
    }

    void HandleMovement()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer);    

        if (isJumping && isGrounded && !wasGrounded)
        {
            Vector3 rawPos = transform.position;

            Vector3 roundedPos = new Vector3(
                RoundTwoDecimals(rawPos.x),
                rawPos.y,
                RoundTwoDecimals(rawPos.z)
            );
            
            targetPosition = roundedPos;
            
            isJumping = false;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
        if (isGrounded && !isJumping)
        {
            Vector3 targetPos = transform.position;
            targetPos.x = Mathf.Lerp(targetPos.x, targetPosition.x, moveSmoothness * Time.deltaTime);
            targetPos.z = Mathf.Lerp(targetPos.z, targetPosition.z, moveSmoothness * Time.deltaTime);
            transform.position = targetPos;
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                Vector3 rawPos = transform.position;

                Vector3 roundedPos = new Vector3(
                    RoundTwoDecimals(rawPos.x),
                    rawPos.y,
                    RoundTwoDecimals(rawPos.z)
                );
                
                transform.position = roundedPos;
            }
        }
    }

    public void TurnLeft()
    {
        if (!canAct || isJumping) return;
        canAct = false;

        targetRotation = targetRotation * Quaternion.Euler(0, -rotationAmount, 0);
        defaultPoint.localRotation = targetRotation;
        Invoke(nameof(ResetAction), 0.1f);
    }

    public void TurnRight()
    {
        if (!canAct || isJumping) return;
        canAct = false;

        targetRotation = targetRotation * Quaternion.Euler(0, rotationAmount, 0);
        defaultPoint.localRotation = targetRotation;
        Invoke(nameof(ResetAction), 0.1f);
    }

    void HandleTeleportToLocation()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        
        if (Physics.SphereCast(ray, raySphereRadius, out RaycastHit hit, Mathf.Infinity))
        {
            if(hit.collider.CompareTag("MovePoint"))
                hitObject = hit.transform;
            if(hit.collider.CompareTag("DoorPoint") || 
                hit.collider.CompareTag("Door") ||
                hit.collider.CompareTag("Searchable") ||
                hit.collider.CompareTag("Key"))
                interactObject = hit.transform;
                
        }
        else 
        {
            hitObject = null;   
        }
    
        if(isOnDefault)
        {   
            movePoints.SetActive(true);
            if(hitObject)
                teleportButton.SetActive(true);
            else teleportButton.SetActive(false);
        }
        else
        {
            movePoints.SetActive(false);
            teleportButton.SetActive(true);
            hitObject = null;
        } 
    }

    public void MoveForward()
    {
        // if (!canAct || isJumping) return;
        // canAct = false;

        // targetPosition = targetPosition + transform.forward * moveDistance;
        // Invoke(nameof(ResetAction), 0.1f);

        if(hitObject && isOnDefault)
        {
            if (rotateButton.activeSelf)
                rotateButton.SetActive(false);

            transform.position = hitObject.parent.position;
            transform.rotation = hitObject.parent.rotation;
            cam.localEulerAngles = hitObject.parent.GetComponent<MovePoint>().cameraRotation;
            isOnDefault = false;
        }
        else if(!hitObject && !isOnDefault)
        {
            if (!rotateButton.activeSelf)
                rotateButton.SetActive(true);
            
            Debug.Log(defaultPoint.position);
            Vector3 targetPosition = new Vector3(defaultPoint.position.x, transform.position.y, defaultPoint.position.z);
            transform.position = targetPosition;
            transform.rotation = defaultPoint.rotation;
            cam.localEulerAngles = Vector3.zero;
            isOnDefault = true;
        }
    }

    public void Interact()
    {
        if(interactObject.tag == "Door" && hasKey)
        {
            Debug.Log("Opening Door");
            DoorScript doorScript = interactObject.parent.GetComponent<DoorScript>();
            doorScript.UnlockDoor();

        }
        else if(interactObject.tag == "DoorPoint")
        {
            Debug.Log("DoorPoint works");
            defaultPoint = interactObject.transform.Find("Default Point");
            defaultPoint.rotation = transform.rotation;
            Debug.Log(defaultPoint.name);
            Vector3 targetPosition = new Vector3(defaultPoint.position.x, transform.position.y, defaultPoint.position.z);
            transform.position = targetPosition;
            transform.rotation = defaultPoint.rotation;
        }
        else if(interactObject.tag == "Searchable")
        {
            Debug.Log("Search works but nothing found");
        }
        else if(interactObject.tag == "Key")
        {
            Debug.Log("Search works, found key!");
            hasKey = true;
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("JumpPad"))
        {
            if (!canAct) return;
            canAct = false;
            Debug.Log("Touched");
            isJumping = true;
            rb.AddForce(other.transform.up * jumpForce, ForceMode.Impulse);
            targetRotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

            Invoke(nameof(ResetAction), 0.5f);
        }
    }

    void ResetAction()
    {
        canAct = true;
    }

    float RoundTwoDecimals(float v)
    {
        return Mathf.Round(v * 100f) / 100f;
    }

}