﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This is the agent which represents the Autonomous Driving capabilities of a vehicle.
 * It is built on top of a 'VehicleAgent' and is able to:
 * - Navigate towards a set target, i.e. the next junction
 * - Use the pedals (VehicleAgent's Accelerate and Brake methods) to keep a safe, consistent speed
 * - Use the steering wheel (VehicleAgent's SteerLeft, and SteerRight methods) to follow the road
 * - Keeping the appropriate safety distance with the vehicle in front
 * - Brake suddenly to avoid close, fixed obstacles
 * - Change lanes and behave appropriately with regards to nearby pods
 * - Report permanent obstructions to the NavigationAgent (i.e. to request for a new target)
 **/
[RequireComponent(typeof(VehicleAgent))]
[RequireComponent(typeof(NavigationAgent))]
public class DrivingAgent : MonoBehaviour {

    private const float DistanceThreshold = .5f;
    
    public float minimumFrontDistance = 1f;
    public float frontDistancePerSpeed = .5f;

    private Vector3 currentTarget;

    public void SetNextTarget(Vector3 target) {
        Debug.Log("Target updated: " + target);
        currentTarget = target;
    }

    private VehicleAgent vehicle;

    // Use this for initialization
    void Start() {
        vehicle = gameObject.GetComponent<VehicleAgent>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (vehicle.IsMoving()) {
            // Acceleration

            if (ObstaclePresentInFront()) {
                //Debug.Log ("Obstacle detected! Braking.");
                vehicle.Brake();
            }
            else if (NeedsAcceleration()) {
                //Debug.Log("Way is clear. Accelerating...");
                vehicle.Accelerate();
            }
            else {
                // Coast...
            }

            SteerTowardsTarget();
        }
        else {
            if (NeedsAcceleration()) {
                vehicle.Accelerate();
            }
        }
    }

    // Checks whether there is an obstacle in front of the vehicle or not at a specific distance
    private bool ObstaclePresentAtDistance(float distance) {
        Vector3 forward = gameObject.transform.TransformVector(Vector3.forward);
        Debug.DrawRay(gameObject.transform.position, forward * distance, Color.red, 0.05f, false);
        return Physics.Raycast(gameObject.transform.position, forward, distance);
    }

    // Checks whether there is an obstacle in front of the vehicle at an optimal distance (based on current speed)
    private bool ObstaclePresentInFront() {
        float distance = minimumFrontDistance + (frontDistancePerSpeed * vehicle.GetCurrentSpeed());
        // Debug.Log ("Current speed is " + vehicle.GetCurrentSpeed ().ToString() + ", Front distance is " + distance.ToString ());
        return ObstaclePresentAtDistance(distance);
    }

    private bool NeedsAcceleration() {
        return true;
    }

    private bool IsMoving() {
        return (Mathf.Abs(vehicle.GetCurrentSpeed()) > 0);
    }

    private Vector3 NeededSteeringDirection() {
        Vector3 relativePoint = gameObject.transform.InverseTransformPoint(currentTarget);
        if (relativePoint.x > 0) {
            return Vector3.right;
        }
        else if (relativePoint.x < 0) {
            return Vector3.left;
        }
        else {
            return Vector3.zero;
        }
    }

    private void SteerTowardsTarget() {
        if (NeededSteeringDirection() == Vector3.right) {
            vehicle.SteerRight();
        }
        else if (NeededSteeringDirection() == Vector3.left) {
            vehicle.SteerLeft();
        }
    }

    public bool ReachedCurrentTarget() {
        var distance = Vector3.Distance(transform.position, currentTarget);
        return distance <= DistanceThreshold;
    }
}