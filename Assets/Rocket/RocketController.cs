using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class RocketController : MonoBehaviour
{

    public float thrustForce;
    public float tiltForce;
    public GameObject tiltPosition;
    public GameObject[] thrustParticleObjects;
    public GameObject leftThrustParticleObject;
    public GameObject rightThrustParticleObject;
    public MinMaxRange rangeDeltaX;
    public MinMaxRange rangeDeltaY;
    public GameObject brokenPieces;
    public MinMaxRange landingRangeX;
    public float maxLandingForce;
    public float maxLandingAngle; // degrees
    public float explosionImpulse;
    public float radialImpulse;
    public float randomImpulse;
    public float explosionRadiusMultiplier = 1.0f;

    public UnityEngine.Events.UnityEvent OnSuccess;
    public UnityEngine.Events.UnityEvent OnExplode;

    // Player input
    float thrustAlpha = 0.0f;
    Vector2 tiltDirection;

    Rigidbody body;
    Animator animator;

    ParticleWrapper[] particles;
    ParticleWrapper leftThrustParticle;
    ParticleWrapper rightThrustParticle;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Wrap each particle system under our wrapper so we can easily enable/disable and set power
        particles = new ParticleWrapper[thrustParticleObjects.Length];
        for(int i = 0; i < thrustParticleObjects.Length; i++) {
            GameObject obj = thrustParticleObjects[i];
            particles[i] = new ParticleWrapper(obj.GetComponent<ParticleSystem>());
        }

        leftThrustParticle = new ParticleWrapper(leftThrustParticleObject.GetComponent<ParticleSystem>());
        rightThrustParticle = new ParticleWrapper(rightThrustParticleObject.GetComponent<ParticleSystem>());

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 70) {
            animator.SetBool("isLanding", true);
        }
        if (transform.position.y <= -70) {
            OnExplode.Invoke();
            gameObject.SetActive(false);
        }
    }
     
    void FixedUpdate()
    {
        body.AddRelativeForce(Vector2.up * thrustForce * thrustAlpha);

        body.AddForceAtPosition(new Vector3(tiltDirection.x, 0, tiltDirection.y) * tiltForce, tiltPosition.transform.position);
    }

    void SetThrustParticleEnabled(bool isEnabled, float power = 1.0f) {
        foreach (ParticleWrapper p in particles) {
            p.SetEnabled(isEnabled);
            p.SetPower(power);
        }
    }

    void SetThrustParticlePower(float power) {
        foreach (ParticleWrapper p in particles) {
            p.SetPower(power);
        }
    }

    void SetThrustDirection(ThrustDirection direction, float power = 1.0f) {
        switch(direction) {
            case ThrustDirection.Left:
                leftThrustParticle.SetEnabled(false);

                rightThrustParticle.SetEnabled(true);
                rightThrustParticle.SetPower(power);
                break;
            case ThrustDirection.Right:
                rightThrustParticle.SetEnabled(false);

                leftThrustParticle.SetEnabled(true);
                leftThrustParticle.SetPower(power);
                break;
            case ThrustDirection.None:
                leftThrustParticle.SetEnabled(false);
                rightThrustParticle.SetEnabled(false);
                break;
        }
    }

    void SetThrustDirectionPower(float power) {
        if (power > 0)
        {
            SetThrustDirection(ThrustDirection.Right, power);
        }
        else if (power < 0) {
            SetThrustDirection(ThrustDirection.Left, -power);
        }
        else {
            SetThrustDirection(ThrustDirection.None);
        }
    }

    public void OnThrust(InputValue input)
    {
        // Input action type is set to value, so this method will be called
        // only when there is an update to the input (pressed, released)
        thrustAlpha = input.isPressed ? 1.0f : 0.0f;
        SetThrustParticleEnabled(input.isPressed);
    }

    public void OnMove(InputValue input)
    {
        tiltDirection = input.Get<Vector2>();

        if (tiltDirection.x == -1)
        {
            SetThrustDirection(ThrustDirection.Left);
        }
        else if (tiltDirection.x == 1)
        {
            SetThrustDirection(ThrustDirection.Right);
        }
        else
        {
            SetThrustDirection(ThrustDirection.None);
        }
    }

    public void OnTouch(InputValue input) {
        TouchState t = input.Get<TouchState>();

        // Apply / reset based on touch state
        if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            SetThrustParticleEnabled(true, 0.0f);
        }
        else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            SetThrustParticleEnabled(false);
            SetThrustDirection(ThrustDirection.None);
            tiltDirection = Vector2.zero;
            thrustAlpha = 0.0f;
            return;
        }

        // Set variables so our physics system knows what force to apply
        Vector2 delta = t.position - t.startPosition;
        Vector2 alpha = new Vector2(Mathf.Sign(delta.x) * lerpMinMax(Mathf.Abs(delta.x), rangeDeltaX), lerpMinMax(delta.y, rangeDeltaY));

        // touch x-axis for rotation
        tiltDirection = Vector2.right * alpha.x;

        // touch y-axis for thrust force
        thrustAlpha = Mathf.Clamp01(alpha.y);

        SetThrustDirectionPower(alpha.x);
        SetThrustParticlePower(thrustAlpha);
    }

    float lerpMinMax(float value, MinMaxRange range) {
        return Mathf.Clamp01((value - range.minimum) / (range.maximum-range.minimum));
    }

    bool landed = false;
    void OnCollisionEnter(Collision collision) {
        Vector3 force = collision.impulse / Time.fixedDeltaTime;
        Vector3 angle = transform.rotation.eulerAngles;
        Vector3 point = collision.GetContact(0).point;

        // bottom plate: use position of bottom of rocket.
        GameObject bbase = transform.Find("Base").gameObject;
        if (GameObject.ReferenceEquals(collision.GetContact(0).thisCollider.gameObject, bbase)) {
            print("Base");
            point = bbase.transform.position;
        }

        if (angle.x > 180) angle.x -= 360;

        Debug.Log("Force: " + force);
        Debug.Log("Angle: " + angle);
        Debug.Log("Point: " + point);
        
        if (landed) return;


        if (force.sqrMagnitude >= maxLandingForce * maxLandingForce ||
            Mathf.Abs(angle.x) >= maxLandingAngle ||
            (point.x < landingRangeX.minimum || point.x > landingRangeX.maximum)) {
            landed = true;
            Vector3 velocity = GetComponent<Rigidbody>().velocity;
            GameObject pieces = Instantiate(brokenPieces);
            Vector3 collisionPoint = collision.GetContact(0).point;

            pieces.transform.rotation = transform.rotation;
            pieces.transform.position = transform.position;
            pieces.SetActive(true);
            for (int i = 0; i < pieces.transform.childCount; i++) {
                GameObject p = pieces.transform.GetChild(i).gameObject;
                Vector3 direction = (p.transform.position - point);

                // Newton's law of gravitation
                // 1/(rm^-1)^2
                Vector3 impulse = (direction.normalized / direction.sqrMagnitude) * explosionRadiusMultiplier * explosionRadiusMultiplier * explosionImpulse;

                // Also explode away from center
                Vector3 impulse2 = new Vector3(
                    p.transform.localPosition.x + Random.Range(-0.2f, 0.2f),
                    0,
                    p.transform.localPosition.z + Random.Range(-0.2f, 0.2f)).normalized * radialImpulse;

                // Random
                Vector3 impulse3 = new Vector3(
                    Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)
                ) * randomImpulse;

                Rigidbody pr = p.GetComponent<Rigidbody>();

                pr.AddForce(impulse + impulse2 + impulse3, ForceMode.Impulse);
            }

            if (OnExplode != null) {
                OnExplode.Invoke();
            }

            gameObject.SetActive(false);
        } else {
            landed = true;
            OnSuccess.Invoke();
        }
    }

    enum ThrustDirection {
        Left, None, Right
    }
}
