using System;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public GameObject ballPrefab;

    public GameObject parent;
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    [Range(1, 100)]    public float fireRate = 10;
    [Range(0.02f, 1)]  public float ballRadius = 0.3f;
    [Range(1, 100)]    public float ballDensity = 20;
    [Range(5, 100)]    public float ballSpeed = 20;
    [Range(-100, 100)] public float topSpin;
    [Range(-100, 100)] public float sideSpin;

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
            ball.transform.localScale = new Vector3(ballRadius, ballRadius, ballRadius) * 2;
            var body = ball.GetComponent<PhysicsBody>();
            body.velocity = -transform.up * ballSpeed;
            body.angularVelocity = new Vector3(topSpin, 0, sideSpin) * Mathf.Deg2Rad * 360; // spin is in rotations per second
            body.radius = ballRadius;
            body.mass = body.volume * ballDensity;
        }

        // position and rotate the launcher relative to the parent object 
        if (parent != null)
        {
            Matrix4x4 parentMat = parent.transform.localToWorldMatrix;
            float caliber = Mathf.Sqrt(ballRadius) * 2.5f;

            transform.position = parentMat.MultiplyPoint(offsetPosition);
            transform.rotation = parent.transform.rotation * Quaternion.Euler(offsetRotation);
            transform.localScale = new Vector3(caliber, transform.localScale.y, caliber);
        }
    }
}
