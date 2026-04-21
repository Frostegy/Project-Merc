using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieManager : MonoBehaviour
{
    [Header("Damage Multipliers")]
    public float headShotMultiplier = 1.5f;
    public float torsoDamageMultiplier = 1.0f;
    public float armDamageMultiplier = 0.6f;
    public float legDamageMultiplier = 0.75f;

    [Header("Limb Removal")]
    [SerializeField] private GameObject leftArmBoneObject;
    [SerializeField] private GameObject rightArmBoneObject;
    [SerializeField] private GameObject headBoneObject;

    [Header("Health")]
    public int overallHealth = 100;
    public int headhealth = 40;
    public int torsoHealth = 100;
    public int leftArmHealth = 40;
    public int rightArmHealth = 40;
    public int leftLegHealth = 50;
    public int rightLegHealth = 50;

    [Header("Stagger Chances")]
    [Range(0f, 1f)] public float headStaggerChance = 1.0f;
    [Range(0f, 1f)] public float torsoStaggerChance = 0.3f;
    [Range(0f, 1f)] public float armStaggerChance = 0.2f;
    [Range(0f, 1f)] public float legStaggerChance = 0.5f;

    [Header("Drops")]
    [SerializeField, Range(0f, 1f)] private float healDropChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float ammoDropChance = 0.6f;

    [SerializeField] private GameObject healPickupPrefab;
    [SerializeField] private GameObject handgunAmmoPickupPrefab;
    [SerializeField] private GameObject shotgunAmmoPickupPrefab;
    [SerializeField] private GameObject rifleAmmoPickupPrefab;
    [SerializeField] private GameObject magnumAmmoPickupPrefab;

    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);

    private bool isDead;
    private bool headRemoved;
    private bool leftArmRemoved;
    private bool rightArmRemoved;

    private Animator anim;
    private NavMeshAgent agent;
    private ZombieController zombie;
    private WaveEnemy waveEnemy;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zombie = GetComponent<ZombieController>();
        waveEnemy = GetComponent<WaveEnemy>();
    }

    public void TakeDamage(ZombieLimb limb, int damage)
    {
        if (isDead)
            return;

        switch (limb)
        {
            case ZombieLimb.Head:
                DealHeadShotDamage(damage);
                TryStagger(headStaggerChance);
                break;

            case ZombieLimb.Torso:
                DealTorsoDamage(damage);
                TryStagger(torsoStaggerChance);
                break;

            case ZombieLimb.LeftArm:
                DealArmDamage(true, damage);
                TryStagger(armStaggerChance);
                break;

            case ZombieLimb.RightArm:
                DealArmDamage(false, damage);
                TryStagger(armStaggerChance);
                break;

            case ZombieLimb.LeftLeg:
                DealLegDamage(true, damage);
                TryStagger(legStaggerChance);
                break;

            case ZombieLimb.RightLeg:
                DealLegDamage(false, damage);
                TryStagger(legStaggerChance);
                break;
        }

        overallHealth = Mathf.Max(0, overallHealth);
        headhealth = Mathf.Max(0, headhealth);
        torsoHealth = Mathf.Max(0, torsoHealth);
        leftArmHealth = Mathf.Max(0, leftArmHealth);
        rightArmHealth = Mathf.Max(0, rightArmHealth);
        leftLegHealth = Mathf.Max(0, leftLegHealth);
        rightLegHealth = Mathf.Max(0, rightLegHealth);

        DeathCheck();
    }

    public void DealHeadShotDamage(int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * headShotMultiplier);

        headhealth -= finalDamage;
        overallHealth -= finalDamage;

        if (headhealth <= 0 && !headRemoved)
        {
            RemoveHead();
        }
    }

    public void DealTorsoDamage(int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * torsoDamageMultiplier);
        torsoHealth -= finalDamage;
        overallHealth -= finalDamage;
    }

    public void DealArmDamage(bool leftArmDamage, int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * armDamageMultiplier);

        if (leftArmDamage)
        {
            leftArmHealth -= finalDamage;

            if (leftArmHealth <= 0 && !leftArmRemoved)
            {
                RemoveArm(true);
            }
        }
        else
        {
            rightArmHealth -= finalDamage;

            if (rightArmHealth <= 0 && !rightArmRemoved)
            {
                RemoveArm(false);
            }
        }

        overallHealth -= finalDamage;
    }

    public void DealLegDamage(bool leftLegDamage, int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * legDamageMultiplier);

        if (leftLegDamage)
        {
            leftLegHealth -= finalDamage;
        }
        else
        {
            rightLegHealth -= finalDamage;
        }

        overallHealth -= finalDamage;
    }

    void TryStagger(float chance)
    {
        if (isDead)
            return;

        if (Random.value <= chance && anim != null)
        {
            anim.SetTrigger("Stagger");
        }
    }

    void DeathCheck()
    {
        if (overallHealth <= 0 || headhealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (zombie != null)
        {
            zombie.enabled = false;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Swipe");
            anim.ResetTrigger("Stagger");
            anim.SetBool("isWalking", false);
            anim.SetBool("isGrabbing", false);
            anim.SetTrigger("Die");
        }

        WaveEnemy currentWaveEnemy = GetComponent<WaveEnemy>();
        if (currentWaveEnemy != null)
        {
            currentWaveEnemy.ReportDeath();
        }

        TryDropLoot();

        Destroy(gameObject, 4f);
    }

    void TryDropLoot()
    {
        float roll = Random.value;

        if (roll <= healDropChance)
        {
            SpawnDrop(healPickupPrefab);
            return;
        }

        if (roll <= healDropChance + ammoDropChance)
        {
            DropAmmoForEquippedWeapon();
        }
    }

    void DropAmmoForEquippedWeapon()
    {
        ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
        if (activeWeapon == null)
            return;

        RayCastWeapon[] equippedWeapons = activeWeapon.equippedWeapons;
        if (equippedWeapons == null || equippedWeapons.Length == 0)
            return;

        List<GameObject> possibleDrops = new List<GameObject>();

        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            RayCastWeapon weapon = equippedWeapons[i];

            if (weapon == null || weapon.weaponData == null)
                continue;

            GameObject ammoPrefab = GetAmmoPickupPrefab(weapon.weaponData.ammoType);

            if (ammoPrefab != null)
            {
                possibleDrops.Add(ammoPrefab);
            }
        }

        if (possibleDrops.Count == 0)
            return;

        GameObject chosenDrop = possibleDrops[Random.Range(0, possibleDrops.Count)];
        SpawnDrop(chosenDrop);
    }

    GameObject GetAmmoPickupPrefab(AmmoType ammoType)
    {
        switch (ammoType)
        {
            case AmmoType.Handgun:
                return handgunAmmoPickupPrefab;

            case AmmoType.Shotgun:
                return shotgunAmmoPickupPrefab;

            case AmmoType.Rifle:
                return rifleAmmoPickupPrefab;

            case AmmoType.Magnum:
                return magnumAmmoPickupPrefab;
        }

        return null;
    }

    void SpawnDrop(GameObject pickupPrefab)
    {
        if (pickupPrefab == null)
            return;

        Instantiate(pickupPrefab, transform.position + dropOffset, Quaternion.identity);
    }

    void RemoveArm(bool leftArm)
    {
        if (leftArm)
        {
            leftArmRemoved = true;

            if (leftArmBoneObject != null)
            {
                Destroy(leftArmBoneObject);
            }
        }
        else
        {
            rightArmRemoved = true;

            if (rightArmBoneObject != null)
            {
                Destroy(rightArmBoneObject);
            }
        }
    }

    void RemoveHead()
    {
        headRemoved = true;

        if (headBoneObject != null)
        {
            Destroy(headBoneObject);
        }
    }
}

