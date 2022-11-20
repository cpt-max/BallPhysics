// sphere-triangle collision code
// ported from https://www.flipcode.com/archives/Moving_Sphere_VS_Triangle_Collision.shtml

using System;
using UnityEngine;

public static class SphereVsTriangle
{
	public static bool TestCollision(Sphere sphere, Vector3 sphereVel, Triangle tri, out float distTravel, out Vector3 collNormal)
	{
		distTravel = float.MaxValue;
		collNormal = Vector3.zero;

		int i;
		Vector3 nvelo = Vector3.Normalize(sphereVel);

		if (Vector3.Dot(tri.normal, nvelo) > -0.001f)
			return false;

		int col = -1;

		Plane plane = Plane.FromPointAndNormal(tri.p1, tri.normal);

		// pass1: sphere VS plane
		float h = plane.Dist(sphere.center);
		if (h < -sphere.radius)
			return false;

		if (h > sphere.radius) {
			h -= sphere.radius;
			float dot = Vector3.Dot(tri.normal, nvelo);
			if (dot != 0) {
				float t = -h / dot;
				Vector3 onPlane = sphere.center + nvelo * t;
				if (IsPointInsideTriangle(tri.p1, tri.p2, tri.p3, onPlane)) {
					if (t < distTravel) {
						distTravel = t;
						collNormal = tri.normal;
						col = 0;
					}
				}
			}
		}

		// pass2: sphere VS triangle vertices
		for (i = 0; i < 3; i++)
		{
			Vector3 seg_pt0 = tri.Point(i);
			Vector3 seg_pt1 = seg_pt0 - nvelo;
			Vector3 v = seg_pt1 - seg_pt0;

			bool res = TestIntersectionSphereLine(sphere, seg_pt0, seg_pt1, out int nbInter, out float inter1, out float inter2);
			if (res == false)
				continue;

			float t = inter1;
			if (inter2 < t)
				t = inter2;

			if (t < 0)
				continue;

			if (t < distTravel)
			{
				distTravel = t;
				Vector3 onSphere = seg_pt0 + v * t;
				collNormal = sphere.center - onSphere;
				col = 1;
			}
		}

		// pass3: sphere VS triangle edges
		for (i = 0; i < 3; i++)
		{
			Vector3 edge0 = tri.Point(i);
			int j = i + 1;
			if (j == 3)
				j = 0;
			Vector3 edge1 = tri.Point(j);

			plane = Plane.FromPoints(edge0, edge1, edge1 - nvelo);
			float d = plane.Dist(sphere.center);
			if (d > sphere.radius || d < -sphere.radius)
				continue;

			float srr = sphere.radius * sphere.radius;
			float r = Mathf.Sqrt(srr - d * d);

			Vector3 pt0 = plane.Project(sphere.center); // center of the sphere slice (a circle)

			h = DistancePointToLine(pt0, edge0, edge1, out Vector3 onLine);
			Vector3 v = (onLine - pt0).normalized;
			Vector3 pt1 = v * r + pt0; // point on the sphere that will maybe collide with the edge

			int a0 = 0, a1 = 1;
			float pl_x = Mathf.Abs(plane.a);
			float pl_y = Mathf.Abs(plane.b);
			float pl_z = Mathf.Abs(plane.c);
			if (pl_x > pl_y && pl_x > pl_z)
			{
				a0 = 1;
				a1 = 2;
			}
			else
			{
				if (pl_y > pl_z)
				{
					a0 = 0;
					a1 = 2;
				}
			}

			Vector3 vv = pt1 + nvelo;

			bool res = TestIntersectionLineLine(new Vector2(pt1[a0], pt1[a1]),
												new Vector2(vv[a0], vv[a1]),
												new Vector2(edge0[a0], edge0[a1]),
												new Vector2(edge1[a0], edge1[a1]),
												out float t);
			if (!res || t < 0)
				continue;

			Vector3 inter = pt1 + nvelo * t;

			Vector3 r1 = edge0 - inter;
			Vector3 r2 = edge1 - inter;
			if (Vector3.Dot(r1, r2) > 0)
				continue;

			if (t > distTravel)
				continue;

			distTravel = t;
			collNormal = sphere.center - pt1;
			col = 2;
		}

		if (col != -1)
			collNormal.Normalize();

		return col == -1 ? false : true;
	}


