﻿using System.Collections;
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
        climbing,
        fixedAnim,
        dead,
        frozen,
        MAX
    }

    [Header("Speeds")]
    public float moveSpeed = 5;
    public float airMoveSpeed = 5;
    public float jumpMoveSpeed = 20;
    public float turnSpeed = 4;

    [Header("Distances")]
    public float jumpDist = 4;
    public float jumpHeight = 2;
    public float jumpDistMin = 1f;
    public float jumpHeightMin = 0.4f;
    public float maxJumps = 1;
    public float coyoteTimeDur = 0.2f;
    public float maxClimbHeight = 2f;
    public float minClimbHeight = 0.2f;
    public float maxClimbWallDist = 0.5f;
    public LayerMask climbableLayer = 1;
    public LayerMask playerColMask = 1;

    // [Header("Please set")]
    // public Transform followTarg;
    Transform cam;
    GameManager gm;
    Rigidbody rb;
    Animator anim;
    CapsuleCollider capsule;

    [Space]
    [Header("*Calculated*")]
    public float jumpGravity;
    public float fallGravity;
    public float idleGravity;
    public float jumpVel;
    public float minJumpDur;

    [Header("*Dynamic*")]
    public bool isGrounded;
    public bool wallContact;
    public Vector3 wallContactNormal = Vector3.zero;
    public float numJumps = 1;
    float minJumpTimer = 0;
    float lastGroundedTime;
    public Vector3 vel;
    public MoveState state = MoveState.idle;
    public Transform lastRespawnT;
    public Transform climbLanding;
    public Rigidbody connectedBody;
    public Vector3 connectedGPos;
    public Vector3 connectedLPos;

    // input stuff
    Controls controls;
    Vector2 inpvel;
    Vector2 inplook;
    bool inpaction;
    bool inpjump;
    bool inpjumpHold;
    bool inpIntraJumpRelease;

    void Awake() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        capsule = GetComponentInChildren<CapsuleCollider>();

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

    }
    private void Start() {
        state = MoveState.falling;
        CalcJump();

        climbLanding = new GameObject().transform;
        climbLanding.parent = transform;
        climbLanding.name = "Climbing helper";
        climbLanding.localPosition = Vector3.zero;
        lastRespawnT = new GameObject().transform;
        lastRespawnT.name = "Respawn Point";
        lastRespawnT.parent = gm.transform;
        lastRespawnT.position = Vector3.zero;
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
        jumpVel = CalcVel(jumpHeight, bigJumpDist, jumpMoveSpeed);
        jumpGravity = CalcGrav(jumpHeight, bigJumpDist, jumpMoveSpeed);
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
        vel = Vector3.zero;
        // todo move last respawn point
        transform.position = lastRespawnT.position;
        transform.rotation = lastRespawnT.rotation;
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
        // state transitions
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
                bool hitWall = wallContact && Vector3.Dot(vel, wallContactNormal) < -0.3f;
                if (vel.y <= 0 || (inpjumpHold == false && Time.time >= minJumpTimer + minJumpDur) || hitWall) {
                    state = MoveState.falling;
                }
                break;
            case MoveState.climbing:
                // wait for anim
                break;
            case MoveState.dead:
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
                moveVel = inputVel * jumpMoveSpeed;
                break;
            case MoveState.falling:
                grav = fallGravity;
                moveVel = inputVel * airMoveSpeed;
                break;
            case MoveState.climbing:
                grav = 0;
                moveVel = Vector3.zero;
                break;
            case MoveState.dead:
                // respawn timer
                // Respawn();
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

        bool inputJump = inpjumpHold && inpIntraJumpRelease;
        if (inputJump) {
            // check climbing
            // wall raycast
            // todo also check when wall is up off the ground
            RaycastHit hitWall, hitTop;
            Vector3 wallRayDir = transform.forward;
            Vector3 wallRayStart = transform.position + Vector3.up * minClimbHeight + wallRayDir * (capsule.radius - 0.05f);
            // Debug.Log("Check climb 1");
            // Debug.DrawRay(wallRayStart, wallRayDir * maxClimbWallDist, Color.red, 2f, false);
            if (Physics.Raycast(wallRayStart, wallRayDir, out hitWall, maxClimbWallDist, climbableLayer)) {
                // floor raycast
                Vector3 floorRayStart = hitWall.point + wallRayDir * capsule.radius + Vector3.up * (maxClimbHeight - hitWall.point.y + transform.position.y);
                // Debug.Log("Check climb 2 " + hitWall.collider.name);
                // Debug.DrawRay(floorRayStart, Vector3.down * (maxClimbHeight - minClimbHeight), Color.blue, 2f, false);
                if (Physics.Raycast(floorRayStart, Vector3.down, out hitTop, maxClimbHeight - minClimbHeight, playerColMask)) {
                    // clearance space cast
                    Vector3 climbToPos = hitTop.point + Vector3.up * 0.05f;
                    float colHeight = capsule.height;
                    float colRad = capsule.radius - 0.05f;
                    Vector3 capsuleStartHeightC = climbToPos + Vector3.up * (colHeight - colRad);
                    Vector3 capsuleEndHeightC = climbToPos + Vector3.up * colRad;
                    // Debug.Log("Check climb 3 " + climbToPos);
                    // Debug.DrawLine(capsuleStartHeightC + Vector3.up * colRad, capsuleEndHeightC + Vector3.down * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleStartHeightC + Vector3.left * colRad, capsuleStartHeightC + Vector3.right * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleStartHeightC + Vector3.forward * colRad, capsuleStartHeightC + Vector3.back * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleEndHeightC + Vector3.left * colRad, capsuleEndHeightC + Vector3.right * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleEndHeightC + Vector3.forward * colRad, capsuleEndHeightC + Vector3.back * colRad, Color.yellow, 4f, false);
                    if (!Physics.CheckCapsule(capsuleStartHeightC, capsuleEndHeightC, colRad, playerColMask)) {
                        // can climb!
                        float relHeight = hitTop.point.y - transform.position.y;
                        Debug.Log("Can climb " + hitWall.collider.name + " h:" + relHeight);
                        state = MoveState.climbing;
                        climbLanding.position = climbToPos;
                        climbLanding.forward = wallRayDir;
                        // for moving platforms
                        climbLanding.parent = hitTop.transform;
                        vel = Vector3.zero;
                        // todo start anim instead
                        FinishClimb();
                        // } else {
                        //     var cs = Physics.OverlapCapsule(capsuleStartHeightC, capsuleEndHeightC, colRad, playerColMask);
                        //     Debug.Log("obstructed!");
                        //     foreach (var c in cs) {
                        //         Debug.Log(" by " + c.name);
                        //     }
                    }
                }
            }
        }
        // jump
        // note: doesn't allow for air jumps
        bool canJump = numJumps < maxJumps && (isGrounded || Time.time <= lastGroundedTime + coyoteTimeDur);
        if (inputJump && canJump && state != MoveState.climbing) {
            state = MoveState.jumping;
            vel.y = jumpVel;
            numJumps++;
            inpjump = false;
            isGrounded = false;
            minJumpTimer = Time.time;
            inpIntraJumpRelease = false;
        }
        // moving platforms
        if (connectedBody) {
            Vector3 connectedVel = connectedBody.transform.position - connectedGPos;
            connectedVel /= Time.deltaTime;
            Vector3 lastLPos = connectedLPos;
            UpdateConnectedPos();
            Vector3 rotVel = connectedLPos - lastLPos;
            rotVel /= Time.deltaTime;
            Debug.Log($"conVel: {connectedVel} conrot: {rotVel}");
            vel += connectedVel;// + rotVel;
        }

        rb.velocity = vel;
        // Debug.Log("state: " + state);
        // anim.SetFloat("velx", vel.x);
        // anim.SetFloat("vely", vel.y);
        // anim.SetInteger("state", (int) state);
    }
    public void FinishClimb() {
        state = MoveState.falling;
        transform.position = climbLanding.position;
        transform.rotation = climbLanding.rotation;
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
        bool wasGrounded = isGrounded;
        wallContact = false;
        isGrounded = false;
        var wasConnectedBodyRb = connectedBody;
        connectedBody = null;
        Vector3 center = transform.position + Vector3.up;
        ContactPoint[] contacts = new ContactPoint[other.contactCount];
        other.GetContacts(contacts);
        // Debug.Log(other.contactCount + " contacts");
        // float surfaceThres = 0.7f;
        foreach (var cp in contacts) {
            float updot = Vector3.Dot(cp.normal, Vector3.up);
            if (updot >= 0.7f) {
                isGrounded = true;
                connectedBody = cp.otherCollider.attachedRigidbody;
                break;
            } else if (updot > -0.7f) {
                // hit a wall
                wallContact = true;
                wallContactNormal = cp.normal;
            }
            // cp.otherCollider - center
        }
        if (connectedBody && connectedBody != wasConnectedBodyRb) {
            UpdateConnectedPos();
        }
        if (wasGrounded ^ isGrounded) {
            // state changed
            lastGroundedTime = Time.time;
        }
    }
    void UpdateConnectedPos() {
        connectedGPos = connectedBody.transform.position;
        connectedLPos = connectedBody.transform.InverseTransformPoint(transform.position - connectedGPos);
    }
}