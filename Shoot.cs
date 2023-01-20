using Godot;
using System;
using System.Collections.Generic;

public partial class Shoot : Node3D
{
    [Export]
    float shotTime = 0.5f;
    PackedScene bulletResource;
    List<Node3D> bulletPool = new();
    Machine machine;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        bulletResource = GD.Load<PackedScene>("res://scenes/prefabs/bullet.tscn");
        machine = (Machine) this.FindParent("Machine");
        machine.Connect("doSHOOT", new(this, "ShootBullet"));
        for(int i = 0; i < 16; i++)
        {
            var bullet = (Node3D)bulletResource.Instantiate();
            bulletPool.Add(bullet);
            GetTree().GetRoot().CallDeferred("add_child", bullet);
            bullet.Visible = false;
        }
	}

    public override void _Process(double delta)
    {
        lastShot += (float)delta;
    }

    private Node3D getBullet()
    {
        foreach (Node3D bullet in bulletPool)
        {
            if (!bullet.Visible)
            {
                return bullet;
            }
        }

        var newBullet = (Node3D)bulletResource.Instantiate();
        bulletPool.Add(newBullet);
        GetTree().GetRoot().CallDeferred("add_child", newBullet);
        return newBullet;
    }

    float lastShot = 0.0f;
    public void ShootBullet()
    {
        if (lastShot < shotTime)
            return;

        Node3D bullet = getBullet();
        bullet.Position = GlobalPosition;
        bullet.Rotation = GlobalRotation;
        bullet.Visible = true;
        lastShot = 0;
    }

}
