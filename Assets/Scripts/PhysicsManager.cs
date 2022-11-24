using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public struct BallState
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 angVel;
    public float speed;
    public float radius;
    public float mass;
    public float inertia;
    public float elasticity;
    public float surfaceFriction;
}

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager instance;

    public int ballCount;
    public int sleepingBallCount;
    public int maxBallCount = 100;

    public float gravity = 9.81f;
    public float airDensity = 1.225f;

    public AnimationCurve rotDragCoefficient = new AnimationCurve();
    public AnimationCurve linDragCoefficient = new AnimationCurve();

    List<Ball> balls = new List<Ball>();
    List<StaticMeshCollider> colliders = new List<StaticMeshCollider>();

    BallState[] ballStates = new BallState[256];
    int[] ballInds1 = new int[256]; // 1st update phase
    int[] ballInds2 = new int[256]; // 2nd update phase
    int ballCount2ndPhase; 

    [HideInInspector]
    public GameObject ballsParent;

    public Stopwatch stopWatchTotal = new Stopwatch();
    public Stopwatch stopWatchMeshColl = new Stopwatch();
    public Stopwatch stopWatchSphereColl = new Stopwatch();

    float uiUpdateTimer;

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
        stopWatchTotal.Restart();
        stopWatchMeshColl.Reset();
        stopWatchSphereColl.Reset();

        ballCount = balls.Count;
        sleepingBallCount = 0;

        // copy ball parameters to readonly ballStates array.
        // this array represents the beginning ball states, before updates are applied.
        // this will make sure that reults are independent of the ball update order.
        if (ballStates.Length < balls.Count)
        {
            ballStates = new BallState[balls.Count * 2];
            ballInds1 = new int[balls.Count * 2];
            ballInds2 = new int[balls.Count * 2];
        }

        for (int i = 0; i < balls.Count; i++)
        {
            ballStates[i].pos = balls[i].position;
            ballStates[i].vel = balls[i].velocity;
            ballStates[i].angVel = balls[i].angularVelocity;
            ballStates[i].radius = balls[i].radius;
            ballStates[i].mass = balls[i].mass;
            ballStates[i].inertia = balls[i].inertia;
            ballStates[i].elasticity = balls[i].elasticity;
            ballStates[i].surfaceFriction = balls[i].surfaceFriction;
            ballStates[i].speed = balls[i].velocity.magnitude;

            ballInds1[i] = i;
            if (balls[i].isSleeping) 
                sleepingBallCount++;
        }

        // update balls in 2 phases
        ballCount2ndPhase = 0;

        UpdateBalls(ballInds1, balls.Count);
        UpdateBalls(ballInds2, ballCount2ndPhase);

        // remove bodies when out of bounds or too many
        for (int i = 0; i < balls.Count; i++)
        {
            if (balls.Count > maxBallCount ||
                balls[i].position.y < -100 ||
                balls[i].position.sqrMagnitude > 100000)
            {
                Destroy(balls[i].gameObject);
                balls.RemoveAt(i);
                i--;
            }
        }

        stopWatchTotal.Stop();
    }

    private void UpdateBalls(int[] ballInds, int count)
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            int ind = ballInds[i];
            Ball ball = balls[ind];

            if (ball.isSleeping ||
                !ball.gameObject.activeSelf)
                continue;

            var stateMe = ballStates[ind];
            float timeTillColl = float.MaxValue;
            int collider = -1;
            bool colliderIsBall = false;
            Vector3 contactNormal = Vector3.zero;
            Vector3 contactPoint = Vector3.zero;

            // brute-force collision check against all static collision objects in the scene
            stopWatchMeshColl.Start();

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

            stopWatchMeshColl.Stop();

            // brute-force collision check against all other bodies in the scene
            stopWatchSphereColl.Start();

            for (int j = 0; j < balls.Count; j++)
            {
                if (balls[j] == null || !balls[j].gameObject.activeSelf)
                    continue;

                var stateHim = ballStates[j];
                if (SphereVsSphere.TestCollision(stateMe.pos, stateMe.vel, stateMe.radius, stateHim.pos, stateHim.vel, stateHim.radius, out float collTime, out Vector3 point, out Vector3 normal))
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

            stopWatchSphereColl.Stop();

            // collision response
            if (collider >= 0)
            {
                // move normally till time of collision
                ball.position += stateMe.vel * timeTillColl;

                // collision happend 
                if (colliderIsBall)
                {
                    ball.BounceOffOtherBall(ballStates[collider], contactPoint, contactNormal, timeTillColl);

                    // wake up the other ball
                    if (balls[collider].isSleeping)
                    {
                        balls[collider].isSleeping = false;
                        balls[collider].sleepTimer = 0;

                        // make sure the awoken ball gets to update in the 2nd phase
                        if (i > collider && ballInds != ballInds2)
                        {
                            ballInds2[ballCount2ndPhase] = collider;
                            ballCount2ndPhase++;
                        }
                    }
                }
                else
                    ball.BounceOffStaticSurface(colliders[collider], contactPoint, contactNormal, timeTillColl);
            }
            else
            {
                // no collision => move body normally
                ball.UpdateMotion(this, dt);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            for (int i = 0; i < balls.Count; i++)
                Destroy(balls[i].gameObject);

            balls.Clear();
        }

        if (Time.time > uiUpdateTimer + 0.2f)
        {
            uiUpdateTimer = Time.time;

            var text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = string.Format(
@"Press Space to Change Ball Parameters
Press Tab to Clear Balls

Ball Count: {0}
Sleeping Balls: {1}

Render Frame: {2:0.0}ms ({3:0} fps)
Physics Step: {4:0.0}ms ({5:0} fps)

Physics Total: {6:0.0}ms
Collisions Total: {7:0.0}ms
Mesh Collisions: {8:0.0}ms
Sphere Collisions: {9:0.0}ms
",
                ballCount, sleepingBallCount,
                Time.deltaTime * 1000, 1 / Time.deltaTime,
                Time.fixedDeltaTime * 1000, 1 / Time.fixedDeltaTime,
                stopWatchTotal.Elapsed.TotalMilliseconds,
                stopWatchMeshColl.Elapsed.TotalMilliseconds + stopWatchSphereColl.Elapsed.TotalMilliseconds,
                stopWatchMeshColl.Elapsed.TotalMilliseconds,
                stopWatchSphereColl.Elapsed.TotalMilliseconds);
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
