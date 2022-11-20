using System;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float radius = 0.5f;
    public float mass = 1;

    public Vector3 position
    {
        get => transform.position;
        set { transform.position = value; }
    }
    public Quaternion rotation
    {
        get => transform.rotation;
        set { transform.rotation = value; }
    }

    public float volume => radius * radius * radius * Mathf.PI * (4f / 3f);

    void Start()
    {
        PhysicsManager.instance.RegisterBody(this);
    }

    public void UpdateMotion(PhysicsManager physics, float dt)
    {
        float crossSectionalArea = radius * radius * Mathf.PI;
        float speedSqr = velocity.sqrMagnitude;
        float speed = Mathf.Sqrt(speedSqr);
        float angularSpeedSqr = angularVelocity.sqrMagnitude;
        float angularSpeed = Mathf.Sqrt(angularSpeedSqr);

        //============================================================================
        // move and rotate 
        //============================================================================
        position += velocity * dt;
        rotation *= Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * dt);

        //============================================================================
        // translational air drag 
        //============================================================================
        float dragCoefficient = 0.47f; // drag coeff for sphere
        float dragAccel = 0.5f * dragCoefficient * physics.airDensity * speedSqr * crossSectionalArea / mass;
        velocity -= velocity.normalized * dragAccel * dt;

        //============================================================================
        // rotational air drag 
        //============================================================================
        float rotDragMoment = physics.rotationalDamping * angularSpeedSqr * radius * radius * radius * radius * radius;
        float inertiaMoment = mass * radius * radius * (2f / 3f); // hollow sphere
        float rotDragAccel = rotDragMoment / inertiaMoment;
        angularVelocity -= angularVelocity.normalized * rotDragAccel * dt;
        
        //============================================================================
        // magnus effect (lift force from spinning): https://www.engineersedge.com/calculators/magnus_effect_calculator_15766.htm
        //============================================================================
        float slipFactor = radius * angularVelocity.magnitude / speed;
        float liftCoefficient = slipFactor * physics.magnusLift; // linear approximation of what should be a curve from experimental data
        float forceMagnus = 0.5f * liftCoefficient * physics.airDensity * crossSectionalArea * speedSqr;
        Vector3 dirMagnus = Vector3.Cross(angularVelocity, velocity).normalized;
        velocity += dirMagnus * forceMagnus * dt / mass;

        //============================================================================
        // gravity
        //============================================================================
        velocity.y -= physics.gravity * dt;
    }
}
