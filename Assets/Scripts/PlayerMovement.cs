using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //  private readonly int speed= Animator.StringToHash("Speed");
    // private readonly int isAiming= Animator.StringToHash("isAiming");
    [SerializeField] float moveSpeed;
    [SerializeField] float crouchSpeed;
    [SerializeField] float Rotation_damping;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundedForce = -2f;
    [SerializeField] float noiseDelay = 3f;
    float noiseTimer = 0;
    Vector3 targetRotation;
    float verticalVelocity;
    // bool isDead;
    float curSpeed;
    
    Vector2 move,mouseLook;
    Vector3 movement;
    CharacterController characterController;
    // float health;
    // Animator animator;
    public  bool isCrouch;
    public Vector3 noisePosition;
    public bool noiseTrigger = false;
    bool canClap = true;

    void Awake()
    {
        canClap = true;
        characterController = GetComponent<CharacterController>();
        // animator = GetComponent<Animator>();
        // health = GetComponent<Health>().curHealth;
    }
                       
    void Update()
    {
        curSpeed = isCrouch ? crouchSpeed : moveSpeed;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mouseLook);
        if (Physics.Raycast(ray, out hit))
        {
            targetRotation = hit.point;
        }
        MovePlayer();
        if (!canClap)
        {
            noiseTimer += Time.deltaTime;

            if (noiseTimer > noiseDelay)
            {
                canClap = true;
                noiseTimer = 0;
            }
        }

    }
    
    public void DeathHandler()
    {
        Destroy(gameObject);
    }
    void MovePlayer()
    {
        Vector3 horMovement = new Vector3(move.x, 0f, move.y);
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundedForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        movement = horMovement * curSpeed + Vector3.up * verticalVelocity;
        
        var lookpos = targetRotation - transform.position;
        lookpos.y = 0;
        if (horMovement.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(horMovement),
                    Time.deltaTime * Rotation_damping
                );
            }     
        characterController.Move(movement * Time.deltaTime);
        // animator.SetFloat(speed, move.magnitude);
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnMouseLook(InputAction.CallbackContext context)
    {
        mouseLook = context.ReadValue<Vector2>();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouch = true;
        }
        if (context.canceled)
        {
            isCrouch = false;
        }
    }
    public void OnClap(InputAction.CallbackContext context)
    {

        if (context.started&&canClap)
        {
            StartCoroutine(SetNoiseforFrame());
        }
    }
    
    IEnumerator SetNoiseforFrame()
    {
        print("clapped");
        noisePosition = transform.position;
        noiseTrigger = true;
        canClap = false;
        yield return new WaitForEndOfFrame();
        noiseTrigger = false;
    }
}
