using System;
using System.Collections.Generic;
using UnityEngine;

public struct BallState
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 angVel;
    public float speed;
    public float radius;
    public float mass;
}

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager instance;

    public int ballCount;
    public int maxBallCount = 100;
    public GameObject ballsParent;

    public float gravity = 9.81f;
    public float airDensity = 1.225f;

    List<Ball> balls = new List<Ball>();
    List<StaticMeshCollider> colliders = new List<StaticMeshCollider>();

    BallState[] ballStates = new BallState[1000];

    void Awake()
    {
        // singleton pattern
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        ballsParent = new GameObject("Balls");
        ballsParent.transform.parent = gameObject.transform;
    }

    void FixedUpdate()
    {
        float dt = Time.deltaTime;
        ballCount = balls.Count;

        // copy ball parameters to readonly bodyStates array
        // this will make sure that reults are independent of the body update order
        if (ballStates.Length < balls.Count)
            ballStates = new BallState[balls.Count * 2];

        for (int i = 0; i < balls.Count; i++)
        {
            ballStates[i].pos    = balls[i].position;
            ballStates[i].vel    = balls[i].velocity;
            ballStates[i].angVel = balls[i].angularVelocity;
            ballStates[i].radius = balls[i].radius;
            ballStates[i].mass   = balls[i].mass;
            ballStates[i].speed  = balls[i].velocity.magnitude;
        }

        // update all bodies
        for (int i = 0; i < balls.Count; i++)
        {
            if (!balls[i].gameObject.activeSelf)
                continue;

            var stateMe = ballStates[i];
            float timeTillColl = float.MaxValue;
            int collider = -1;
            bool colliderIsBall = false;
            Vector3 contactNormal = Vector3.zero;
            Vector3 contactPoint = Vector3.zero;

            // brute-force collision check against all static collision objects in the scene
            for (int j = 0; j < colliders.Count; j++)
            {
                if (colliders[j].SphereCollision(stateMe.pos, stateMe.radius, stateMe.vel, stateMe.speed * dt, out float collTime, out Vector3 point, out Vector3 normal))
                {
                    if (collTime <= dt &&
                        collTime >= 0 &&
                        collTime < timeTillColl)
                    {
                        timeTillColl = collTime;
                        collider = j;
                        colliderIsBall = false;
                        contactPoint = point;
                        contactNormal = normal;
                    }
                }
            }

            // brute-force collision check against all other bodies in the scene
            for (int j = 0; j < balls.Count; j++)
            {
                if (balls[i] == null || !balls[i].gameObject.activeSelf)
                    continue;

                var stateCollider = ballStates[j];
                if (SphereVsSphere.TestCollision(stateMe.pos, stateMe.vel, stateMe.radius, stateCollider.pos, stateCollider.vel, stateCollider.radius, out float collTime, out Vector3 point, out Vector3 normal))
                {
                    if (collTime <= dt &&
                        collTime >= 0 &&
                        collTime < timeTillColl)
                    {
                        timeTillColl = collTime;
                        collider = j;
                        colliderIsBall = true;
                        contactPoint = point;
                        contactNormal = normal;
                    }
                }
            }

            // collision response
            if (collider >= 0)
            {
                // collision happend 
                if (colliderIsBall)
                    balls[i].BounceOffOtherBall(ballStates[collider], contactPoint, contactNormal, timeTillColl);
                else
                    balls[i].BounceOffStaticSurface(contactPoint, contactNormal, timeTillColl);     
            }
            else
            {
                // no collision => move body normally
                balls[i].UpdateMotion(this, dt);
            }
        }

        // remove bodies when out of bounds or too many
        for (int i = 0; i < balls.Count; i++)
        {
            if (balls.Count > maxBallCount || 
                balls[i].position.sqrMagnitude > 10000)
            {
                Destroy(balls[i].gameObject);
                balls.RemoveAt(i);
                i--;
            }
        }
    }


    public void RegisterBall(Ball ball)
    {
        balls.Add(ball);
    }

    
    public void RegisterStaticMeshCollider(StaticMeshCollider collider)
    {
        colliders.Add(collider);
    }
}
