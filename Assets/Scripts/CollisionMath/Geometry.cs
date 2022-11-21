using System;
using UnityEngine;

public struct Triangle
{
	public Vector3 p1;
	public Vector3 p2;
	public Vector3 p3;

	public Vector3 normal;

	public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
		this.p1 = p1;
		this.p2 = p2;
		this.p3 = p3;

		normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
	}

	public Vector3 Point(int index)
	{
		return index == 0 ? p1 :
			   index == 1 ? p2 :
							p3;
	}
}


public struct Plane
{
	public float a;
	public float b;
	public float c;
	public float d;

	public Plane(float a, float b, float c, float d)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}

	public static Plane FromPoints(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 v0 = p0 - p1;
		Vector3 v1 = p2 - p1;
		Vector3 n = Vector3.Cross(v1, v0).normalized;

		return new Plane(n.x, n.y, n.z, -(p0.x * n.x + p0.y * n.y + p0.z * n.z));
	}

	public static Plane FromPointAndNormal(Vector3 p, Vector3 n)
	{
		Vector3 nn = n.normalized;
		return new Plane(nn.x, nn.y, nn.z, -(p.x * nn.x + p.y * nn.y + p.z * nn.z));
	}

	public float Dot(Vector3 p)
	{
		return a * p.x + b * p.y + c * p.z;
	}

	public float Dist(Vector3 p)
	{
		return a * p.x + b * p.y + c * p.z + d;
	}

	public Vector3 Reflect(Vector3 vec)
	{
		float d = Dist(vec);
		return vec + 2 * new Vector3(-a, -b, -c) * d;
	}

	public Vector3 Project(Vector3 p)
	{
		float h = Dist(p);
		return new Vector3(p.x - a * h,
						   p.y - b * h,
						   p.z - c * h);
	}

	public bool IsOnPlane(Vector3 p, float threshold = 0.001f)
	{
		float d = Dist(p);
		if (d < threshold && d > -threshold)
			return true;
		return false;
	}

	// Calculate the intersection between this plane and a line
	// If the plane and the line are parallel, false is returned
	public bool IntersectWithLine(Vector3 p0, Vector3 p1, out float t)
	{
		t = 0;

		Vector3 dir = p1 - p0;
		float div = Dot(dir);
		if (div == 0)
			return false;

		t = -Dist(p0) / div;
		return true;
	}
}
