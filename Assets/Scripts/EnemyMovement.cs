using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] patrollPath pPath;
    [SerializeField] float wayPointTolerance = 1f;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float stopDis = 2f;

    [Header("Search")]
    [SerializeField] float dwellTime = 2f;
    [SerializeField] float detectionTimer = 2f;
    [SerializeField] float slowRate=1;
    [SerializeField] float fastRate=2;
    float detecting;
    // [SerializeField] float searchRotateSpeed = 120f;

    [Header("colors")]
    [SerializeField] MeshRenderer bodyMesh;
    [SerializeField] Color searchColor;
    [SerializeField] Color detectingColor;

    Material bodyMaterial;
    Color origColor;
    CharacterController characterController;
    FieldOFView fieldOFView;
    PlayerMovement playerMovement;

    Vector3 guardPosition;
    Vector3 lastSeenPosition;

    float playerLastSeenTime = Mathf.Infinity;
    bool reachedLastSeen = true;
    bool wasSeeingPlayer=false;


    int currWaypointIndex = 0;

    void Awake()
    {
        fieldOFView = GetComponent<FieldOFView>();
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        playerMovement = fieldOFView.playerRef.GetComponent<PlayerMovement>();
        bodyMaterial = bodyMesh.material;
        origColor = bodyMaterial.color;
        guardPosition = transform.position;
    }

    void Update()
    {
        if (fieldOFView.canSeePlayer)
        {
            detecting += playerMovement.isCrouch ? Time.deltaTime * slowRate : Time.deltaTime * fastRate;
            float t = detecting / detectionTimer;
            if (detecting < detectionTimer)
            {
                bodyMaterial.color = Color.Lerp(origColor, detectingColor, t);
            }
            else
            {
                bodyMaterial.color = searchColor;
                HandleChase();
            }
        }
        else
        {
            if (wasSeeingPlayer)
            {
                detecting = 0;
            }
            HandleSearchAndPatrol();
        }
        wasSeeingPlayer = fieldOFView.canSeePlayer;
    }

    void HandleChase()
    {
        Vector3 playerPos = fieldOFView.playerRef.transform.position;
        lastSeenPosition = playerPos;
        reachedLastSeen = false;
        playerLastSeenTime = 0;

        float sqrDist = (playerPos - transform.position).sqrMagnitude;

        if (sqrDist > stopDis * stopDis)
        {
            Move(playerPos);
        }
    }


    void HandleSearchAndPatrol()
    {
        if (!reachedLastSeen)
        {
            bodyMaterial.color = detectingColor;
            Move(lastSeenPosition);

            if (Vector3.Distance(transform.position, lastSeenPosition) < wayPointTolerance)
            {
                reachedLastSeen = true;
            }

            return;
        }

        playerLastSeenTime += Time.deltaTime;
        float t = playerLastSeenTime / dwellTime;

        if (playerLastSeenTime < dwellTime)
        {
            bodyMaterial.color = Color.Lerp(detectingColor,origColor,t);
            // transform.Rotate(0, searchRotateSpeed * Time.deltaTime, 0);
        }
        else
        {
            bodyMaterial.color = origColor;
            PatrolBehaviour();
        }
    }


    void PatrolBehaviour()
    {
        Vector3 nextPosition = guardPosition;

        if (pPath != null)
        {
            if (AtWaypoint())
            {
                CycleWaypoint();
            }

            nextPosition = GetCurrentWaypoint();
        }

        Move(nextPosition);
    }


    void Move(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f) return;

        direction.Normalize();

        characterController.Move(direction * moveSpeed * Time.deltaTime);

        Vector3 lookPos = targetPosition;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }


    Vector3 GetCurrentWaypoint()
    {
        return pPath.GetPosition(currWaypointIndex);
    }

    void CycleWaypoint()
    {
        currWaypointIndex = pPath.GetNextIndex(currWaypointIndex);
    }

    bool AtWaypoint()
    {
        return Vector3.Distance(transform.position, GetCurrentWaypoint()) < wayPointTolerance;
    }
}
