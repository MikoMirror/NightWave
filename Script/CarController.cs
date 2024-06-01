using Godot;

public partial class CarController : VehicleBody3D
{
	[Export]
	public float EngineForce = 500f;

	[Export]
	public float BrakeForce = 1500f; 

	[Export]
	public float MaxLeftSteeringAngle = 80f;

	[Export]
	public float MaxRightSteeringAngle = 80f;

	[Export]
	public float AccelerationSmoothing = 0.1f;

	[Export]
	public float AccelerationFactor = 2f;

	private VehicleWheel3D _backLeftWheel;
	private VehicleWheel3D _backRightWheel;
	private VehicleWheel3D _frontLeftWheel;
	private VehicleWheel3D _frontRightWheel;
	private float _currentEngineForce = 0f;
	private Vector2 _swipeStartPosition;
	private bool _isSwiping = false;
	private float _screenWidth = 100f;
	private bool _applyBrake = false;

	public override void _Ready()
	{
		_backLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		_backRightWheel = GetNode<VehicleWheel3D>("BackRight");
		_frontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		_frontRightWheel = GetNode<VehicleWheel3D>("FrontRight");

		_screenWidth = GetViewport().GetVisibleRect().Size.X;
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleInput();
		ApplyMovement();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch eventTouch)
		{
			if (eventTouch.Pressed)
		{
			_isSwiping = true;
			_swipeStartPosition = eventTouch.Position;
			_applyBrake = false; 
		}
		else if (!eventTouch.Pressed)
		{
			_isSwiping = false;
			_applyBrake = true; 
			ApplySharpBrake(); 
		}
		}
		else if (@event is InputEventMouseButton eventMouse && eventMouse.ButtonIndex == MouseButton.Left)
		{
			if (eventMouse.Pressed)
			{
				_isSwiping = true;
				_swipeStartPosition = eventMouse.Position;
				_applyBrake = false; 
			}
			else if (!eventMouse.Pressed)
			{
				_isSwiping = false;
				_applyBrake = true; 
			}
		}
	}

	private void HandleInput()
	{
		float targetEngineForce = 0f;
		if (_isSwiping) 
		{
			targetEngineForce = EngineForce * AccelerationFactor;
		}
		_currentEngineForce = Mathf.Lerp(
			_currentEngineForce, targetEngineForce, AccelerationSmoothing
		);

		float steerInput = 0f;

		if (_isSwiping)
		{
			steerInput = CalculateSteeringValue(GetViewport().GetMousePosition().X);
		}

		Steer(steerInput);
	}

	private float CalculateSteeringValue(float horizontalPosition)
	{
		float normalizedX = (horizontalPosition / _screenWidth) * 2 - 1; 
		if (normalizedX < 0)
		{
			return -normalizedX * MaxRightSteeringAngle / MaxRightSteeringAngle;
		}
		else
		{
			return -normalizedX * MaxLeftSteeringAngle / MaxLeftSteeringAngle; 
		}
	}

	private void Steer(float steerAngleDegrees)
	{
		float steerAngleRadians = steerAngleDegrees * (Mathf.Pi / 180f); 
		_frontLeftWheel.Steering = Mathf.Clamp(steerAngleRadians, -MaxRightSteeringAngle, MaxLeftSteeringAngle);
		_frontRightWheel.Steering = Mathf.Clamp(steerAngleRadians, -MaxRightSteeringAngle, MaxLeftSteeringAngle);
	}

	private void ApplySharpBrake()
{
	_backLeftWheel.Brake = BrakeForce;
	_backRightWheel.Brake = BrakeForce;
	Vector3 impulse = -LinearVelocity.Normalized() * 50000f;
	ApplyCentralImpulse(impulse);
}

	private void ApplyMovement()
	{
		_backLeftWheel.EngineForce = _currentEngineForce;
		_backRightWheel.EngineForce = _currentEngineForce;
		if (_applyBrake)
		{
			_backLeftWheel.Brake = BrakeForce;
			_backRightWheel.Brake = BrakeForce;
			if (Mathf.IsZeroApprox(LinearVelocity.Length())) 
			{
				_applyBrake = false; 
			}
		}
		else 
		{
			_backLeftWheel.Brake = 0; 
			_backRightWheel.Brake = 0;
		}
	}
}
