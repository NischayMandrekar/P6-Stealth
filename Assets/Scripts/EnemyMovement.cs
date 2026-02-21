using System;
using Unity.VisualScripting;
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
    bool HasStartMoving = true;
    float noiseDist;
    int currWaypointIndex = 0;

    enum EnemyState { Patrol, Search, Detect, Chase ,Investigate}
    EnemyState curState;
    EnemyState prevState;

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
        switch (curState)
        {
            case EnemyState.Patrol:
                PatrolBehaviour();
                break;
            case EnemyState.Detect:
                DetectBehvaiour();
                break;
            case EnemyState.Chase:
                ChaseBehaviour();
                break;
            case EnemyState.Search:
                SearchBehaviour();
                break;
            case EnemyState.Investigate:
                InvestigationBehaviour();
                break;
        }
    }

    void PatrolBehaviour()
    {
        print("patrol State");
        if (fieldOFView.canSeePlayer)
        {

            curState = EnemyState.Detect;
            return;
        }
        if (playerMovement.noiseTrigger && !fieldOFView.canSeePlayer)
        {
            noiseDist = (playerMovement.noisePosition - transform.position).sqrMagnitude;
            if (noiseDist > hearingDis * hearingDis)
            {
                print("out of Range");
                detecting = 0;
                curState = EnemyState.Patrol;
                return;
            }
            lastSeenPosition = playerMovement.noisePosition;
            HasStartMoving = false;
            playerLastSeenTime = 0;
            curState = EnemyState.Investigate;
            return;
    
        }
        bodyMaterial.color = origColor;
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

    void DetectBehvaiour()
    {
        print("Detect State");
        navMeshAgent.ResetPath();
        if (!fieldOFView.canSeePlayer)
        {
            detecting = 0;
            curState = EnemyState.Search;
        }
        detecting += playerMovement.isCrouch ? Time.deltaTime * slowRate : Time.deltaTime * fastRate;
        navMeshAgent.updateRotation = false;
        Vector3 dir = fieldOFView.playerRef.transform.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (detecting > detectionTimer)
        {
            curState = EnemyState.Chase;
        }
        float t = detecting / detectionTimer;
        bodyMaterial.color = Color.Lerp(origColor, detectingColor, t);
    }
    
    void ChaseBehaviour()
    {
        print("chase State");
        bodyMaterial.color = searchColor;
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = playerStopDis;
        Vector3 playerPos = fieldOFView.playerRef.transform.position;
        Move(playerPos);
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            seenTimer += Time.deltaTime;
            if (seenTimer > maxSeenTime && playerMovement.enabled)
            {
                print("Gameover");
                gameOver = true;
            }
        }
        else
        {
            seenTimer = 0;
        }
        if (!fieldOFView.canSeePlayer)
        {
            lastSeenPosition = fieldOFView.playerRef.transform.position; 
            HasStartMoving = false;
            playerLastSeenTime = 0;
            detecting = 0;
            curState = EnemyState.Investigate;
            return;
        }
    }

    void SearchBehaviour()
    {
        if (fieldOFView.canSeePlayer)
        {
            curState = EnemyState.Detect;
            return;
        }
        print("search State");
        navMeshAgent.updateRotation = true;
        navMeshAgent.stoppingDistance = genStopDis;
        playerLastSeenTime += Time.deltaTime;
        float t = playerLastSeenTime / dwellTime;
        bodyMaterial.color = Color.Lerp(detectingColor, origColor, t);
        // transform.Rotate(0, searchRotateSpeed * Time.deltaTime, 0);
        if (playerLastSeenTime >= dwellTime)
        {
            detecting = 0;
            curState = EnemyState.Patrol;
        }
        if (playerMovement.noiseTrigger&&!fieldOFView.canSeePlayer)
        {
            noiseDist = (playerMovement.noisePosition - transform.position).sqrMagnitude;
            if (noiseDist > hearingDis * hearingDis)
            {
                print("out of Range");
                detecting = 0;
                curState = EnemyState.Patrol;
                return;
            }
            lastSeenPosition = playerMovement.noisePosition;
            HasStartMoving = false;          
            playerLastSeenTime = 0;
            curState = EnemyState.Investigate;
            return;
        }
    }
    
    void InvestigationBehaviour()
    {
        print("investigation state");
        if(navMeshAgent.velocity.sqrMagnitude > 0.1f){
            HasStartMoving = true;
        }
        if (HasStartMoving&&navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            playerLastSeenTime = 0;
            curState = EnemyState.Search;
            return;
        }
        navMeshAgent.stoppingDistance = genStopDis;
        bodyMaterial.color = detectingColor;
        Move(lastSeenPosition);
        if (fieldOFView.canSeePlayer)
        {
            curState = EnemyState.Chase;
        }
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
    
}