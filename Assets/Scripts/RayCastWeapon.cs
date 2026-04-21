using UnityEngine;

public class RayCastWeapon : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;
    public ActiveWeapon.WeaponSlot weaponSlot;

    [Header("Effects")]
    public ParticleSystem[] muzzleFlash;
    public ParticleSystem hitEffect;

    [Header("Raycast")]
    public Transform rayCastOrigin;
    public Transform rayCastDestination;

    public string weaponName
    {
        get
        {
            if (weaponData == null)
                return string.Empty;

            return weaponData.weaponName;
        }
    }

    public void FireOnce()
    {
        if (weaponData == null || rayCastOrigin == null || rayCastDestination == null)
        {
            return;
        }

        foreach (ParticleSystem particle in muzzleFlash)
        {
            if (particle != null)
            {
                particle.Emit(1);
            }
        }

        int pelletCount = Mathf.Max(1, weaponData.pelletsPerShot);

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = (rayCastDestination.position - rayCastOrigin.position).normalized;

            if (weaponData.spread > 0f)
            {
                direction += new Vector3(
                    Random.Range(-weaponData.spread, weaponData.spread),
                    Random.Range(-weaponData.spread, weaponData.spread),
                    Random.Range(-weaponData.spread, weaponData.spread)
                );

                direction.Normalize();
            }

            Ray ray = new Ray(rayCastOrigin.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, weaponData.range))
            {
                Debug.DrawLine(ray.origin, hitInfo.point, Color.red, 1f);

                if (hitEffect != null)
                {
                    hitEffect.transform.position = hitInfo.point;
                    hitEffect.transform.forward = hitInfo.normal;
                    hitEffect.Emit(1);
                }

                LimbHitbox zombieHitbox = hitInfo.collider.GetComponent<LimbHitbox>();

                if (zombieHitbox != null)
                {
                    zombieHitbox.TakeHit(weaponData.damage);
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.yellow, 1f);
            }
        }
    }

}