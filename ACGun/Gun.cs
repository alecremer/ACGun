using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACGun
{
    public class Gun : MonoBehaviour
    {
        public float damage, firingRatePerSecond, reloadGunTime;

        public delegate void OnShoot();
        public delegate void OnReloadGun();
        public delegate void OnEmptyGunShoot();
        public delegate void OnReloadedGun();
        public delegate void OnCantShootCauseCondition();
        public delegate void OnBulletCollisionEnterDelegate(Vector3 p);

        public event OnBulletCollisionEnterDelegate OnBulletCollisionEnterEventHandler;
        public event OnShoot OnShootEvenHandler;
        public event OnReloadGun OnReloadGunEvenHandler;
        public event OnEmptyGunShoot OnEmptyGunShootEventHandler;
        public event OnReloadedGun OnReloadedGunEventHandler;
        public event OnCantShootCauseCondition OnCantShootCauseConditionEventHandler;

        public int ammo, maxAmmoInGun, maxAmmo, gunAmmo, decrementInShoot = 1;
        public bool canShoot = true, gunLoad = true, infiniteAmmo;
        public float bulletVelocity;
        public AudioClip shootSound, bulletSound, bulletCollisionSound;
        public AudioSource audioSource, bulletAudioSourcePrefab;
        public GameObject bullet;
        public Vector3 bulletInstantiatePoint;

        // [HideInInspector]
        public float rotationAngle = 0;

        public Func<bool> conditionToShoot;
        public Func<float> rotationAngleFunction;
        /// <summary>
        /// t, velocity
        /// </summary>
        public Func<float, float, Vector3> bulletTrajectory;

        public void SetAudioSource(AudioClip audio) => audioSource.clip = audio;

        public void Shoot()
        {
            bool fire = false;

            if (gunAmmo > 0 || infiniteAmmo)
            {
                if (conditionToShoot != null)
                {
                    if (!conditionToShoot())
                    {
                        if (OnCantShootCauseConditionEventHandler != null)
                            OnCantShootCauseConditionEventHandler();
                    }
                    fire = conditionToShoot();
                }
                else fire = true;
            }
            else if (!infiniteAmmo && gunAmmo == 0)
            {
                if (OnEmptyGunShootEventHandler != null) OnEmptyGunShootEventHandler();
            }
            if (fire && canShoot)
            {
                canShoot = false;
                InstantiateBullet();
                StartCoroutine(ReloadBullet(1f / firingRatePerSecond)); ;
            }
        }

        IEnumerator ReloadBullet(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            canShoot = true;
        }

        IEnumerator ReloadGun(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            int ammoBuffer;
            if (ammo > maxAmmoInGun) ammoBuffer = maxAmmoInGun;
            else ammoBuffer = ammo;
            gunAmmo = ammoBuffer;
            ammo -= ammoBuffer;
            canShoot = true;
            if (OnReloadedGunEventHandler != null) OnReloadedGunEventHandler();
        }

        public void ReloadGun()
        {
            if (!infiniteAmmo)
            {
                if (ammo > 0)
                {
                    canShoot = false;
                    if (OnReloadGunEvenHandler != null) OnReloadGunEvenHandler();
                    StartCoroutine(ReloadGun(reloadGunTime)); ;

                }
            }
        }
        void InstantiateBullet()
        {
            GameObject bulletBuffer = Instantiate(bullet);
            Bullet bulletComponent = bulletBuffer.GetComponent<Bullet>();
            bulletComponent.transform.SetParent(gameObject.transform);
            bulletBuffer.transform.localPosition = bulletInstantiatePoint;
            bulletBuffer.transform.SetParent(null);

            if (bulletTrajectory != null) bulletComponent.bulletTrajectory = bulletTrajectory;
            bulletComponent.velocity = bulletVelocity;
            bulletComponent.damage = damage;
            bulletComponent.OnCollisionEnterEventHandler += (p) => {
                if (OnBulletCollisionEnterEventHandler != null) OnBulletCollisionEnterEventHandler(p);
            };
            bulletComponent.bulletSound = bulletSound;
            bulletComponent.collisionSound = bulletCollisionSound;
            bulletComponent.audioSourcePrefab = bulletAudioSourcePrefab;
            if (rotationAngleFunction != null)
                bulletComponent.rotationAngle = rotationAngleFunction();
            else
                bulletComponent.rotationAngle = 0;

            if (!infiniteAmmo) gunAmmo -= decrementInShoot;
            if (OnShootEvenHandler != null) OnShootEvenHandler();
        }

        void Start()
        {
            if (shootSound != null)
            {
                audioSource.clip = shootSound;
                OnShootEvenHandler += () => audioSource.Play();
            }
            Debug.Log("RotationAngle: " + rotationAngle);

        }

        void FixedUpdate()
        {

        }
    }
}
