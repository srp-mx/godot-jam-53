using Godot;
using System;
using NoiseTest;

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
    RayCast3D backray;
    Area3D mycol;
    private Vector2 mouseacc = Vector2.Zero; // accumulated mouse
    private float lastPlayerZoom;

    private OpenSimplexNoise noise = new OpenSimplexNoise(0);
    private Vector2 zeroNoise;

    private float trauma;
    [Export]
    public float traumaDamp = 0.5f;
    [Export]
    public float shakeStability = 5.0f;

    private bool mouseCapture = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        Input.SetUseAccumulatedInput(false);
        player = (CharacterBody3D)GetParent().FindChild("CharacterBody3D");
        lastPlayerZoom = orbitRadius;
        playercol = (CollisionShape3D)player.FindChild("CollisionShape3D");
        ray = (RayCast3D)FindChild("RayCast3D");
        backray = (RayCast3D)FindChild("BackRayCast3D");
	}

    public override void _Process(double delta)
    {

        if (mouseCapture)
        {
            Input.SetMouseMode(Input.MouseModeEnum.Captured);
        }
        else
        {
            Input.SetMouseMode(Input.MouseModeEnum.Confined);
        }

        float shake = trauma * trauma * trauma;
        rot.y += sensitivity.x * mouseacc.x * (float)delta;
        rot.x += sensitivity.y * mouseacc.y * (float)delta * (invertY ? -1 : 1); 
        rot.x = Mathf.Clamp(rot.x, -89.9f, 0);

        Vector3 desired = player.Position + deltaPosCam(orbitRadius);

        SetPosition(desired * 0.3f + Position * 0.7f);

        Vector3 shakev = new Vector3(
                    (float)(noise.Evaluate(shake, shake+500) - noise.Evaluate(0, 500)),
                    shakeStability * (float)(1.0 - noise.Evaluate(shake+1000, shake+1500) - noise.Evaluate(1000, 1500)),
                    (float)(noise.Evaluate(shake+2000, shake+2500) - noise.Evaluate(2000, 2500))
                );

        Vector3 target = player.Position + 5*(shakev.Normalized() - new Vector3(0.0f,1.0f,0.0f));

        Transform = Transform.LookingAt(target, shakev.Normalized());

        mouseacc = Vector2.Zero;
        trauma = Mathf.Clamp(trauma - traumaDamp * (float)delta, 0, 1);
    }

    public void AddTrauma(float percent)
    {
        trauma = Mathf.Clamp(percent, 0, 1);
    }

    public override void _PhysicsProcess(double delta)
    {
        ray.SetTargetPosition(new (0, 0, -orbitRadius + 2.0f));
        if (ray.IsColliding())
        {
            var cp = ray.GetCollisionPoint();
            var dist = (player.GlobalPosition - cp).Length();
            orbitRadius = Mathf.Clamp(dist*0.8f, minZoom, maxZoom);
        }
        else
        {
            if (!backray.IsColliding())
            {
                orbitRadius = orbitRadius * 0.9f + lastPlayerZoom * 0.1f;
            }
        }
    }

    // Once per frame (at most) with accumulated input
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            if (mouseCapture)
                mouseacc += motion.Relative;
        }

        if (@event is not InputEventMouseButton button)
            return;

        switch (button.ButtonIndex)
        {
            case MouseButton.WheelDown:
                if (mouseCapture && button.Pressed)
                    playerZoom(1);
                break;
            case MouseButton.WheelUp:
                if (mouseCapture && button.Pressed)
                    playerZoom(-1);
                break;
            case MouseButton.Right:
            case MouseButton.Middle:
                if (button.Pressed)
                    mouseCapture = !mouseCapture;
                break;
        }
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
