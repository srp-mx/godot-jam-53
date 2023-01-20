using Godot;
using System;
using System.Collections.Generic;

public partial class Shoot : Node3D
{
    [Export]
    float shotTime = 0.5f;
    [Export]
    int maxAmmo = 7;
    int currAmmo = 0;
    [Export]
    float reloadTime = 2.5f;
    float reloadTimer = 0;

    PackedScene bulletResource;
    List<Node3D> bulletPool = new();
    Machine machine;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        bulletResource = GD.Load<PackedScene>("res://scenes/prefabs/bullet.tscn");
        machine = (Machine) this.FindParent("Machine");
        machine.Connect("doSHOOT", new(this, "ShootBullet"));
        machine.Connect("doRELOAD", new(this, "Reload"));
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
        reloadTimer += (float)delta;
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
        if (lastShot < shotTime ||  <= 0)
            return;

        Node3D bullet = getBullet();
        bullet.Position = GlobalPosition;
        bullet.Rotation = GlobalRotation;
        bullet.Visible = true;
        lastShot = 0;
        currAmmo--;
        machine.SetAmmoReg(currAmmo);
    }

    public void Reload()
    {
        currAmmo = maxAmmo;
        reloadTimer = 0;
        while (reloadTimer < reloadTime)
        {

        }
        machine.SetAmmoReg(currAmmo);
        machine.bbox.Set(true);
        return;
    }

}
