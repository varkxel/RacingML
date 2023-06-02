using System.Linq;
using UnityEngine;

public class Vehicle : MonoBehaviour {

	[Header("Tags")]
	[SerializeField] private string tags_Steer = "Wheel_Front";
	[SerializeField] private string tags_Drive = "Wheel_Back";
	[SerializeField] private string tags_SteerDrive = "Wheel_SteerDrive";

	public float torque = 118.0f;
	public float maxSteering = 40.0f;
	public float maxBrake = 350.0f;
	public bool forward = true;

	[Range(0.0f, 1.0f)] public float accelerator;
	[Range(-1.0f, 1.0f)] public float steering;
	[Range(0.0f, 1.0f)] public float brakes;
	public WheelCollider[] wheels { get; private set; }
	public WheelCollider[] wheels_Steering { get; private set; }
	public WheelCollider[] wheels_Driving { get; private set; }

	private void Awake() {
		wheels = GetComponentsInChildren<WheelCollider>();
		wheels_Steering = wheels.Where(wheel => IsWheel(wheel, tags_Steer)).ToArray();
		wheels_Driving = wheels.Where(wheel => IsWheel(wheel, tags_Drive)).ToArray();
	}

	private void Update() {
		foreach (WheelCollider wheel in wheels_Driving) {
			wheel.motorTorque = torque / wheels_Driving.Length * accelerator * (forward ? 1.0f : -1.0f);
		}

		foreach (WheelCollider wheel in wheels_Steering) {
			wheel.steerAngle = steering * maxSteering;
		}

		foreach (WheelCollider wheel in wheels) {
			wheel.brakeTorque = brakes * maxBrake;
		}
	}

	private bool IsWheel(WheelCollider wheel, string tag) {
		return wheel.CompareTag(tags_SteerDrive) || wheel.CompareTag(tag);
	}
}
