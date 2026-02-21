using System;
using System.Collections.Generic;
using Godot;

public partial class SeekingAlien : Entity
{
    public NavigationAgent2D NavigationAgent;
    [Export]
    public float Speed;

    public Player Player;

    private Vector2 _randomVector, _newVelocity, _vectorToPlayer, _newPosition, _lastPosition;
    private List<Vector2> _points;
    private bool _playerFound = false;
    private bool _isNavigating = false;
    private bool _isTouchingWall = false;
    public override void _Ready()
    {
        base._Ready();
        NavigationAgent = GetNode<NavigationAgent2D>(nameof(NavigationAgent2D));

        AddToGroup("entities");
        SetHitboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);
        SetHurtboxboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);

        //_screenSize = GetViewportRect().Size;
        (float x, float y) = GenereateNewRandomPoint();
        _newPosition = Position + new Vector2(x, y);

        _points = [];
        CalculateNewPosition();
		StartNavigation();
        GD.Print("GO");
    }

    public override void _Process(double delta)
    {
        if (Player == null) return;
        
        var facingDirection = _newVelocity.Normalized();
		_vectorToPlayer = Position.DirectionTo(Player.GlobalPosition);
		var lenghtBetween = Position - Player.Position;
		if(_vectorToPlayer.Dot(facingDirection) >= 0.75 && lenghtBetween.Length() < 350)
		{
			_playerFound = true;
		}
		// if (_playerFound)
		// {
		// 	GetNode<Timer>("MovementTimer").Stop();
		// }
    }

    public override void _PhysicsProcess(double delta)
    {
        Navigate((float)delta);
    }

    public async void StartNavigation()
    {
        if(_isNavigating) return;
		_isNavigating = true;

		while(!_playerFound)
		{
			_points.Clear(); // Wyczyść starą listę
			CalculateNewPosition(); // Zawsze generuj nowy punkt
			
			_newPosition = _points[0]; // Zawsze pierwszy element
			NavigationAgent.TargetPosition = _newPosition;

			await ToSignal(GetTree().CreateTimer(0.8), Timer.SignalName.Timeout);

			GD.Print("Dotarłem!");
		}

		_isNavigating = false;
    }

    public void Navigate(float delta)
    {
        float stoppingDistance = 30f;
		if(GlobalPosition.DistanceTo(_newPosition) < stoppingDistance)
		{
			return;
		}

		// Jeśli znaleziono gracza, idź w jego kierunku
		if(_playerFound)
		{
			_newPosition = Player.GlobalPosition;
			NavigationAgent.TargetPosition = _newPosition;
		}

		

		Vector2 nextPathPosition = NavigationAgent.GetNextPathPosition();
		_newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * Speed;
		Position += _newVelocity * delta;

		if(_newVelocity.Length() > 0)
		{
			var targetRotation = _newVelocity.Angle() + Mathf.Pi/2;
			Rotation = Mathf.LerpAngle(Rotation, targetRotation, 6 * (float)delta);
		}
    }

    public void CalculateNewPosition()
	{
		if(_playerFound) return;

		int maxAttepmpts = 10;

		for(int i = 0; i < maxAttepmpts; i++)
		{

			(float randomX, float randomY) = GenereateNewRandomPoint();
			_randomVector = new Vector2(randomX, randomY);
			_lastPosition = Position;
			var newPosition = _lastPosition + _randomVector;
			
			var facingDirection = _newVelocity.Normalized();
			// Jeśli dotykamy ściany, akceptuj tylko punkty w przeciwnym kierunku
			if(_isTouchingWall)
			{
				if(Position.DirectionTo(newPosition).Dot(facingDirection) < 0) // Przeciwny kierunek
				{
					_randomVector = -_newVelocity.Normalized();
					_points.Clear();
					_newPosition = GlobalPosition + _randomVector * 150f;
					_points.Add(_newPosition);
					NavigationAgent.TargetPosition = _newPosition;
					_points.Add(newPosition);
					GD.Print($"Punkt po odbiciu: {newPosition}");
					_isTouchingWall = false; // Reset flagi
					return;
				}
			}
			else
			{
				// Normalnie - tylko punkty do przodu
				if(Position.DirectionTo(newPosition).Dot(facingDirection) >= 0.0f)
				{
					_points.Add(newPosition);
					return;
				}
			}
		}
		_points.Add(Position + (_isTouchingWall ? -_newVelocity : _newVelocity).Normalized() * 200);
   		 _isTouchingWall = false;
	}

    public override void InitializeValues(EntitySpawnParams spawnParams)
    {
        if (spawnParams is not SeekingAlienParams seekingAlienParams)
            throw new ArgumentException("Nieprawidłowy typ parametrów");

		Speed = seekingAlienParams.VelocityValue;
        Position = seekingAlienParams.Position;
        Rotation = seekingAlienParams.RotationDirection;
        Player = seekingAlienParams.Player;
    }


    private (float randomX, float randomY) GenereateNewRandomPoint(){
		double xMinVal = -200;double yMinVal = -200;
		double xMaxVal = 200; double yMaxVal = 200;
		return ((float)GD.RandRange(xMinVal, xMaxVal), (float)GD.RandRange(yMinVal, yMaxVal));
	}
}
