using System;
using UnityEngine;

public class Track : MonoBehaviour {

	public Checkpoint[] checkpoints;
	[NonSerialized] public VehicleAgent agent;

	private int checkpointIndex;

	public Checkpoint Current => checkpoints[checkpointIndex];
	public Checkpoint Previous {
		get {
			int index = checkpointIndex - 1;
			if (index < 0) index = checkpoints.Length + index;
			return checkpoints[index];
		}
	}

	private void Awake() {
		foreach (Checkpoint checkpoint in checkpoints) {
			checkpoint.track = this;
		}

		Reset();
	}

	public void Reset() {
		checkpointIndex = 0;
		foreach (Checkpoint checkpoint in checkpoints) {
			checkpoint.gameObject.SetActive(false);
		}
		checkpoints[checkpointIndex].gameObject.SetActive(true);
	}

	public void NextCheckpoint() {
		Checkpoint old = checkpoints[checkpointIndex];
		old.gameObject.SetActive(false);

		checkpointIndex = ++checkpointIndex % checkpoints.Length;

		Checkpoint next = checkpoints[checkpointIndex];
		next.gameObject.SetActive(true);
	}
}
