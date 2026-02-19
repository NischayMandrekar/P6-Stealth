using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] patrollPath pPath;
    [SerializeField] float wayPointTolerance = 1f;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float playerStopDis = 2f;
    [SerializeField] float genStopDis = .5f;
    [SerializeField] float rotationSpeed=10f;

    [Header("Search")]
    [SerializeField] float dwellTime = 2f;
    [SerializeField] float detectionTimer = 2f;
    [SerializeField] float slowRate=1;
    [SerializeField] float fastRate=2;
    [SerializeField] float hearingDis=4;
    float detecting;
    [SerializeField] float maxSeenTime = 4;
    [SerializeField] float searchRotateSpeed = 120f;

    public bool gameOver;

    [Header("colors")]
    [SerializeField] MeshRenderer bodyMesh;
    [SerializeField] Color searchColor;
    [SerializeField] Color detectingColor;



    float seenTimer=0;
    Material bodyMaterial;
    Color origColor;
    NavMeshAgent navMeshAgent;
    FieldOFView fieldOFView;
    PlayerMovement playerMovement;

    Vector3 guardPosition;
    Vector3 lastSeenPosition;

    float playerLastSeenTime = Mathf.Infinity;
    bool reachedLastSeen = true;
    bool wasSeeingPlayer = false;
    bool investigatingNoise = false;
    float noiseDist;



    int currWaypointIndex = 0;

    void Awake()
    {
        fieldOFView = GetComponent<FieldOFView>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        playerMovement = fieldOFView.playerRef.GetComponent<PlayerMovement>();
        bodyMaterial = bodyMesh.material;
        origColor = bodyMaterial.color;
        guardPosition = transform.position;
        navMeshAgent.speed = moveSpeed;
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
        else if (playerMovement.noiseTrigger && !fieldOFView.canSeePlayer)
        {
            noiseDist = (playerMovement.noisePosition - transform.position).sqrMagnitude;
            if (noiseDist < hearingDis * hearingDis)
            {
                investigatingNoise = true;
                lastSeenPosition = playerMovement.noisePosition;
                reachedLastSeen = false;
                playerLastSeenTime = 0;
            }
            else
            {
                investigatingNoise = false;
                NormalBehaviour();
            }
        }
        else
        {
            NormalBehaviour();
        }
        wasSeeingPlayer = fieldOFView.canSeePlayer;
    }

    void NormalBehaviour()
    {
        if (wasSeeingPlayer)
        {
            detecting = 0;
        }

        if (investigatingNoise)
        {
            print("inside normalbehaviour investigating when noise is heard");
            HandleSearchAndPatrol(lastSeenPosition);

            if (reachedLastSeen && playerLastSeenTime > dwellTime)
            {
                investigatingNoise = false;
            }
        }
        else
        {
            HandleSearchAndPatrol(lastSeenPosition);
        }
    }




    void HandleSearchAndPatrol(Vector3 position)
    {
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = genStopDis;
        if (!reachedLastSeen)
        {
            bodyMaterial.color = detectingColor;
            Move(position);
            if (navMeshAgent.remainingDistance==0)
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
            transform.Rotate(0, searchRotateSpeed* Time.deltaTime, 0);
        }
        else
        {
            bodyMaterial.color = origColor;
            PatrolBehaviour();
        }
    }


    void PatrolBehaviour()
    {
        navMeshAgent.updateRotation = true;
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
        
        navMeshAgent.SetDestination(targetPosition);
        if (!navMeshAgent.updateRotation)
        {
        Vector3 direction = navMeshAgent.steeringTarget - transform.position;
        direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
           
        }
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
    void HandleChase()
    {
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = playerStopDis;
        Vector3 playerPos = fieldOFView.playerRef.transform.position;
        lastSeenPosition = playerPos;
        reachedLastSeen = false;
        playerLastSeenTime = 0;
        Move(playerPos);
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            seenTimer += Time.deltaTime;
            if (seenTimer > maxSeenTime&&playerMovement.enabled)
            {
                print("Gameover");
                gameOver = true;
            }
        }
        else
        {
            seenTimer = 0;
        }
    }
}
