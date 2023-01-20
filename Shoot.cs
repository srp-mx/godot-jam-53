using Godot;
using System;
using System.Collections.Generic;

public partial class Shoot : Node3D
{
    PackedScene bulletResource;
    List<Node3D> bulletPool = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        bulletResource = GD.Load<PackedScene>("res://scenes/prefabs/bullet.tscn");
        for(int i = 0; i < 16; i++)
        {
            var bullet = (Node3D)bulletResource.Instantiate();
            bulletPool.Add(bullet);
            GetTree().GetRoot().CallDeferred("add_child", bullet);
            bullet.Visible = false;
        }
	}

    float lastPress = 0.0f;
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Enter) && lastPress > 0.5f)
        {
            ShootBullet();
            lastPress = 0;
        }
        lastPress += (float)delta;
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

    public void ShootBullet()
    {
        Node3D bullet = getBullet();
        bullet.Position = GlobalPosition;
        bullet.Rotation = GlobalRotation;
        bullet.Visible = true;
    }

}
