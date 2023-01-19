using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
    private double timer = 0;
    private Machine machine;

    private Action<double> currentAct;
    double p1, p2; // parameters

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    //[Signal]
    //public delegate void SignalName(int param1, float param2);

    public override void _Ready()
    {
        machine = this.GetParent<Machine>();
        machine.Connect("doROT", new Callable(this, "doROT"));
    }

    private void rot(double delta)
    {
        double secondsToFinish = 1.0;
        double wiggle = 0.2;
        if (timer < secondsToFinish)
        {
            RotateY((float)(p1 * delta / secondsToFinish));
        }

        if(timer >= secondsToFinish - wiggle)
        {
            machine.bbox.Set(true);
        }
    }
    // signal
    public void doROT(int amount)
    {
        amount -= 127;
        timer = 0;
        currentAct = rot;
        p1 = Mathf.DegToRad((float)amount);
    }

    public override void _Process(double delta)
    {
        if (currentAct is not null)
        {
            currentAct(delta);
        }

        timer += delta;
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.basis * new Vector3(inputDir.x, 0, inputDir.y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.x = direction.x * Speed;
			velocity.z = direction.z * Speed;
		}
		else
		{
			velocity.x = Mathf.MoveToward(Velocity.x, 0, Speed);
			velocity.z = Mathf.MoveToward(Velocity.z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
