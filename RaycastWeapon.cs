using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RaycastWeaponPreset weaponPreset;
    [SerializeField] private Transform gunTip;
    [SerializeField] private Transform shootingPosition;

    [Header("General Info")]
    [SerializeField] private string weaponName;
    [SerializeField] private int magazineCapacity;
    [SerializeField] private float shotsPerSecond;
    [SerializeField] private float effectiveRange;
    [SerializeField] private float damage;
    [SerializeField] private float reloadTime;
    [SerializeField] private LayerMask shootable;

    [Header("CurrentStatus")]
    [SerializeField] private int bulletsInMagazine;
    [SerializeField] private float nextTimeToFire;


    private InputManager inputManager;
    private Animator animator;
    private AudioSource weaponSoundSource;
    private RaycastHit weaponHit;

    private bool canShoot;
    private bool isReloading;

    /*LOADING AND UNLOADING OF DATA*/
    #region
    private void Awake()
    {
        weaponSoundSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        LoadWeaponData();
        inputManager = FindObjectOfType<InputManager>();
    }
    private void OnDisable()
    {
        UnloadWeaponData();
        inputManager = null;
    }

    /*LOADING AND UNLOADING OF WEAPON DATA*/
    private void LoadWeaponData()
    {
        weaponName = weaponPreset.name;
        magazineCapacity = weaponPreset.magazineCapacity;
        shotsPerSecond = weaponPreset.shotsPerSecond;
        effectiveRange = weaponPreset.effectiveRange;
        damage = weaponPreset.damage;
        reloadTime = weaponPreset.reloadTime;
        shootable = weaponPreset.shootable;

        bulletsInMagazine = weaponPreset.magazineCapacity;
        isReloading = false;
        canShoot = true;
    }
    private void UnloadWeaponData()
    {
        weaponName = default;
        magazineCapacity = default;
        shotsPerSecond = reloadTime = effectiveRange = damage = default;
        shootable = -1;
    }
    #endregion

    private void Update()
    {
        Checks();
        if (Time.time >= nextTimeToFire)
        {
            if (weaponPreset.automatic)
            {
                if (inputManager.automaticFire > 0f)
                {
                    if(canShoot && !isReloading)
                        animator.SetTrigger("Fire");
                    if(bulletsInMagazine <= 0 && !isReloading && !canShoot)
                        animator.SetTrigger("Fire");
                }
            }
            else if (!weaponPreset.automatic)
            {
                if (inputManager.fire)
                {
                    if (canShoot && !isReloading)
                        animator.SetTrigger("Fire");
                    if (bulletsInMagazine <= 0 && !isReloading && !canShoot)
                        animator.SetTrigger("Fire");
                }
            }
        }
    }
    private void Checks()
    {
        canShoot = bulletsInMagazine >= 1 ? true : false;

        animator.SetBool("Dry", !canShoot);
        animator.SetBool("isReloading", isReloading);

        if (inputManager.reload)
            StartCoroutine(Reload());
    }

    /*SHOOTING*/
    #region
    public void Shoot()
    {
        
        Ray shootRay = new Ray(shootingPosition.position, shootingPosition.forward);

        //RAYCASTING
        Physics.Raycast(shootRay, out weaponHit, effectiveRange, shootable);
        nextTimeToFire = Time.time + shotsPerSecond;

        WeaponFired();
        Damage();
        bulletsInMagazine--;
    }
    
    private void Damage()
    {
        if (!weaponHit.collider)
            return;
        else
        {
            if (weaponHit.collider.CompareTag("Player"))
            {
                //weaponHit.collider.GetComponent<Health>().TakeDamage(damage);
                ImpactEffect();
            }
            else
            {
                ImpactEffect();
                if(weaponHit.collider.TryGetComponent<Rigidbody>(out var rb))
                {
                    float rng = Random.Range(0.8f, 1.5f);
                    Vector3 crossProduct = transform.position - rb.transform.position;
                    crossProduct = (-crossProduct * rng).normalized;

                    rb.AddForce(weaponPreset.knockback * crossProduct, ForceMode.Impulse);
                    rb.AddTorque(-crossProduct * 50, ForceMode.Impulse);
                }
            }
        }
    }
    #endregion

    /*RELOADING*/
    #region
    public IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        isReloading = false;

        int missingBulletsInMagazine = magazineCapacity - bulletsInMagazine;
        bulletsInMagazine += missingBulletsInMagazine;
    }
    #endregion

    /*EFFECTS AND SOUND*/
    #region

    private void WeaponFired()
    {
        //Instantiate random muzzleflash
        int i = Random.Range(0, weaponPreset.muzzleFlashes.Length);
        var muzzleFlashInstance = Instantiate(weaponPreset.muzzleFlashes[i], gunTip);
        Destroy(muzzleFlashInstance, 0.2f);

        //Play random sound
        int x = Random.Range(0, weaponPreset.gunShots.Length);
        weaponSoundSource.clip = weaponPreset.gunShots[x];
        weaponSoundSource.Play();
    }

    public void WeaponDry()
    {
        weaponSoundSource.clip = weaponPreset.gunEmpty;
        weaponSoundSource.Play();
    }

    private void ImpactEffect()
    {

        //Instantiate proper effect
        if (weaponHit.collider.CompareTag("Player"))
        {
            Instantiate(weaponPreset.bloodImpactEffect, weaponHit.point, Quaternion.LookRotation(weaponHit.normal));
        }
        else
        {
            Instantiate(weaponPreset.impactEffect, weaponHit.point, Quaternion.LookRotation(weaponHit.normal));
        }
    }
    #endregion

}