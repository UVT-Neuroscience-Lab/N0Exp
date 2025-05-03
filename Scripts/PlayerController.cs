using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public Transform groundCheck;
    public Transform defaultPoint;
    public LayerMask groundLayer;
    public bool isOnDefault = true;

    [Header("Buttons")]
    public GameObject rotateButton;
    public GameObject teleportButton;

    private Transform hitObject;
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
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (Physics.SphereCast(ray, raySphereRadius, out RaycastHit hitInfo, Mathf.Infinity))
            {
                hitObject = hit.transform;
            }
            else 
            {
                hitObject = null;   
            }
        }

        if(isOnDefault)
        {
            if(hitObject)
                teleportButton.SetActive(true);
            else teleportButton.SetActive(false);
        }
        else teleportButton.SetActive(true);
    }

    public void MoveForward()
    {
        // if (!canAct || isJumping) return;
        // canAct = false;

        // targetPosition = targetPosition + transform.forward * moveDistance;
        // Invoke(nameof(ResetAction), 0.1f);

        if(hitObject)
        {
            if (rotateButton.activeSelf)
                rotateButton.SetActive(false);

            transform.position = hitObject.parent.position;
            transform.rotation = hitObject.parent.rotation;
            Camera.main.transform.localEulerAngles = hitObject.parent.GetComponent<MovePoint>().cameraRotation;
            isOnDefault = false;
        }
        else if(!hitObject && !isOnDefault)
        {
            if (!rotateButton.activeSelf)
                rotateButton.SetActive(true);
            
            transform.position = defaultPoint.transform.position;
            transform.rotation = defaultPoint.transform.rotation;
            Camera.main.transform.localEulerAngles = Vector3.zero;
            isOnDefault = true;
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