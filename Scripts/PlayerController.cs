using UnityEngine;
using UnityEngine.UI;
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

    [Header("References")]
    public Transform cam;
    public Transform groundCheck;
    public Transform defaultPoint;
    public LayerMask groundLayer;
    public GameObject movePoints;
    public bool isOnDefault = true;
    public bool hasKey = false;

    [Header("Fade Settings")]
    public Image blackScreen;
    public float fadeDuration = 0.5f;

    [Header("Buttons")]
    public GameObject rotateButton;
    public GameObject teleportButton;
    public GameObject interactButton;

    private Transform hitObject;
    private Transform interactObject;
    private bool canAct = true;
    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isJumping = false;
    private bool isGrounded;
    private Rigidbody rb;

    // Input rate limiting
    private float lastInputTime;
    public float inputCooldown = 1f;

    void Start()
    {
        hitObject = null;
        interactObject = null;
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
            Vector3 tp = transform.position;
            tp.x = Mathf.Lerp(tp.x, targetPosition.x, moveSmoothness * Time.deltaTime);
            tp.z = Mathf.Lerp(tp.z, targetPosition.z, moveSmoothness * Time.deltaTime);
            transform.position = tp;

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
        if (Time.time < lastInputTime + inputCooldown) return;
        lastInputTime = Time.time;
        if (!canAct || isJumping) return;
        canAct = false;
        targetRotation *= Quaternion.Euler(0, -rotationAmount, 0);
        defaultPoint.localRotation = targetRotation;
        Invoke(nameof(ResetAction), 0.1f);
    }

    public void TurnRight()
    {
        if (Time.time < lastInputTime + inputCooldown) return;
        lastInputTime = Time.time;
        if (!canAct || isJumping) return;
        canAct = false;
        targetRotation *= Quaternion.Euler(0, rotationAmount, 0);
        defaultPoint.localRotation = targetRotation;
        Invoke(nameof(ResetAction), 0.1f);
    }

    public void MoveForward()
    {
        if (Time.time < lastInputTime + inputCooldown) return;
        lastInputTime = Time.time;
        StartCoroutine(FadeAndTeleportMove());
    }

    public void Interact()
    {
        if (Time.time < lastInputTime + inputCooldown) return;
        lastInputTime = Time.time;
        if (interactObject == null) return;

        if (interactObject.CompareTag("Door") && hasKey)
        {
            DoorController door = interactObject.parent.GetComponent<DoorController>();
            door.UnlockDoor();
        }
        else if (interactObject.CompareTag("DoorPoint"))
        {
            StartCoroutine(FadeAndTeleportDefault());
        }
        else if (interactObject.CompareTag("Searchable"))
        {
            var gfx = interactObject.parent.Find("GFX");
            if (gfx)
                gfx.GetComponent<Movable>().move = true;
            Invoke(nameof(ResetMovable), 1f);
        }
        else if (interactObject.CompareTag("Key"))
        {
            if (!hasKey)
            {
                var gfx = interactObject.parent.Find("GFX");
                if (gfx) gfx.GetComponent<Movable>().move = true;
                Invoke(nameof(TakeKey), 0.5f);
            }
        }
    }

    void HandleTeleportToLocation()
    {
        interactButton.SetActive(interactObject != null);

        if (isOnDefault)
            movePoints.SetActive(true);
        else
            movePoints.SetActive(false);

        teleportButton.SetActive(isOnDefault ? hitObject != null : true);
        if (!isOnDefault) hitObject = null;

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        Debug.DrawRay(ray.origin, ray.direction * 1000f, hitObject != null ? Color.green : Color.red, 0.1f);

        if (Physics.SphereCast(ray, raySphereRadius, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                hitObject = interactObject = null;
                return;
            }
            if (hit.collider.CompareTag("MovePoint"))
                hitObject = hit.transform;
            else if (hit.collider.CompareTag("DoorPoint") || hit.collider.CompareTag("Door") ||
                     hit.collider.CompareTag("Searchable") || hit.collider.CompareTag("Key"))
                interactObject = hit.transform;
            else
                hitObject = interactObject = null;
        }
        else
        {
            hitObject = interactObject = null;
        }
    }

    private IEnumerator FadeAndTeleportMove()
    {
        blackScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0f, 1f));

        if (hitObject && isOnDefault)
        {
            if (rotateButton.activeSelf) rotateButton.SetActive(false);
            transform.position = hitObject.parent.position;
            transform.rotation = hitObject.parent.rotation;
            cam.localEulerAngles = hitObject.parent.GetComponent<MovePoint>().cameraRotation;
            isOnDefault = false;
        }
        else if (!hitObject && !isOnDefault)
        {
            if (!rotateButton.activeSelf) rotateButton.SetActive(true);
            Vector3 tp = defaultPoint.position;
            transform.position = new Vector3(tp.x, transform.position.y, tp.z);
            transform.rotation = defaultPoint.rotation;
            cam.localEulerAngles = Vector3.zero;
            isOnDefault = true;
        }

        yield return StartCoroutine(Fade(1f, 0f));
        blackScreen.gameObject.SetActive(false);
    }

    private IEnumerator FadeAndTeleportDefault()
    {
        blackScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0f, 1f));

        defaultPoint = interactObject.transform.Find("Default Point");
        defaultPoint.rotation = transform.rotation;
        Vector3 dp = defaultPoint.position;
        transform.position = new Vector3(dp.x, transform.position.y, dp.z);
        transform.rotation = defaultPoint.rotation;
        cam.localEulerAngles = Vector3.zero;
        isOnDefault = true;

        yield return StartCoroutine(Fade(1f, 0f));
        blackScreen.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        Color col = blackScreen.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            blackScreen.color = new Color(col.r, col.g, col.b, Mathf.Lerp(start, end, elapsed / fadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(col.r, col.g, col.b, end);
    }

    void TakeKey()
    {
        var key = interactObject.parent.Find("Key");
        if (key) Destroy(key.gameObject);
        Invoke(nameof(ResetMovable), 0.5f);
        hasKey = true;
    }

    void ResetMovable()
    {
        var gfx = interactObject.parent.Find("GFX");
        if (gfx) gfx.GetComponent<Movable>().move = false;
    }

    void ResetAction() { canAct = true; }

    float RoundTwoDecimals(float v) => Mathf.Round(v * 100f) / 100f;
}