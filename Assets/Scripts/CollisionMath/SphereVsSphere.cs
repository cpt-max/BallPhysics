using System;
using UnityEngine;

public static class SphereVsSphere
{
    public static bool TestCollision(Vector3 pos1, Vector3 vel1, float rad1, Vector3 pos2, Vector3 vel2, float rad2, out float timeTillColl, out Vector3 contactPoint, out Vector3 contactNormal)
    {
        // Dervivation: for spheres to be in contact, the distance btw them has to be equal to the sum of theri radius:
        // |(pos1 + vel1 * t) - (pos2 + vel2 * t)| = rad1 + rad2
        timeTillColl = 0;
        contactNormal = Vector3.zero;
        contactPoint = Vector3.zero;

        Vector3 deltaPos = pos2 - pos1;
        Vector3 deltaVel = vel2 - vel1;

        float a = Vector3.Dot(deltaVel, deltaVel);
        float b = 2 * Vector3.Dot(deltaPos, deltaVel);
        float c = Vector3.Dot(deltaPos, deltaPos) - (rad1 + rad2) * (rad1 + rad2);  

        if (QuadraticEquation(a, b, c, out float t1, out float t2))
        {
            timeTillColl = Mathf.Min(t1, t2);
            Vector3 collPos1 = pos1 + vel1 * timeTillColl;
            Vector3 collPos2 = pos2 + vel2 * timeTillColl;
            contactNormal = Vector3.Normalize(collPos1 - collPos2);
            contactPoint = collPos1 - contactNormal * rad1;
            return true;
        }

        return false;
    }

    // returns t1 and t2, the two solutions to the quadratic equation:
    // a*t*t + b*t + c = 0
    static bool QuadraticEquation(float a, float b, float c, out float t1, out float t2)
    {
        t1 = 0;
        t2 = 0;

        float div = 2 * a;
        if (div == 0)
            return false;

        float sqr = b * b - 4 * a * c;
        if (sqr < 0)
            return false;

        float sqrt = Mathf.Sqrt(sqr);
        t1 = (-b + sqrt) / div;
        t2 = (-b - sqrt) / div;

        return true;
    }
}
