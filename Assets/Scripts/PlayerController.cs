using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 15.0f;
    private float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Animator")]
    public Animator anim;
    public bool isJumpingAnim;
    public bool isGroundedAnim;


    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    //private float cameraZOffset = -1.0f;
    private Camera playerCamera;

    


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x , transform.position.y + cameraYOffset, transform.position.z );
            playerCamera.transform.SetParent(transform);
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

      
    }

    void Update()
    {
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedZ = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedZ);

        // Handle Animations
        if(moveDirection != Vector3.zero)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving",false);
        }
        float inputMagnitude = Mathf.Clamp01(moveDirection.magnitude);
        float speed = inputMagnitude * walkingSpeed;
        if (isRunning)
        {
            inputMagnitude = inputMagnitude * 2;
        }
        anim.SetFloat("Input Magnitude", inputMagnitude, 0.1f, Time.deltaTime);

        if (characterController.isGrounded)
        {
            anim.SetBool("isGrounded", true);
            anim.SetBool("isFalling", false);
            anim.SetBool("isJumping", false);

        }
        else
        {
            anim.SetBool("isGrounded", false);
            anim.SetBool("isFalling", true);
        }

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
            anim.SetBool("isJumping", true);
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        TempShooting();

        // Hold left Alt to glide
        if (Input.GetKey(KeyCode.LeftAlt) && !characterController.isGrounded) 
        {
            gravity = 5.0f;
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (!characterController.isGrounded)
        {
            gravity = 20.0f;
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            gravity = 20.0f;
        }



        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlock Curser
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    private void TempShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetBool("Shoot", true);

        }
        if (Input.GetMouseButtonUp(0))
        {
            anim.SetBool("Shoot", false);
        }
    }
}
