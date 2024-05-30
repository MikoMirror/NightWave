using Godot;
using System;
using System.Collections.Generic;
using System.Runtime;

public partial class RoadManager : Node3D
{
	[Export]
	public PackedScene RoadSegmentScene; 

	[Export]
	public float SegmentLength = 119f;  

	[Export]
	public float CameraSpeed = 100f;

	[Export]
	public int SegmentsAhead = 5;

	[Export]
	public int SegmentsBehind = 2;

	[Export]
	public float CameraFarClip = 500f;

	private Camera3D _camera;
	private LinkedList<Node3D> _roadSegments = new LinkedList<Node3D>();
	private Vector3 _lastSegmentPosition;
	private Queue<Node3D> _segmentPool = new Queue<Node3D>();

	public override void _Ready()
	{
		GCSettings.LatencyMode = GCLatencyMode.LowLatency;
		_camera = GetNode<Camera3D>("Camera3D");
		_camera.Far = CameraFarClip;
		_lastSegmentPosition = Vector3.Zero;
		RoadSegmentScene = GD.Load<PackedScene>("res://scene/RoadSegment.tscn");
		
		for (int i = 0; i < SegmentsAhead; i++)
		{
			GenerateRoadSegment();
		}
	}

	public override void _Process(double delta)
	{
		_camera.GlobalPosition += new Vector3(CameraSpeed * (float)delta, 0, 0);

		GenerateRoadSegmentsIfNeeded();
		RemoveOffscreenSegments();
		if (_camera.GlobalPosition.X % (SegmentsAhead * SegmentLength) < 0.01f)
		{
			GC.Collect();
		}
	}

	private void GenerateRoadSegmentsIfNeeded()
	{
		float cameraPositionX = _camera.GlobalPosition.X;
		float segmentsAheadDistance = SegmentsAhead * SegmentLength;

		if (_lastSegmentPosition.X < cameraPositionX + segmentsAheadDistance)
		{
			while (_lastSegmentPosition.X < cameraPositionX + segmentsAheadDistance)
			{
				GenerateRoadSegment();
			}
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
