using UnityEngine;

public class Movable : MonoBehaviour
{
    public Vector3 targetPosOffset;
    public bool move;

    private Vector3 defaultPos;
    private Vector3 targetPos;

    void Start()
    {
        defaultPos = transform.position;
        targetPos = defaultPos + targetPosOffset;
    }

    void Update()
    {
        Vector3 destination = move ? targetPos : defaultPos;
        transform.position = Vector3.Lerp(transform.position, destination, 5f * Time.deltaTime);
        if (Vector3.Distance(transform.position, destination) < 0.001f)
            transform.position = destination;
    }
}
