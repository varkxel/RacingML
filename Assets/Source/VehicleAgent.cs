using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

public class VehicleAgent : Agent {

	public const int entityCount = 8;

	private const float MinVelocity = 0.1f;
	public const float IdlePunishment = 10.0f;
	private static readonly List<int> spawned = new List<int>();
	private static int spawnedCount;
	public VehicleAgentSpawn spawn;

	public float distanceReward = 0.1f;
	private Rigidbody rb;

	private Vehicle vehicle;

	private void Update() {
		float velocity = length(new float3(rb.velocity).xz);
		#if IDLE_PUNISHMENT
		if (velocity <= MinVelocity) {
			AddReward(-IdlePunishment * Time.deltaTime);
			return;
		}
		#endif
		float reward = distanceReward * velocity * Time.deltaTime;

		CalculateCheckpointDirection(out float3 checkpointDirection, out _);
		reward *= dot(rb.velocity, checkpointDirection);

		AddReward(reward);
	}

	private void OnCollisionEnter(Collision collision) {
		#if COLLISION_PENALTIES
		float force = length(collision.impulse);
		AddReward(-force / 100.0f);
		#endif
	}

	public override void Initialize() {
		vehicle = GetComponent<Vehicle>();
		rb = vehicle.GetComponent<Rigidbody>();
	}

	public override void OnEpisodeBegin() {
		#if RESET_SPAWNS
		if (spawn != null) {
			spawn.track.agent = null;
		}

		VehicleAgentSpawn[] spawns = FindObjectsOfType<VehicleAgentSpawn>();
		int spawnIndex;
		do {
			spawnIndex = Random.Range(0, spawns.Length);
		} while (spawned.Contains(spawnIndex));

		spawn = spawns[spawnIndex];
		spawned.Add(spawnIndex);
		spawn.track.Reset();
		spawn.track.agent = this;
		#endif

		vehicle.accelerator = 0.0f;
		vehicle.brakes = 0.0f;
		vehicle.steering = 0.0f;

		#if RESET_SPAWNS
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		Transform spawnTransform = spawn.transform;
		rb.position = spawnTransform.position;
		rb.rotation = spawnTransform.rotation;
		spawned.Add(spawnIndex);

		if (++spawnedCount >= entityCount) {
			spawned.Clear();
			spawnedCount = 0;
		}
		#endif
	}

	public override void OnActionReceived(ActionBuffers actions) {
		ActionSegment<float> vectorAction = actions.ContinuousActions;

		vehicle.accelerator = unlerp(-1.0f, 1.0f, vectorAction[0]);
		vehicle.brakes = unlerp(-1.0f, 1.0f, vectorAction[2]);
		vehicle.steering = vectorAction[1];
		vehicle.forward = vectorAction[3] >= 0.0f;
	}

	private void CalculateCheckpointDirection(out float3 direction, out float relativeDistance) {
		float3 next = spawn.track.Current.transform.position;
		float3 previous = spawn.track.Previous.transform.position;

		float3 current = vehicle.transform.position;

		direction = normalizesafe(next - current);
		relativeDistance = distancesq(current, next);
		relativeDistance /= distancesq(previous, next);
	}

	public override void CollectObservations(VectorSensor sensor) {
		float3 forward = vehicle.transform.forward;
		CalculateCheckpointDirection(out float3 checkpointDirection, out float checkpointDistance);

		// Observe the dot product to the next checkpoint (1 observations)
		sensor.AddObservation(dot(forward, checkpointDirection));

		// Observe the relative distance to the next checkpoint (1 observation)
		sensor.AddObservation(checkpointDistance);

		// Observe direction + speed (3+1 observations)
		sensor.AddObservation(forward);
		sensor.AddObservation(length(rb.velocity));
	}
}
