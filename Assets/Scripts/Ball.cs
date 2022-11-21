using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float radius = 0.5f;
    public float mass = 1;
    public float elasticity = 0.5f; // 0..1
    public float airDragTranslational = 0.47f; // drag coeff for sphere
    public float airDragRotational = 1f;
    public float magnusLift = 0.4f;
    public float surfaceFriction = 2f;

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
    public float momentOfInertia => mass * radius * radius * (2f / 3f); // hollow sphere

    void Start()
    {
        PhysicsManager.instance.RegisterBall(this);
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
        rotation = Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * dt) * rotation;

        //============================================================================
        // translational air drag 
        //============================================================================
        float dragAccel = 0.5f * airDragTranslational * physics.airDensity * speedSqr * crossSectionalArea / mass;
        velocity -= velocity.normalized * dragAccel * dt;

        //============================================================================
        // rotational air drag 
        // this is a crude approximation using a constant rotational air drag coefficient, which would vary a lot in the real world
        //============================================================================
        float rotDragMoment = airDragRotational * angularSpeedSqr * radius * radius * radius * radius * radius;

        rotDragMoment *= Mathf.Lerp(3,  1, angularSpeed / 30); // add extra drag for low speeds
        rotDragMoment *= Mathf.Lerp(5,  1, angularSpeed / 10); // add extra drag for very low speeds
        rotDragMoment *= Mathf.Lerp(10, 1, angularSpeed /  3); // add extra drag for very very low speeds

        float rotDragAccel = rotDragMoment / momentOfInertia;
        angularVelocity -= angularVelocity.normalized * rotDragAccel * dt;
        
        //============================================================================
        // magnus effect (lift force from spinning)
        // https://www.engineersedge.com/calculators/magnus_effect_calculator_15766.htm
        //============================================================================
        float slipFactor = radius * angularVelocity.magnitude / speed;
        float liftCoefficient = slipFactor * magnusLift; // linear approximation of what should be a curve from experimental data
        float forceMagnus = 0.5f * liftCoefficient * physics.airDensity * crossSectionalArea * speedSqr;
        Vector3 dirMagnus = Vector3.Cross(angularVelocity, velocity).normalized;
        velocity += forceMagnus * dt * dirMagnus / mass;

        //============================================================================
        // gravity
        //============================================================================
        velocity.y -= physics.gravity * dt;
    }

    public void BounceOffStaticSurface(Vector3 contactPoint, Vector3 contactNormal, float timeTillColl)
    {
        Vector3 posAtColl = position + velocity * timeTillColl;
        Vector3 deltaAngVel = AngVelDeltaFromSurfaceFriction(contactPoint, contactNormal, posAtColl, Vector3.zero, Vector3.zero, Vector3.zero);

        position = posAtColl;
        velocity = Vector3.Reflect(velocity, contactNormal) * (0.5f + 0.5f * elasticity);
        angularVelocity += deltaAngVel;
    }

    public void BounceOffOtherBall(in BallState otherBall, Vector3 contactPoint, Vector3 contactNormal, float timeTillColl)
    {
        Vector3 posAtColl = position + velocity * timeTillColl;
        Vector3 posAtCollHim = otherBall.pos + otherBall.vel * timeTillColl;
        Vector3 deltaAngVel = AngVelDeltaFromSurfaceFriction(contactPoint, contactNormal, posAtColl, posAtCollHim, otherBall.vel, otherBall.angVel);

        float impactSpeed = Vector3.Dot(velocity - otherBall.vel, contactNormal);
        float effectiveMass = 1 / (1 / mass + 1 / otherBall.mass);
        float impulse = effectiveMass * impactSpeed * (1 + elasticity);

        position = posAtColl;
        velocity -= contactNormal * impulse / mass;
        angularVelocity += deltaAngVel;
    }

    private Vector3 AngVelDeltaFromSurfaceFriction(Vector3 contactPoint, Vector3 contactNormal, Vector3 posAtCollMe, Vector3 posAtCollHim, Vector3 velocityHim, Vector3 angularVelocityHim)
    {
        // quickly made up => needs improvement
        // also angular momentum needs to be converted to linear momentum on impact
        Vector3 contactVecMe = contactPoint - posAtCollMe;
        Vector3 contactVecHim = contactPoint - posAtCollHim;

        Vector3 contactPointVelMe  = velocity    + Vector3.Cross(angularVelocity,  contactVecMe);
        Vector3 contactPointVelHim = velocityHim + Vector3.Cross(angularVelocityHim, contactVecHim);
        Vector3 contactPointVelDelta = contactPointVelMe - contactPointVelHim;

        float tangentialSpeed = contactPointVelDelta.magnitude;
        float impactSpeed = Mathf.Max(0, Vector3.Dot(velocity, -contactNormal));
        float friction = tangentialSpeed * impactSpeed * mass;
        Vector3 torque = Vector3.Cross(contactVecMe, friction * -contactPointVelDelta.normalized);
        float contactTime = 0.01f;
        Vector3 deltaAngVel = surfaceFriction * contactTime * torque / momentOfInertia;
        return deltaAngVel;
    }
}
