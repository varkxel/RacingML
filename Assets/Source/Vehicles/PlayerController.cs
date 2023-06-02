using UnityEngine;
using static Unity.Mathematics.math;

namespace Source.Vehicles {
	[RequireComponent(typeof(Vehicle))]
	public class PlayerController : MonoBehaviour {
		private Vehicle vehicle;

		private void Awake() {
			vehicle = GetComponent<Vehicle>();
		}

		private void Update() {
			vehicle.accelerator = clamp(Input.GetAxisRaw("Vertical"), 0.0f, 1.0f);
			vehicle.steering = Input.GetAxisRaw("Horizontal");

			vehicle.brakes = abs(clamp(Input.GetAxisRaw("Vertical"), -1.0f, 0.0f));
		}
	}
}
