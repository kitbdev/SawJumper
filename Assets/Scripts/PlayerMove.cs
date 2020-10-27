using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour {

    public enum MoveState {
        idle,
        walking,
        jumping,
        falling,
        dead,
        frozen,
        MAX
    }

    [Header("Speeds")]
    public float moveSpeed = 5;
    public float airMoveSpeed = 5;
    public float turnSpeed = 4;

    [Header("Distances")]
    public float jumpDist = 4;
    public float jumpHeight = 2;
    public float jumpDistMin = 1f;
    public float jumpHeightMin = 0.4f;
    public float maxJumps = 1;

    [Header("Please set")]
    public Transform followTarg;
    Transform cam;
    Rigidbody rb;
    Animator anim;

    [Space]
    [Header("*Calculated*")]
    public float jumpGravity;
    public float fallGravity;
    public float idleGravity;
    public float jumpVel;
    public float minJumpDur;

    [Header("*Dynamic*")]
    public float numJumps = 1;
    public float glideTimer = 0;
    float minJumpTimer = 0;
    public bool isGrounded;
    public Vector3 vel;
    public MoveState state = MoveState.idle;

    Controls controls;
    Vector2 inpvel;
    public Vector2 inplook;
    bool inpaction;
    bool inpjump;
    bool inpjumpHold;
    bool inpIntraJumpRelease;

    void Awake() {
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.useGravity = false;

        controls = new Controls();
        controls.Player.Move.performed += c => inpvel = c.ReadValue<Vector2>();
        controls.Player.Move.canceled += c => inpvel = Vector2.zero;
        controls.Player.Jump.performed += c => { inpjump = true; inpjumpHold = true; };
        controls.Player.Jump.canceled += c => { inpjumpHold = false; inpIntraJumpRelease = true; };
        controls.Player.Interact.performed += c => TryInteract();
        controls.Player.Look.performed += c => inplook = c.ReadValue<Vector2>();
        controls.Player.Look.canceled += c => inplook = Vector2.zero;
        inpIntraJumpRelease = true;

        state = MoveState.falling;
        CalcJump();
    }
    private void OnEnable() {
        controls.Enable();
    }
    private void OnDisable() {
        controls.Disable();
    }

    public void TryInteract() {

    }

    [ContextMenu("Recalc Jump")]
    public void CalcJump() {
        // use jump dist and jump height to calculate the jump vel and gravity
        // fast fall
        float bigJumpDist = jumpDist * 4 / 3;
        float smallJumpDist = jumpDist * 3 / 4;
        jumpVel = CalcVel(jumpHeight, bigJumpDist, airMoveSpeed);
        jumpGravity = CalcGrav(jumpHeight, bigJumpDist, airMoveSpeed);
        fallGravity = CalcGrav(jumpHeight, smallJumpDist, airMoveSpeed);
        minJumpDur = jumpVel / CalcGrav(jumpHeightMin, jumpDistMin, airMoveSpeed);
        idleGravity = 0.01f;
    }
    float CalcVel(float height, float dist, float vel) {
        return 2 * height * vel / dist;
    }
    float CalcGrav(float height, float dist, float vel) {
        return 2 * height * vel * vel / (dist * dist);
    }
    void Respawn() {
        state = MoveState.falling;
        // todo
        transform.position = Vector3.zero;
    }
    void Update() {
        if (transform.position.y < -50) {
            Respawn();
        }

        // rotation
        // followTarg.rotation *= Quaternion.AngleAxis(inplook.x * turnSpeed * Time.deltaTime, Vector3.up);

        // rotate while moving
        if (vel.sqrMagnitude > 0.01f) {
            Vector3 flatVel = vel;
            flatVel.y = 0;
            float ang = Mathf.LerpAngle(0, Vector3.SignedAngle(transform.forward, flatVel, Vector3.up), turnSpeed * Time.deltaTime);
            transform.rotation *= Quaternion.AngleAxis(ang, Vector3.up);
        }
    }
    void FixedUpdate() {
        // movement
        float grav = fallGravity;
        Vector3 inputVel = new Vector3(inpvel.x, 0, inpvel.y);
        Vector3 moveVel = Vector3.zero;
        bool anyMovement = inputVel.sqrMagnitude > 0.01f;
        if (anyMovement) {
            inputVel = cam.TransformDirection(inputVel);
            inputVel.y = 0;
        } else {
            inputVel = Vector3.zero;
        }
        if (isGrounded && state != MoveState.jumping) {
            if (anyMovement) {
                state = MoveState.walking;
            } else {
                state = MoveState.idle;
            }
            numJumps = 0;
            grav = idleGravity;
            vel.y = 0;
            moveVel = inputVel * moveSpeed;
        }
        switch (state) {
            case MoveState.idle:
                if (!isGrounded) {
                    state = MoveState.falling;
                }
                break;
            case MoveState.walking:
                if (!isGrounded) {
                    state = MoveState.falling;
                }
                break;
            case MoveState.jumping:
                if (vel.y <= 0 || (inpjumpHold == false && Time.time >= minJumpTimer + minJumpDur)) {
                    state = MoveState.falling;
                }
                break;
            case MoveState.dead:
                // respawn timer
                break;
            case MoveState.frozen:
                break;
            default:
                break;
        }
        // state actions
        switch (state) {
            case MoveState.idle:
                grav = idleGravity;
                moveVel = inputVel * moveSpeed;
                break;
            case MoveState.walking:
                grav = idleGravity;
                moveVel = inputVel * moveSpeed;
                break;
            case MoveState.jumping:
                grav = jumpGravity;
                moveVel = inputVel * airMoveSpeed;
                break;
            case MoveState.falling:
                grav = fallGravity;
                moveVel = inputVel * airMoveSpeed;
                break;
            case MoveState.dead:
                break;
            case MoveState.frozen:

                break;
            default:
                break;
        }
        // move
        moveVel = moveVel - vel;
        moveVel.y = 0;
        vel += moveVel;
        vel += Vector3.down * grav * Time.deltaTime;

        // jump
        if (inpjumpHold && numJumps < maxJumps && inpIntraJumpRelease) {
            state = MoveState.jumping;
            vel.y = jumpVel;
            numJumps++;
            inpjump = false;
            isGrounded = false;
            minJumpTimer = Time.time;
            inpIntraJumpRelease = false;
        }
        // todo moving platforms

        rb.velocity = vel;
        // Debug.Log("state: " + state);
        // anim.SetFloat("velx", vel.x);
        // anim.SetFloat("vely", vel.y);
        // anim.SetInteger("state", (int) state);
    }
    private void OnCollisionEnter(Collision other) {
        CheckGrounded(other);
    }
    private void OnCollisionStay(Collision other) {
        CheckGrounded(other);
    }
    private void OnCollisionExit(Collision other) {
        CheckGrounded(other);
    }
    void CheckGrounded(Collision other) {
        Collider col = GetComponent<Collider>();
        isGrounded = false;
        Vector3 center = transform.position + Vector3.up;
        foreach (var cp in other.contacts) {
            if (Vector3.Dot(cp.normal, Vector3.up) > 0.7f) {
                isGrounded = true;
                break;
            }
            // cp.otherCollider - center
        }
        // check if we hit a wall or something
        // for grabbing onto?
    }
}