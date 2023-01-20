using Godot;
using System;

public partial class Bullet : Area3D
{
    [Export]
    public float bulletSpeed = 10f;

    private Vector3 velocity = Vector3.Zero;
    private double liveTime = 0;

    public override void _Ready()
    {
        Connect("body_entered", new(this, "body_entered"));
    }

	public override void _PhysicsProcess(double delta)
	{
        velocity = Visible ? -Transform.basis.z * bulletSpeed : Vector3.Zero;

        var transform = Transform;
        transform.origin += velocity * (float)delta;
        Transform = transform;

        if (!Visible)
        {
            Position = new(0, -999999, 0);
            liveTime = 0;
        }
        else
        {
            liveTime += delta;
        }

        if (liveTime > 10)
        {
            Visible = false;
        }
	}

    public void body_entered(Node3D body)
    {
        Visible = false;
    }
}
