using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour {

	public float reward = 0.2f;
	internal Track track;

	private void OnTriggerEnter(Collider other) {
		VehicleAgent agent = other.GetComponentInParent<VehicleAgent>();
		if (agent == null || agent != track.agent) return;

		agent.AddReward(reward);
		track.NextCheckpoint();
	}
}
