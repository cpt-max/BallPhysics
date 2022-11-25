# Ball Physics demo

Simple C# ball physics demo in Unity, without using a physics engine or Unity colliders.<br>
Balls collide with other balls, as well as static triangle meshes.<br>
Performance ain't great, due to lack of space partitioning for objects or mesh triangles. It's brute force collision checks for now.

Simulated ball properties:
- radius
- mass 
- elasticity 
- air drag (linear and rotational)
- magnus effect (lift on spinning ball)
- surface friction

<br>

Here's a [published Windows executeable](https://www.dropbox.com/s/jr0yzg6xzp11te0/BallPhysics.zip?dl=1).

![BallPhysics](https://user-images.githubusercontent.com/10579300/203954176-3fdeec21-ab7d-4745-bdb6-51f98e217553.jpg)
