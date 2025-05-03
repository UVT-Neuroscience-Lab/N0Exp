using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float rotationAmount = 90f;
    public float moveDistance = 1f;
    public float rotationSmoothness = 5f;
    public float moveSmoothness = 5f;

    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isJumping = false;
    private bool isGrounded, canAct = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
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

        if(Input.GetKeyDown(KeyCode.G) && isGrounded)
        {
            if (!canAct) return;
            canAct = false;

            isJumping = true;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            targetRotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

            Invoke(nameof(ResetAction), 0.5f);
        }
    }

    public void TurnLeft()
    {
        if (!canAct || isJumping) return;
        canAct = false;

        targetRotation = targetRotation * Quaternion.Euler(0, -rotationAmount, 0);
        Invoke(nameof(ResetAction), 0.1f);
    }

    public void TurnRight()
    {
        if (!canAct || isJumping) return;
        canAct = false;

        targetRotation = targetRotation * Quaternion.Euler(0, rotationAmount, 0);
        Invoke(nameof(ResetAction), 0.1f);
    }

    public void MoveForward()
    {
        // if (!canAct || isJumping) return;
        // canAct = false;

        // targetPosition = targetPosition + transform.forward * moveDistance;
        // Invoke(nameof(ResetAction), 0.1f);

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.Log("Casting Ray");
            if (hit.collider.CompareTag("MovePoint"))
            {
                Debug.Log("Hit a MovePoint at " + hit.point);
                transform.position = hit.transform.parent.position;
                transform.rotation = hit.transform.parent.rotation;
                Camera.main.transform.localEulerAngles = hit.transform.parent.GetComponent<MovePoint>().cameraRotation;
            }   
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