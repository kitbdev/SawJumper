using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        praise,
        dead,
        frozen,
        MAX
    }

    [Header("Speeds")]
    public float moveSpeed = 5;
    public float airMoveSpeed = 5;
    public float jumpMoveSpeed = 20;
    public float turnSpeed = 4;
    public float lockTurnSpeed = 2;

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
    public float respawnDur = 3;

    // [Header("Please set")]
    // public Transform followTarg;
    Transform cam;
    GameManager gm;
    Rigidbody rb;
    Animator anim;
    CapsuleCollider capsule;
    Health health;

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
    public Interactable nearInteractable;
    List<ContactPoint> curCps = new List<ContactPoint>();
    public bool camLocked = false;

    // input stuff
    Controls controls;
    Vector2 inpvel;
    Vector2 inplook;
    bool inpaction;
    // bool inpjump;
    bool inpjumpHold;
    bool inpIntraJumpRelease;

    void Awake() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        capsule = GetComponentInChildren<CapsuleCollider>();
        health = GetComponent<Health>();
        rb.useGravity = false;

        controls = new Controls();
        controls.Player.Move.performed += c => inpvel = c.ReadValue<Vector2>();
        controls.Player.Move.canceled += c => inpvel = Vector2.zero;
        controls.Player.Jump.performed += c => { inpjumpHold = true; }; // inpjump = true;
        controls.Player.Jump.canceled += c => { inpjumpHold = false; inpIntraJumpRelease = true; };
        controls.Player.Interact.performed += c => TryInteract();
        controls.Player.Look.performed += c => inplook = c.ReadValue<Vector2>();
        controls.Player.Look.canceled += c => inplook = Vector2.zero;
        controls.Player.Recenter.performed += c => camLocked = true;
        controls.Player.Recenter.canceled += c => camLocked = false;
        inpIntraJumpRelease = true;
        camLocked = false;
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
        health.deadEvent.AddListener(Die);
    }
    private void OnDisable() {
        controls.Disable();
        health.deadEvent.RemoveListener(Die);
    }

    public void SetInteractable(Interactable interactable) {
        nearInteractable = interactable;
    }
    public void TryInteract() {
        if (nearInteractable) {
            nearInteractable.Interacted();
        }
    }

    public void Die() {
        state = MoveState.dead;
        // then respawn after
        // Respawn();
        // anim.SetInteger("state",(int)state);
        // Debug.Log(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        // todo ui prompt instead??
        Invoke("Respawn", respawnDur);
    }
    public void Respawn() {
        state = MoveState.falling;
        vel = Vector3.zero;
        health.SetHealth();
        transform.position = lastRespawnT.position;
        transform.rotation = lastRespawnT.rotation;
    }
    public void SetRespawnPoint(Transform point) {
        // based on level? or dynamically to last (non moving) platform we were on?
        lastRespawnT.position = point.position;
        lastRespawnT.rotation = point.rotation;
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
    void Update() {
        if (transform.position.y < -50) {
            Respawn();
        }

        // rotation
        // followTarg.rotation *= Quaternion.AngleAxis(inplook.x * turnSpeed * Time.deltaTime, Vector3.up);

        // rotate while moving
        if (camLocked) {
            float turnAmount = inplook.x;
            transform.Rotate(0, turnAmount * lockTurnSpeed * Time.deltaTime, 0);
        } else {
            if (vel.sqrMagnitude > 0.01f) {
                Vector3 flatVel = vel;
                flatVel.y = 0;
                float ang = Mathf.LerpAngle(0, Vector3.SignedAngle(transform.forward, flatVel, Vector3.up), turnSpeed * Time.deltaTime);
                transform.rotation *= Quaternion.AngleAxis(ang, Vector3.up);
                anim.SetFloat("turn", ang);
            } else {
                anim.SetFloat("turn", 0);
            }
        }
    }
    void FixedUpdate() {
        CheckGrounded();
        // movement
        bool inLockedStates = state == MoveState.climbing || state == MoveState.dead;

        float grav = fallGravity;
        Vector3 inputVel = new Vector3(inpvel.x, 0, inpvel.y);
        Vector3 moveVel = Vector3.zero;
        bool anyMovement = inputVel.sqrMagnitude > 0.01f;
        if (inLockedStates) {
            anyMovement = false;
            vel = Vector3.zero;
        }
        if (anyMovement) {
            inputVel = cam.TransformDirection(inputVel);
            inputVel.y = 0;
        } else {
            inputVel = Vector3.zero;
        }
        if (isGrounded && (state == MoveState.idle || state == MoveState.walking || state == MoveState.falling)) {
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
                grav = 0;
                moveVel = Vector3.zero;
                // vel = Vector3.zero;
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
        if (inpjumpHold && !inLockedStates) {
            // check climbing
            // wall raycast
            RaycastHit hitWall, hitTop;
            Vector3 wallRayDir = transform.forward;
            float wallRayRad = 0.1f;
            float colRad = capsule.radius - 0.08f;
            Vector3 wallRayStart = transform.position + Vector3.up * (minClimbHeight + wallRayRad) + wallRayDir * (colRad - 0.1f);
            Vector3 wallRayStartTop = transform.position + Vector3.up * (maxClimbHeight - wallRayRad) + wallRayDir * (colRad - 0.1f);
            // Debug.Log("Check climb 1");
            Debug.DrawRay(wallRayStart, wallRayDir * maxClimbWallDist, Color.red, 2f, false);
            if (Physics.CapsuleCast(wallRayStart, wallRayStartTop, wallRayRad, wallRayDir, out hitWall, maxClimbWallDist, climbableLayer)) {
                // floor raycast
                Vector3 floorRayStart = hitWall.point + wallRayDir * capsule.radius + Vector3.up * (maxClimbHeight - hitWall.point.y + transform.position.y);
                // Debug.Log("Check climb 2 " + hitWall.collider.name);
                Debug.DrawRay(floorRayStart, Vector3.down * (maxClimbHeight - minClimbHeight), Color.blue, 2f, false);
                if (Physics.Raycast(floorRayStart, Vector3.down, out hitTop, maxClimbHeight - minClimbHeight, playerColMask)) {
                    // clearance space cast
                    Vector3 climbToPos = hitTop.point + Vector3.up * 0.05f;
                    float colHeight = capsule.height;
                    Vector3 capsuleStartHeightC = climbToPos + Vector3.up * (colHeight - colRad);
                    Vector3 capsuleEndHeightC = climbToPos + Vector3.up * colRad;
                    // Debug.Log("Check climb 3 " + climbToPos);
                    Debug.DrawLine(capsuleStartHeightC + Vector3.up * colRad, capsuleEndHeightC + Vector3.down * colRad, Color.yellow, 2f, false);
                    // Debug.DrawLine(capsuleStartHeightC + Vector3.left * colRad, capsuleStartHeightC + Vector3.right * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleStartHeightC + Vector3.forward * colRad, capsuleStartHeightC + Vector3.back * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleEndHeightC + Vector3.left * colRad, capsuleEndHeightC + Vector3.right * colRad, Color.yellow, 4f, false);
                    // Debug.DrawLine(capsuleEndHeightC + Vector3.forward * colRad, capsuleEndHeightC + Vector3.back * colRad, Color.yellow, 4f, false);
                    if (!Physics.CheckCapsule(capsuleStartHeightC, capsuleEndHeightC, colRad, playerColMask)) {
                        // can climb!
                        float relHeight = hitTop.point.y - transform.position.y;
                        // Debug.Log("Climbing " + hitWall.collider.name + " h:" + relHeight);
                        state = MoveState.climbing;
                        climbLanding.position = climbToPos;
                        climbLanding.forward = wallRayDir;
                        // for moving platforms?
                        climbLanding.parent = hitTop.transform;
                        vel = Vector3.zero;

                        // transform.position = climbLanding.position;
                        // transform.rotation = climbLanding.rotatio
                        // Tween tween = transform.DOLocalMove(climbToPos - wallRayDir * colRad - Vector3.up * colHeight, 0.1f);
                        // tween.SetLoops(0);
                        // tween.Play();
                        // FinishClimb();
                        // debug
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
            // inpjump = false;
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
            // Debug.Log($"conVel: {connectedVel} conrot: {rotVel}");
            vel += connectedVel; // + rotVel;
        }

        rb.velocity = vel;
        // Debug.Log("state: " + state);
        // anim.SetFloat("velx", vel.x);
        anim.SetFloat("speed", vel.magnitude / moveSpeed);
        anim.SetInteger("state", (int) state);
        anim.SetBool("grounded", isGrounded);
    }
    public void ClimbRepos() {
        // state = MoveState.falling;
        transform.position = climbLanding.position;
        transform.rotation = climbLanding.rotation;
    }
    public void FinishClimb() {
        state = MoveState.falling;
        transform.position = climbLanding.position;
        transform.rotation = climbLanding.rotation;
    }
    private void OnCollisionEnter(Collision other) {
        // Debug.Log("colenter");
        ReportCol(other);
    }
    private void OnCollisionStay(Collision other) {
        // Debug.Log("colstay");
        ReportCol(other);
    }
    private void OnCollisionExit(Collision other) {
        // Debug.Log("colex");
        // CheckGrounded(other);
    }
    void ReportCol(Collision other) {
        ContactPoint[] contacts = new ContactPoint[other.contactCount];
        other.GetContacts(contacts);
        foreach (var cp in contacts) {
            curCps.Add(cp);
        }
    }
    void CheckGrounded() {
        bool wasGrounded = isGrounded;
        wallContact = false;
        isGrounded = false;
        var wasConnectedBodyRb = connectedBody;
        connectedBody = null;
        Vector3 center = transform.position + Vector3.up;
        // ContactPoint[] contacts = new ContactPoint[other.contactCount];
        // other.GetContacts(contacts);
        // Debug.Log(other.contactCount + " contacts");
        // float surfaceThres = 0.7f;
        foreach (var cp in curCps) {
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
            // Debug.Log("grounded: " + isGrounded);
        }
        curCps.Clear();
    }
    void UpdateConnectedPos() {
        connectedGPos = connectedBody.transform.position;
        connectedLPos = connectedBody.transform.InverseTransformPoint(transform.position - connectedGPos);
    }
}