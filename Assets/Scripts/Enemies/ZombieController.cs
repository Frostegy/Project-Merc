using UnityEngine;
using UnityEngine.AI;

enum ZombieState
{
    Chasing,
    Attacking,
    Grabbing
}

public class ZombieController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Swipe Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int swipeDamage = 15;
    [SerializeField] private float swipeMoveSpeedMultiplier = 0.65f;
    [SerializeField] private float swipeStoppingDistance = 0.15f;
    [SerializeField] private float swipeKnockbackForce = 3f;

    [Header("Spacing")]
    [SerializeField] private float minDistanceFromPlayer = 0.9f;
    [SerializeField] private float resumeMoveDistance = 1.05f;

    [Header("Attack Choice")]
    [SerializeField, Range(0f, 1f)] private float grabChance = 0.25f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Grab")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private KeyCode escapeKey = KeyCode.E; // display only for GrabUI for now
    [SerializeField] private int pressesNeeded = 8;
    [SerializeField] private float mashDecayPerSecond = 2f;
    [SerializeField] private float biteInterval = 1f;
    [SerializeField] private int biteDamage = 5;
    [SerializeField] private GrabUI grabUI;

    private NavMeshAgent agent;
    private Animator anim;
    private PlayerController playerController;
    private InputManager inputManager;

    private ZombieState state = ZombieState.Chasing;

    private bool isSwiping;
    private bool isGrabbing;

    private float cooldownTimer;
    private float mashProgress;
    private float biteTimer;

    private float normalSpeed;
    private float normalStoppingDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        normalSpeed = agent.speed;
        normalStoppingDistance = Mathf.Max(attackRange, minDistanceFromPlayer);
        agent.stoppingDistance = normalStoppingDistance;

        if (player == null)
        {
            PlayerController foundPlayer = FindFirstObjectByType<PlayerController>();
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                playerController = foundPlayer;
                inputManager = foundPlayer.GetComponent<InputManager>();
            }
        }
        else
        {
            playerController = player.GetComponent<PlayerController>();
            inputManager = player.GetComponent<InputManager>();
        }

        if (grabUI == null)
        {
            grabUI = FindFirstObjectByType<GrabUI>(FindObjectsInactive.Include);
        }
    }

    private void Update()
    {
        if (player == null) return;
        if (GamePause.IsPaused) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        switch (state)
        {
            case ZombieState.Chasing:
                HandleChasing();
                break;

            case ZombieState.Attacking:
                HandleAttacking();
                break;

            case ZombieState.Grabbing:
                agent.isStopped = true;
                FacePlayer();
                UpdateGrab();
                break;
        }

        anim.SetBool("isWalking", state == ZombieState.Chasing && agent.velocity.magnitude > 0.1f);
    }

    private void HandleChasing()
    {
        agent.speed = normalSpeed;
        agent.stoppingDistance = Mathf.Max(normalStoppingDistance, minDistanceFromPlayer);
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && cooldownTimer <= 0f)
        {
            ChooseAttack();
            return;
        }

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            return;
        }

        if (distanceToPlayer <= minDistanceFromPlayer)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    private void HandleAttacking()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= minDistanceFromPlayer)
        {
            agent.isStopped = true;
        }
        else if (distanceToPlayer >= resumeMoveDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        FacePlayer();

        if (!isSwiping)
            EndSwipe();
    }

    private void ChooseAttack()
    {
        FacePlayer();

        bool canGrab = playerController != null && !playerController.isGrabbed;
        bool shouldGrab = Random.value < grabChance && canGrab;

        if (shouldGrab)
        {
            StartGrab();
        }
        else
        {
            StartSwipe();
        }
    }

    private void StartSwipe()
    {
        if (isSwiping || isGrabbing) return;

        state = ZombieState.Attacking;
        isSwiping = true;

        agent.isStopped = false;
        agent.speed = normalSpeed * swipeMoveSpeedMultiplier;
        agent.stoppingDistance = Mathf.Max(swipeStoppingDistance, minDistanceFromPlayer);
        agent.SetDestination(player.position);

        anim.CrossFadeInFixedTime("zombie_anim_attack", 0.05f);
    }

    private void EndSwipe()
    {
        agent.speed = normalSpeed;
        agent.stoppingDistance = normalStoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        state = ZombieState.Chasing;
        anim.CrossFadeInFixedTime("zombie_anim_walk", 0.05f);
    }

    private void StartGrab()
    {
        state = ZombieState.Grabbing;
        isGrabbing = true;
        mashProgress = 0f;
        biteTimer = biteInterval;

        agent.isStopped = true;
        agent.ResetPath();

        if (playerController != null)
            playerController.BeginGrab(grabPoint != null ? grabPoint : transform);

        if (grabUI != null)
        {
            grabUI.Show(escapeKey);
            grabUI.SetProgress(0f);
        }

        anim.SetBool("isGrabbing", true);
    }

    private void UpdateGrab()
    {
        if (inputManager != null && inputManager.breakFreePressedInput)
            mashProgress++;

        mashProgress -= mashDecayPerSecond * Time.deltaTime;
        mashProgress = Mathf.Clamp(mashProgress, 0f, pressesNeeded);

        if (grabUI != null)
            grabUI.SetProgress(mashProgress / pressesNeeded);

        biteTimer -= Time.deltaTime;
        if (biteTimer <= 0f)
        {
            if (!GamePause.IsPaused)
            {
                PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
                if (health != null)
                    health.TakeDamage(biteDamage);
            }

            biteTimer = biteInterval;
        }

        if (mashProgress >= pressesNeeded)
            ReleaseGrab();
    }

    private void ReleaseGrab()
    {
        isGrabbing = false;
        state = ZombieState.Chasing;
        cooldownTimer = attackCooldown;

        if (playerController != null)
            playerController.EndGrab();

        if (grabUI != null)
            grabUI.Hide();

        anim.SetBool("isGrabbing", false);

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
    }

    public void DoSwipeDamage()
    {
        if (GamePause.IsPaused) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange + 0.5f) return;

        PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(swipeDamage);

        Rigidbody rb = player.GetComponentInParent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = player.position - transform.position;
            pushDir.y = 0f;
            pushDir.Normalize();

            Vector3 knockback = (pushDir + Vector3.up * 0.15f).normalized;
            rb.AddForce(knockback * swipeKnockbackForce, ForceMode.Impulse);
        }
    }

    public void SwipingEnd()
    {
        isSwiping = false;
        cooldownTimer = attackCooldown;
    }

    private void OnDisable()
    {
        if (grabUI != null)
            grabUI.Hide();
    }

}