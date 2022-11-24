using System;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public GameObject ballPrefab;

    public GameObject parent;
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    [Range(1, 20)]     public float fireRate = 10;
    [Range(0.03f, 1)]  public float radius = 0.3f;
    [Range(1, 100)]    public float density = 20;
    [Range(3, 150)]    public float speed = 20;
    [Range(-100, 100)] public float topSpin;
    [Range(-100, 100)] public float sideSpin;
    [Range(0, 1)]      public float elasticity = 0.5f; // 0..1
    [Range(0, 5)]      public float airDrag = 0.47f; // drag coeff for sphere
    [Range(0, 2)]      public float rotAirDrag = 1f;
    [Range(0, 1)]      public float magnusLift = 0.4f;
    [Range(0, 1)]      public float surfaceFriction = 0.5f;

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
            float minFireInterval = (radius * 4f) / speed;
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
            gameObj.transform.localScale = new Vector3(radius, radius, radius) * 2;

            var ball = gameObj.GetComponent<Ball>();
            ball.velocity = -transform.up * speed;
            ball.angularVelocity = Quaternion.LookRotation(-transform.up, Vector3.up) * new Vector3(topSpin, sideSpin, 0) * 360 * Mathf.Deg2Rad; // spin is in rotations per second
            ball.radius = radius;
            ball.mass = ball.volume * density;
            ball.elasticity = elasticity;
            ball.airDrag = airDrag;
            ball.airDragRotational = rotAirDrag;
            ball.magnusLift = magnusLift;
            ball.surfaceFriction = surfaceFriction;
        }

        // position and rotate the launcher relative to the parent object 
        if (parent != null)
        {
            Matrix4x4 parentMat = parent.transform.localToWorldMatrix;
            float caliber = Mathf.Sqrt(radius) * 2.5f;
            Vector3 targetPos = parentMat.MultiplyPoint(offsetPosition);

            transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Min(0.5f, Time.deltaTime * 30));
            transform.rotation = parent.transform.rotation * Quaternion.Euler(offsetRotation);
            transform.localScale = new Vector3(caliber, transform.localScale.y, caliber);
        }

        // show/hide UI panel for configuring ball parameters
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            var uiDoc = GetComponent<UnityEngine.UIElements.UIDocument>();
            var uiScript = GetComponent<BallLauncherUI>();

            if (uiDoc != null && uiScript != null)
            {
                if (uiDoc.enabled)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    uiDoc.enabled = false;
                    uiScript.enabled = false;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    uiDoc.enabled = true;
                    uiScript.enabled = true;
                }
            }
        }
    }
}
