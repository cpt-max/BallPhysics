﻿using System;
using System.Collections.Generic;
using UnityEngine;

public struct BodyState
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

    public int bodyCount;

    public float gravity = 9.81f;
    public float airDensity = 1.225f;
    public float rotationalDamping = 1f;
    public float magnusLift = 0.4f;

    List<PhysicsBody> bodies = new List<PhysicsBody>();
    List<StaticMeshCollider> colliders = new List<StaticMeshCollider>();

    BodyState[] bodyStates = new BodyState[1000];

    void Awake()
    {
        // singleton pattern
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    void FixedUpdate()
    {
        float dt = Time.deltaTime;
        bodyCount = bodies.Count;

        // copy body parameters to readonly bodyStates array
        if (bodyStates.Length < bodies.Count)
            bodyStates = new BodyState[bodies.Count * 2];

        for (int i = 0; i < bodies.Count; i++)
        {
            bodyStates[i].pos    = bodies[i].position;
            bodyStates[i].vel    = bodies[i].velocity;
            bodyStates[i].angVel = bodies[i].angularVelocity;
            bodyStates[i].radius = bodies[i].radius;
            bodyStates[i].mass   = bodies[i].mass;
            bodyStates[i].speed  = bodies[i].velocity.magnitude;
        }

        // update all bodies
        //System.Threading.Tasks.Parallel.For(0, bodies.Count - 1, i =>
        for (int i = 0; i < bodies.Count; i++)
        {
            if (!bodies[i].gameObject.activeSelf)
                continue;

            var stateMe = bodyStates[i];
            float minCollTime = float.MaxValue;
            int collider = -1;
            Vector3 surfaceNormal = Vector3.zero;

            // brute-force collision check against all colliders in the scene
            for (int j = 0; j < colliders.Count; j++)
            {
                // early out if we are well outside the bounding volume
                float maxMoveRadius = stateMe.radius + stateMe.speed * dt;
                if (colliders[j].bounds != null &&
                    colliders[j].bounds.SqrDistance(stateMe.pos) > maxMoveRadius * maxMoveRadius + 0.01f)
                    continue;
                
                if (colliders[j].SphereCollision(stateMe.pos, stateMe.radius, stateMe.vel, out float timeTillColl, out Vector3 normal))
                {
                    if (timeTillColl >= 0 &&
                        timeTillColl <= dt &&
                        timeTillColl < minCollTime)
                    {
                        minCollTime = timeTillColl;
                        collider = j;
                        surfaceNormal = normal;
                    }
                }
            }

            // brute-force collision check against all other bodies in the scene
            for (int j = 0; j < bodies.Count; j++)
            {
                if (!bodies[i].gameObject.activeSelf)
                    continue;

                var stateCollider = bodyStates[j];
                if (SphereVsSphere.TestCollision(stateMe.pos, stateMe.vel, stateMe.radius, stateCollider.pos, stateCollider.vel, stateCollider.radius, out float timeTillColl))
                {
                    if (timeTillColl >= 0 &&
                        timeTillColl <= dt &&
                        timeTillColl < minCollTime)
                    {
                        minCollTime = timeTillColl;
                        collider = j;
                        Vector3 collPos = stateMe.pos + stateMe.vel * minCollTime;
                        surfaceNormal = Vector3.Normalize(collPos - stateCollider.pos);
                    }
                }
            }

            if (collider >= 0)
            {
                // collision happend => reflect the ball off the surface of the other ball
                Vector3 collPos = stateMe.pos + stateMe.vel * minCollTime;
                Vector3 newVel = Vector3.Reflect(stateMe.vel, surfaceNormal);

                bodies[i].position = collPos;
                bodies[i].velocity = newVel;
            }
            else
            {
                // no collision => move body normally
                bodies[i].UpdateMotion(this, dt);
            }

            // disable bodies when out of bounds
            if (bodies[i].position.sqrMagnitude > 10000)
                bodies[i].gameObject.SetActive(false);
        }
        //);
    }

    public void RegisterBody(PhysicsBody body)
    {
        bodies.Add(body);
    }

    
    public void RegisterStaticMeshCollider(StaticMeshCollider collider)
    {
        colliders.Add(collider);
    }
}
