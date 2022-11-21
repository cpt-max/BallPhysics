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
    [Range(0, 1)]      public float ballElasticity = 0.5f; // 0..1
    [Range(0, 5)]      public float ballDragTranslational = 0.47f; // drag coeff for sphere
    [Range(0, 100)]    public float ballDragRotational = 1f;
    [Range(0, 1)]      public float ballMagnusLift = 0.4f;
    [Range(0, 5)]      public float ballSurfaceFriction = 2f;

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
            // limit fire interval so there is a minimum distance btw individual balls
            float fireInterval = 1 / fireRate;
            float minFireInterval = (ballRadius * 4f) / ballSpeed;
            fireInterval = Mathf.Max(fireInterval, minFireInterval);

            autoFireTimer += Time.deltaTime;
            if (autoFireTimer > fireInterval)
            {
                autoFireTimer -= fireInterval;
                fire = true;
            }
        }

        // fire!!!
        if (fire && ballPrefab != null)
        {
            var gameObj = Instantiate(ballPrefab, transform);
            gameObj.transform.parent = PhysicsManager.instance.ballsParent.transform;
            gameObj.transform.localScale = new Vector3(ballRadius, ballRadius, ballRadius) * 2;

            var ball = gameObj.GetComponent<Ball>();
            ball.velocity = -transform.up * ballSpeed;
            ball.angularVelocity = Quaternion.LookRotation(-transform.up, Vector3.up) * new Vector3(topSpin, sideSpin, 0) * 360 * Mathf.Deg2Rad; // spin is in rotations per second
            ball.radius = ballRadius;
            ball.mass = ball.volume * ballDensity;
            ball.elasticity = ballElasticity;
            ball.airDragTranslational = ballDragTranslational;
            ball.airDragRotational = ballDragRotational;
            ball.magnusLift = ballMagnusLift;
            ball.surfaceFriction = ballSurfaceFriction;
        }

        // position and rotate the launcher relative to the parent object 
        if (parent != null)
        {
            Matrix4x4 parentMat = parent.transform.localToWorldMatrix;
            float caliber = Mathf.Sqrt(ballRadius) * 2.5f;
            Vector3 targetPos = parentMat.MultiplyPoint(offsetPosition);

            transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Min(0.5f, Time.deltaTime * 30));
            transform.rotation = parent.transform.rotation * Quaternion.Euler(offsetRotation);
            transform.localScale = new Vector3(caliber, transform.localScale.y, caliber);
        }
    }
}
