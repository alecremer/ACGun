using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACGun
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        public delegate void OnCollisionEnterDelegate(Vector3 p);

        public event OnCollisionEnterDelegate OnCollisionEnterEventHandler;

        public float updatePositionTimeRate = 0, damage;
        /// <summary>
        /// t, velocity
        /// </summary>
        public Func<float, float, Vector3> bulletTrajectory;
        [HideInInspector]
        public float velocity;
        float t = 0;
        Rigidbody2D rb;
        public AudioSource audioSourcePrefab;
        AudioSource audioSource;
        public AudioClip bulletSound, collisionSound;
        [HideInInspector]
        public float rotationAngle;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            audioSource.clip = collisionSound;
            audioSource.Play();

            if (OnCollisionEnterEventHandler != null) OnCollisionEnterEventHandler(transform.position);
            Destroy(gameObject);
            //Damage calculator
        }

        Vector3 Rotate(Vector2 v)
        {
            float[][] rotationMatrix = new float[][] {
            new float[] { Mathf.Cos(rotationAngle), Mathf.Sin(rotationAngle)},
            new float[] { -Mathf.Sin(rotationAngle), Mathf.Cos(rotationAngle)}
        };

            float[] coordinatesBuffer = new float[] { 0, 0 };
            float[] u = new float[] { v.x, v.y };

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    coordinatesBuffer[i] += rotationMatrix[i][j] * u[j];
                }
            }
            return new Vector3(coordinatesBuffer[0], coordinatesBuffer[1], 0);
        }

        IEnumerator Wait(float waitTime)
        {
            rb.velocity = Rotate(bulletTrajectory(t, velocity));
            yield return new WaitForSeconds(waitTime);
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = Instantiate(audioSourcePrefab);
            audioSource.clip = bulletSound;
            audioSource.Play();
            Debug.Log(rotationAngle);
            rotationAngle = Mathf.Deg2Rad * rotationAngle;

        }

        void FixedUpdate()
        {
            t += Time.deltaTime;
            StartCoroutine(Wait(updatePositionTimeRate));
        }
    }

}