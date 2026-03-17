using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class Laser : RayCast2D
{
	[Signal]
	public delegate void RaycastCollideEventHandler(double damage, Entity entity);
	[Export]
	public double CastSpeed = 7000.0;
	[Export]
	public float MaxLenght = 500f;

	public int MaxPierce = 1;
	public int Quantity;
	public double Damage;

	private double _temperature = 0d;
	const double MIN_TEMPERATURE = 0d;
	const double MAX_TEMPERATURE = 100d;
	const double OVERHEAT_TEMPERATURE = 80d;
	const double HALFWAY_TEMPERATURE = 50d;
	public bool IsOnCooldown = false;
	[Export]
    public double Temperature
    {
		get{return _temperature;}
		set{
			_temperature = Mathf.Clamp(value, MIN_TEMPERATURE, MAX_TEMPERATURE);

			if(Termometer == null) return;
			Termometer.Value = _temperature;

			if(_temperature < HALFWAY_TEMPERATURE) Termometer.TintProgress = Colors.Green;
			else if(_temperature >= HALFWAY_TEMPERATURE && _temperature < OVERHEAT_TEMPERATURE) Termometer.TintProgress = Colors.Yellow;
			else if(_temperature >= OVERHEAT_TEMPERATURE) Termometer.TintProgress = Colors.Red;

			if(_temperature >= MAX_TEMPERATURE)
			{
				IsCasting = false;
				IsOnCooldown = true;
				CooldownSound.Play();
			}

			CooldownParticles.Emitting = IsOnCooldown;
		}
	}
	[Export]
	public float StartingDistance = 40f;
	private Color _color = Colors.White;
	
	[Export]
	public Color Color
	{
		get {return _color;}
		set
		{
			_color = value;
			if(line2D == null) return;

			line2D.Modulate = value;
			CastingParticles.Modulate = value;
			BeamParticles.Modulate = value;
			CollisionParticles.Modulate = value;
		}
	}
	private bool _isCasting = false;
	[Export]
	public bool IsCasting
	{
		get  {return _isCasting;}
        set {
			if(_isCasting == value) return;
			_isCasting = value;

			SetPhysicsProcess(_isCasting);

			if(BeamParticles == null) return;

			BeamParticles.Emitting = value;
			CastingParticles.Emitting = value;

			if (_isCasting)
			{
				Enabled = _isCasting;

				var laserStart = Vector2.Up * StartingDistance;
				line2D.SetPointPosition(0, laserStart);
				line2D.SetPointPosition(1, laserStart);

				CastingParticles.Position = laserStart;

				ShootSound.Play();

				Appear();
			}
			else		
			{
				TargetPosition = Vector2.Zero;
				CollisionParticles.Emitting = false;
				ShootSound.Stop();
				Disappear();
			}
        }
	}
	public Line2D line2D;
	public GpuParticles2D CastingParticles, CollisionParticles, BeamParticles, CooldownParticles;
	public AudioStreamPlayer2D ShootSound, CooldownSound;
	public TextureProgressBar Termometer;
	//public Tween Tween = null;

    private void Disappear()
    {
        line2D.Visible = false;
    }


    private void Appear()
    {
        line2D.Visible = true;
    }


    public override void _Ready()
    {
		line2D = GetNode<Line2D>(nameof(Line2D));
		CastingParticles = GetNode<GpuParticles2D>("CastingParticles");
		CollisionParticles = GetNode<GpuParticles2D>("CollisionParticles");
		BeamParticles = GetNode<GpuParticles2D>("BeamParticles");
		CooldownParticles = GetNode<GpuParticles2D>("CooldownParticles");

		ShootSound = GetNode<AudioStreamPlayer2D>("SoundEffects/ShootSound");
		CooldownSound = GetNode<AudioStreamPlayer2D>("SoundEffects/CooldownSound");
		Termometer = GetNode<TextureProgressBar>("CanvasLayer/Termometr");

		Termometer.MinValue = MIN_TEMPERATURE;
		Termometer.MaxValue = MAX_TEMPERATURE;

		line2D.Visible = false;

		IsCasting = false;
		Color = _color;
		
		line2D.Points = [Vector2.Up * StartingDistance, Vector2.Zero];
		CastingParticles.Position = line2D.Points[0];

		RaycastCollide += OnRaycastCollide;

		AddToGroup("laser");
	
    }

    public override void _PhysicsProcess(double delta)
    {
        TargetPosition = TargetPosition.MoveToward(Vector2.Up * MaxLenght, (float)(CastSpeed * delta));

		var laserEndPosition = TargetPosition;
		ForceRaycastUpdate();
		if (IsColliding())
		{
			laserEndPosition = ToLocal(GetCollisionPoint());
			CollisionParticles.GlobalRotation = GetCollisionNormal().Angle();
			CollisionParticles.Position = laserEndPosition;
		}
		line2D.SetPointPosition(1, laserEndPosition);

		CollisionParticles.Emitting = IsCasting && IsColliding();

		var laserStartPosition = line2D.Points[0];
		BeamParticles.Position = laserStartPosition + (laserEndPosition - laserStartPosition) * 0.5f; // middle of the line

		// setting the box in which the particles will display
		if(BeamParticles.ProcessMaterial is ParticleProcessMaterial material)
		{
			Vector3 extents = material.EmissionBoxExtents;
			extents.X = laserEndPosition.DistanceTo(laserStartPosition) * 0.5f;
			material.EmissionBoxExtents = extents;
		}

		var hits = GetAllRayHits(ToGlobal(laserStartPosition), ToGlobal(laserEndPosition), MaxPierce);

		foreach(var hit in hits)
		{
			try{
				var collider = hit["collider"].As<Entity>();
				EmitSignal(SignalName.RaycastCollide, Damage, collider);
			}
			catch(System.InvalidCastException)
			{
				continue;
			}

		}
    }

    public override void _Process(double delta)
    {
        if(IsCasting) Temperature += 0.1d;
		else if(IsCasting == false && IsOnCooldown == false) Temperature -= 0.1d;

		if(IsOnCooldown)
		{
			Temperature -= 0.1;
			IsCasting = false;
			
			if(_temperature <= 0) { 
				IsOnCooldown = false;
			}
		}
    }

	List<Dictionary> GetAllRayHits(Vector2 from, Vector2 to, int pierceCount)
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		List<Dictionary> hits = [];
		Array<Rid> exclude = [];

		for(int i=0; i < pierceCount; i++)
		{
			var query = PhysicsRayQueryParameters2D.Create(from, to);
			query.Exclude = exclude;

			var result = spaceState.IntersectRay(query);

			if(result.Count == 0) break;
			
			hits.Add(result);
			exclude.Add(result["rid"].As<Rid>());
		}
		return hits;

	}

	public void OnRaycastCollide(double damage, Entity character)
	{
		character.EntityHP.DealDamage(damage);
	}

}
