using Godot;
using System.Collections.Generic;
using System.Runtime;

public partial class RoadManager : Node3D
{
	[Export]
	public PackedScene RoadSegmentScene;
	[Export]
	public float SegmentLength = 119f;
	[Export]
	public int SegmentsAhead = 10;
	[Export]
	public int SegmentsBehind = 2;

	private LinkedList<Node3D> _roadSegments = new LinkedList<Node3D>();
	private Vector3 _lastSegmentPosition;
	private Queue<Node3D> _segmentPool = new Queue<Node3D>();
	private VehicleBody3D _carNode;
	private Camera3D _camera;
	private float _timeSinceLastCarPositionUpdate = 0f;

	public override void _Ready()
	{
		GCSettings.LatencyMode = GCLatencyMode.LowLatency;
		_lastSegmentPosition = Vector3.Zero;

		RoadSegmentScene = GD.Load<PackedScene>("res://scene/RoadSegment.tscn"); 
		var carScene = GD.Load<PackedScene>("res://scene/MainCar.tscn"); 
		_carNode = carScene.Instantiate<VehicleBody3D>();
		AddChild(_carNode);

		_camera = _carNode.GetNode<Camera3D>("Camera3D");

		_carNode.GlobalPosition = _lastSegmentPosition + new Vector3(0, 15, 0); 

		for (int i = 0; i < SegmentsAhead; i++)
		{
			GenerateRoadSegment();
		}
	}

	 public override void _Process(double delta)
	{
		GenerateRoadSegmentsIfNeeded();
		RemoveOffscreenSegments();
		_timeSinceLastCarPositionUpdate += (float)delta;
		if (_timeSinceLastCarPositionUpdate >= 1.0f)
		{
			GD.Print("Car Position:", _carNode.GlobalPosition); 
			_timeSinceLastCarPositionUpdate = 0f; 
		}
	}
	
	private void GenerateRoadSegmentsIfNeeded()
	{
		float carPositionX = _carNode.GlobalPosition.X;
		float segmentsAheadDistance = SegmentsAhead * SegmentLength;

		while (_lastSegmentPosition.X < carPositionX + segmentsAheadDistance)
		{
			GenerateRoadSegment();
		}
	}

	private void RemoveOffscreenSegments()
	{
		float cameraPositionX = _camera.GlobalPosition.X;
		float segmentsBehindDistance = SegmentsBehind * SegmentLength;

		while (_roadSegments.First != null && _roadSegments.First.Value.GlobalPosition.X < cameraPositionX - segmentsBehindDistance)
		{
			RecycleRoadSegment(_roadSegments.First.Value);
			_roadSegments.RemoveFirst();
		}
	}

	private void GenerateRoadSegment()
	{
		Node3D segmentInstance;
		if (_segmentPool.Count > 0)
		{
			segmentInstance = _segmentPool.Dequeue();
			segmentInstance.Visible = true;
		}
		else
		{
			segmentInstance = RoadSegmentScene.Instantiate<Node3D>();
			AddChild(segmentInstance);
		}

		segmentInstance.GlobalPosition = _lastSegmentPosition;
		_lastSegmentPosition += new Vector3(SegmentLength, 0, 0);
		_roadSegments.AddLast(segmentInstance);
	}

	private void RecycleRoadSegment(Node3D segment)
	{
		segment.Visible = false;
		_segmentPool.Enqueue(segment);
	}
}
