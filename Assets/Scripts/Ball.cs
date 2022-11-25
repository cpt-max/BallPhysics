using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float radius = 0.5f;
    public float mass = 1;
    [Range(0.1f, 1)] public float elasticity = 0.7f; // 0..1
    [Range(0, 1)] public float surfaceFriction = 1f;
    public float airDrag = 0.47f; // drag coeff for sphere
    public float airDragRotational = 1f;
    public float magnusLift = 0.4f;

    Vector3 sleepPos;
    Quaternion sleepRot;
    public bool isSleeping;
    public float sleepTimer;
    const float sleepRadius = 0.1f;
    const float sleepAng = 5f;
    const float fallAsleepDuration = 1;

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
    public float inertia => mass * radius * radius * (2f / 3f); // moment of inertia for hollow sphere

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
        float linDragCoeff = physics.linDragCoefficient.length == 0 ? 1 : physics.linDragCoefficient.Evaluate(speed);
        float dragAccel = 0.5f * airDrag * linDragCoeff * physics.airDensity * speedSqr * crossSectionalArea / mass;
        float speedDelta = dragAccel * dt;
        speedDelta = Mathf.Clamp(speedDelta, -speed, +speed);
        velocity -= velocity.normalized * speedDelta;

        //============================================================================
        // rotational air drag 
        // this is a crude approximation using a constant rotational air drag coefficient, which would vary a lot in the real world
        //============================================================================
        float rotDragCoeff = physics.rotDragCoefficient.length == 0 ? 1 : physics.rotDragCoefficient.Evaluate(angularSpeed);
        float rotDragMoment = airDragRotational * rotDragCoeff * angularSpeedSqr * radius * radius * radius * radius * radius;
        float rotDragAccel = rotDragMoment / inertia;
        float rotSpeedDelta = rotDragAccel * dt;
        rotSpeedDelta = Mathf.Clamp(rotSpeedDelta, -angularSpeed, +angularSpeed);
        angularVelocity -= angularVelocity.normalized * rotSpeedDelta;

        //============================================================================
        // magnus effect (lift force from spinning)
        // https://www.engineersedge.com/calculators/magnus_effect_calculator_15766.htm
        //============================================================================
        if (speed > 0.001f)
        {
            float slipFactor = radius * angularVelocity.magnitude / speed;
            float liftCoefficient = slipFactor * magnusLift; // linear approximation of what should be a curve from experimental data
            float forceMagnus = 0.5f * liftCoefficient * physics.airDensity * crossSectionalArea * speedSqr;
            Vector3 dirMagnus = Vector3.Cross(angularVelocity, velocity).normalized;
            velocity += forceMagnus * dt * dirMagnus / mass;
        }

        //============================================================================
        // gravity
        //============================================================================
        velocity.y -= physics.gravity * dt;

        //============================================================================
        // sleep
        //============================================================================
        if ((position - sleepPos).sqrMagnitude > sleepRadius * sleepRadius ||
            Quaternion.Angle(rotation, sleepRot)  > sleepAng)
        {
            isSleeping = false;
            sleepTimer = 0;
            sleepPos = position;
            sleepRot = rotation;
        }
        else
        {
            sleepTimer += dt;
            if (sleepTimer > fallAsleepDuration)
            {
                isSleeping = true;
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
            }
        }
    }

    public void BounceOffStaticSurface(StaticMeshCollider collider, Vector3 contactPoint, Vector3 contactNormal)
    {
        float combinedElasticity = Mathf.Max(elasticity, collider.elasticity);
        float combinedFriction = Mathf.Min(surfaceFriction, collider.surfaceFriction) * 0.5f;

        // linear impuls transfer
        float linImpactSpeed = Vector3.Dot(velocity, contactNormal);
        float effectiveMass = (1 + combinedElasticity) / (1 / mass);
        Vector3 impulse = linImpactSpeed * effectiveMass * contactNormal;
        velocity -= impulse / mass;

        // angular momentum transfer
        Vector3 contactVecMe = contactPoint - position;
        Vector3 contactVelFromRotMe = Vector3.Cross(angularVelocity, contactVecMe);
        Vector3 deltaContactVel = velocity + contactVelFromRotMe;
        Vector3 rotImpulseNormal = -deltaContactVel.normalized;
        Vector3 contactCrossMe = Vector3.Cross(contactVecMe, rotImpulseNormal);
        float rotImpactSpeed = Vector3.Dot(velocity, rotImpulseNormal) + Vector3.Dot(angularVelocity, contactCrossMe);
        float effectiveRotMass = (1 + combinedElasticity) / (1 / mass + 
            Vector3.Dot(contactCrossMe, contactCrossMe) / inertia); // without scalar inertia: Vector3.Dot(contactCross, (InertiaTensor.Inverted * contactCross))
        Vector3 rotImpulse = rotImpactSpeed * effectiveRotMass * combinedFriction * rotImpulseNormal;
        angularVelocity += Vector3.Cross(rotImpulse, contactVecMe) / inertia;
    }

    public void BounceOffOtherBall(in BallState otherBall, Vector3 contactPoint, Vector3 contactNormal, float timeTillCollOtherBall)
    {
        float combinedElasticity = Mathf.Max(elasticity, otherBall.elasticity);
        float combinedFriction = Mathf.Min(surfaceFriction, otherBall.surfaceFriction) * 0.5f;

        Vector3 posHe = otherBall.pos + otherBall.vel * timeTillCollOtherBall;

        // linear impuls transfer
        Vector3 deltaVel = velocity - otherBall.vel;
        float linImpactSpeed = Vector3.Dot(deltaVel, contactNormal);
        float effectiveMass = (1 + combinedElasticity) / (1 / mass + 1 / otherBall.mass);
        Vector3 impulse = linImpactSpeed * effectiveMass * contactNormal;
        velocity -= impulse / mass;

        // angular momentum transfer
        Vector3 contactVecMe = contactPoint - position;
        Vector3 contactVecHe = contactPoint - posHe;

        Vector3 contactVelFromRotMe = Vector3.Cross(angularVelocity, contactVecMe);
        Vector3 contactVelFromRotHe = Vector3.Cross(otherBall.angVel, contactVecHe);
       
        Vector3 deltaContactVel = deltaVel + (contactVelFromRotMe - contactVelFromRotHe);
        Vector3 rotImpulseNormal = -deltaContactVel.normalized;

        Vector3 contactCrossMe = Vector3.Cross(contactVecMe, rotImpulseNormal);
        Vector3 contactCrossHe = Vector3.Cross(contactVecHe, rotImpulseNormal);

        float rotImpactSpeed = Vector3.Dot(deltaVel, rotImpulseNormal) + (Vector3.Dot(angularVelocity, contactCrossMe) - Vector3.Dot(otherBall.angVel, contactCrossHe));
        
        float effectiveRotMass = (1 + combinedElasticity) / (
            1 / mass + 1 / otherBall.mass + 
            Vector3.Dot(contactCrossMe, contactCrossMe) / inertia +     // without scalar inertia: Vector3.Dot(contactCross, (InertiaTensor.Inverted * contactCross))
            Vector3.Dot(contactCrossHe, contactCrossHe) / otherBall.inertia); // without scalar inertia: Vector3.Dot(contactCross, (InertiaTensor.Inverted * contactCross))
        Vector3 rotImpulse = rotImpactSpeed * effectiveRotMass * combinedFriction * rotImpulseNormal;

        angularVelocity += Vector3.Cross(rotImpulse, contactVecMe) / inertia;
    }
}
