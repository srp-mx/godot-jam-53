using Godot;
using System;

public partial class CameraMovement : Camera3D
{
    [Export]
    public Vector2 sensitivity = new(2,2);
    [Export]
    public float minZoom = 2;
    [Export]
    public float maxZoom = 10;
    [Export]
    public bool invertY = false;

    public float orbitRadius = 10.0f;
    public Vector2 rot = Vector2.Zero;
    CharacterBody3D player;
    CollisionShape3D playercol;
    RayCast3D ray;
    Area3D mycol;
    private Vector2 mouseacc = Vector2.Zero; // accumulated mouse
    private float lastPlayerZoom;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        Input.SetUseAccumulatedInput(false);
        player = (CharacterBody3D)GetParent().FindChild("CharacterBody3D");
        lastPlayerZoom = orbitRadius;
        playercol = (CollisionShape3D)player.FindChild("CollisionShape3D");
        ray = (RayCast3D)FindChild("RayCast3D");
	}

    public override void _Process(double delta)
    {
        rot.y += sensitivity.x * mouseacc.x * (float)delta;
        rot.x += sensitivity.y * mouseacc.y * (float)delta * (invertY ? -1 : 1); 
        rot.x = Mathf.Clamp(rot.x, -89.9f, 0);

        Vector3 desired = player.Position + deltaPosCam(orbitRadius);

        SetPosition(desired * 0.8f + Position * 0.2f);

        Transform = Transform.LookingAt(player.Position, new(0,1,0));

        mouseacc = Vector2.Zero;
    }

    public override void _PhysicsProcess(double delta)
    {
        ray.SetTargetPosition(new (0, 0, -orbitRadius));
        if (ray.IsColliding())
        {
            var cp = ray.GetCollisionPoint();
            var dist = (player.GlobalPosition - cp).Length();
            orbitRadius = Mathf.Clamp(dist*0.8f, minZoom, maxZoom);
        }
    }

    // Once per frame (at most) with accumulated input
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
            mouseacc += motion.Relative;

        if (@event is not InputEventMouseButton button)
            return;

        if (!button.IsPressed())
            return;

        if (button.ButtonIndex == MouseButton.WheelDown)
            playerZoom(1);
        else if (button.ButtonIndex == MouseButton.WheelUp)
            playerZoom(-1);
    }

    private Vector3 deltaPosCam(float radius)
    {
        float xang = (rot.x + 90.0f) / 180.0f * Mathf.Pi;
        float yang = rot.y / 180.0f * Mathf.Pi;
        return new Vector3(
        Mathf.Sin(xang) * Mathf.Sin(yang),
        Mathf.Cos(xang),
        Mathf.Sin(xang) * Mathf.Cos(yang)).Normalized() * radius;
    }

    private void playerZoom(float increment)
    {
        orbitRadius = Mathf.Clamp(orbitRadius + increment, minZoom, maxZoom);
        lastPlayerZoom = orbitRadius;
    }
}
