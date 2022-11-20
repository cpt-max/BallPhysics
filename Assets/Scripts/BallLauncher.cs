using System;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public GameObject ballPrefab;

    public float rotSpeed = 1;
    public float fireRate = 10;

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
        if (fire)
        {
            var ball = Instantiate(ballPrefab, transform);
            ball.transform.parent = transform.parent;
            var body = ball.GetComponent<PhysicsBody>();
            body.velocity = -transform.up * 10;
        }

        // aim the launcher
        Vector3 rot = new Vector3(
            -Input.GetAxis("Vertical") * 0.2f - Input.GetAxis("Mouse Y"),
            0,
            Input.GetAxis("Horizontal") * 0.2f + Input.GetAxis("Mouse X")
            );

        transform.Rotate(rot * rotSpeed);
    }
}
