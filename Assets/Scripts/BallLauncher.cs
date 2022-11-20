using System;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public GameObject ballPrefab;

    public GameObject parent;
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    public float rotSpeed = 1;
    public float fireRate = 10;
    public float ballSpeed = 20;

    float autoFireTimer;

    void Update()
    {
        // single shot when pressing the fire button
        bool fire = Input.GetButtonDown("Fire1");
        if (fire)
            autoFireTimer = 0;

        // autofire when holding the fire button
        if (Input.GetButton("Fire1"))
        {
            autoFireTimer += Time.deltaTime;
            if (autoFireTimer > 1 / fireRate)
            {
                autoFireTimer -= 1 / fireRate;
                fire = true;
            }
        }

        // fire!!!
        if (fire && ballPrefab != null)
        {
            var ball = Instantiate(ballPrefab, transform);
            ball.transform.parent = transform.parent;
            var body = ball.GetComponent<PhysicsBody>();
            body.velocity = -transform.up * ballSpeed;
        }

        // position and rotate the launcher relative to the parent object 
        if (parent != null)
        {
            Matrix4x4 parentMat = parent.transform.localToWorldMatrix;
            transform.position = parentMat.MultiplyPoint(offsetPosition);
            transform.rotation = parent.transform.rotation * Quaternion.Euler(offsetRotation);
        }
    }
}
