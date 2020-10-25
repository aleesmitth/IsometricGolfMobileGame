﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBallController : MonoBehaviour {
    private const float MIN_VELOCITY = 2;
    public FloatValue shotStrength;
    public FloatValue ballPower;
    public FloatValue touchDeadzone;
    public FloatValue rotationSpeed;
    public FloatValue maxStrength;
    public Transform ballDirection;
    public AimBallsController aimBallsController;
    private Vector3 initialTouchPosition;
    private Touch touch;
    
    //cache of expensive getters
    private Camera _camera;
    private Rigidbody _rigidbody;
    private void Start() {
        this._camera = Camera.main;
        this._rigidbody = transform.GetComponent<Rigidbody>();
    }

    private void Update() {
        if (_rigidbody.velocity.sqrMagnitude < MIN_VELOCITY) {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
            
        if (Input.touchCount != 1) return;
        this.touch = Input.GetTouch(0);
        switch (touch.phase) {
            case TouchPhase.Began: StartDrag();
                break;
            //case TouchPhase.Stationary:
            case TouchPhase.Moved: Dragging(); 
                break;
            case TouchPhase.Canceled:
            case TouchPhase.Ended: Release(); 
                break;
            case TouchPhase.Stationary:
            default:
                break;
        }
    }

    private void StartDrag() {
        this.initialTouchPosition = CastRayFromScreenToWorld();
        //Debug.DrawRay(mainBall.position, initialTouchPosition, Color.red, 2f);
    }

    private void Dragging() {
        if(!ImStill()) return;
        var position = CastRayFromScreenToWorld();
        //direction from where i am touching now to initial touch
        var direction = initialTouchPosition - position;
        //if it was a tap on the screen return
        if (direction.magnitude < touchDeadzone.value) return;
        //direction relative to my ball
        //direction = mainBall.position - direction;
        //disable rotation in y axis
        direction.y = 0;

        //Debug.DrawRay(mainBall.position, direction, Color.cyan, 2f);
        //Debug.DrawRay(mainBall.position, direction, Color.cyan, 2f);
        var lookRotation = Quaternion.LookRotation(direction,Vector3.up);
        ballDirection.rotation = Quaternion.Lerp(ballDirection.rotation, lookRotation, Time.deltaTime * rotationSpeed.value);
        UpdateShotStrength(position);
        //mainBall.transform.forward = direction;
        //mainBall.ro

        //mainBall.rotation();
    }

    private void Release() {
        _rigidbody.AddForce(ballDirection.forward * shotStrength.value, ForceMode.Impulse);
        shotStrength.value = 0;
        aimBallsController.ResetBalls();
    }

    private Vector3 CastRayFromScreenToWorld() {
        // TODO esto es lo complicado, el touch es en 2d entonces necesito decirle cuanto Z, pero si le doy un z fijo queda
        // muy hardcodeada la distancia a la que toca.

        Vector3 position = Vector3.zero;
        // create ray from the camera and passing through the touch position:
        Ray ray = _camera.ScreenPointToRay(Input.GetTouch(0).position);
        // create a logical plane at this object's position (my main ball in this case)
        // and perpendicular to world Y:
        Plane plane = new Plane(Vector3.up, ballDirection.position);
        if (plane.Raycast(ray, out var distance)){ // if plane hit...
            position = ray.GetPoint(distance); // get the point
            // pos has the position in the plane you've touched
        }

        return position;
    }

    private bool ImStill() {
        //check if object is still moving
        return _rigidbody.velocity.sqrMagnitude < MIN_VELOCITY;
    }

    private void UpdateShotStrength(Vector3 position) {
        shotStrength.value = Mathf.Clamp(Vector3.Distance(initialTouchPosition, position), 0, maxStrength.value) * ballPower.value;
    }
}