	static bool TestIntersectionLineLine(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out float t)
	{
		t = 0;

		Vector2 d1 = p2 - p1;
		Vector2 d2 = p3 - p4;

		float denom = d2.y * d1.x - d2.x * d1.y;
		if (denom == 0)
			return false;

		float dist = d2.x * (p1.y - p3.y) - d2.y * (p1.x - p3.x);
		dist /= denom;
		t = dist;

		return true;
	}

	static bool IsPointInsideTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 pt)
	{
		Vector3 u = vertex1 - vertex0;
		Vector3 v = vertex2 - vertex0;
		Vector3 w = pt - vertex0;

		float uu = Vector3.Dot(u, u);
		float uv = Vector3.Dot(u, v);
		float vv = Vector3.Dot(v, v);
		float wu = Vector3.Dot(w, u);
		float wv = Vector3.Dot(w, v);
		float d = uv * uv - uu * vv;

		float invD = 1 / d;
		float s = (uv * wv - vv * wu) * invD;
		if (s < 0 || s > 1)
			return false;
		float t = (uv * wu - uu * wv) * invD;
		if (t < 0 || (s + t) > 1)
			return false;

		return true;
	}

	static float Square(float val)
	{
		return val * val;
	}

	static bool TestIntersectionSphereLine(Sphere sphere, Vector3 pt0, Vector3 pt1, out int nbInter, out float inter1, out float inter2)
	{
		inter1 = float.MaxValue;
		inter2 = float.MaxValue;
		nbInter = 0;

		float a, b, c, i;

		a = Square(pt1.x - pt0.x) + Square(pt1.y - pt0.y) + Square(pt1.z - pt0.z);
		b = 2 * ((pt1.x - pt0.x) * (pt0.x - sphere.center.x)
			+ (pt1.y - pt0.y) * (pt0.y - sphere.center.y)
			+ (pt1.z - pt0.z) * (pt0.z - sphere.center.z));
		c = Square(sphere.center.x) + Square(sphere.center.y) +
			Square(sphere.center.z) + Square(pt0.x) +
			Square(pt0.y) + Square(pt0.z) -
			2 * (sphere.center.x * pt0.x + sphere.center.y * pt0.y + sphere.center.z * pt0.z) - Square(sphere.radius);
		i = b * b - 4 * a * c;

		if (i < 0)
			return false;

		if (i == 0) {
			nbInter = 1;
			inter1 = -b / (2 * a);
		}
		else
		{
			nbInter = 2;
			inter1 = (-b + Mathf.Sqrt(Square(b) - 4 * a * c)) / (2 * a);
			inter2 = (-b - Mathf.Sqrt(Square(b) - 4 * a * c)) / (2 * a);
		}

		return true;
	}

	static float SqrDistancePointToLine(Vector3 point, Vector3 pt0, Vector3 pt1, out Vector3 linePt)
	{
		Vector3 v = point - pt0;
		Vector3 s = pt1 - pt0;
		float lenSq = s.sqrMagnitude;
		float dot = Vector3.Dot(v, s) / lenSq;
		Vector3 disp = s * dot;
		linePt = pt0 + disp;
		v -= disp;
		return v.sqrMagnitude;
	}

	static float DistancePointToLine(Vector3 point, Vector3 pt0, Vector3 pt1, out Vector3 linePt)
	{
		return Mathf.Sqrt(SqrDistancePointToLine(point, pt0, pt1, out linePt));
	}
}
