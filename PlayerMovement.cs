using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
    private double timer = 0;
    private double ptimer = 0;
    private Machine machine;

    private Action<double> currentAct;
    private Action<double> currentPAct;
    double p1, p2; // parameters

    Vector3 velocity;
    Vector3 movdir = Vector3.Zero;

    private bool gravityEnabled = true;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    //[Signal]
    //public delegate void SignalName(int param1, float param2);

    public override void _Ready()
    {
        machine = this.GetParent<Machine>();
        machine.Connect("doROT", new Callable(this, "doROT"));
        machine.Connect("doINTERACT", new Callable(this, "doInteract"));
        machine.Connect("doMOV_", new Callable(this, "doMov_"));
        machine.Connect("doJUMP_UP", new Callable(this, "doJump_up"));
    }

    private void rot(double delta)
    {
        double secondsToFinish = 0.05;
        if (timer < secondsToFinish)
        {
            RotateY((float)(p1 * delta / secondsToFinish));
        }

        if(timer >= secondsToFinish)
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
        currentPAct = null;
        p1 = Mathf.DegToRad((float)amount);
    }

    // signal TODO
    public void doInteract()
    {
        
    }

    private void mov(double delta)
    {
        if (ptimer >= p1)
        {
            machine.bbox.Set(true);
            movdir = Vector3.Zero;
        }
    }

    // signal
    public void doMov_(int amount, Vector2 dir)
    {
		movdir = (Transform.basis * new Vector3(dir.x, 0, dir.y)).Normalized();
        p1 = ((double)amount) / 10.0;
        currentPAct = mov;
        currentAct = null;
        ptimer = 0;
    }

    // signal
    private bool willJump = false;
    private double lastJump = 0.0f;
    private double jumpStrength = 0;
    public void doJump_up(int amount)
    {
        jumpStrength = JumpVelocity * ((double)amount) / 255.0;
        if (lastJump > 0.3)
        {
            willJump = true;
            lastJump = 0;
        }
    }

    public override void _Process(double delta)
    {
        if (currentAct is not null)
        {
            currentAct(delta);
        }

        timer += delta;
        lastJump += delta;
    }

	public override void _PhysicsProcess(double delta)
	{
        velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor() && gravityEnabled)
        {
            velocity.y -= gravity * (float)delta;
        }

		// Handle Jump.
		if (willJump && IsOnFloor())
        {
			velocity.y = JumpVelocity;
            willJump = false;
        }

        if (currentPAct is not null)
        {
            currentPAct(delta);
        }
        ptimer += delta;

		if (movdir != Vector3.Zero)
		{
			velocity.x = movdir.x * Speed;
			velocity.z = movdir.z * Speed;
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
