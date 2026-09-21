using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lander : MonoBehaviour {

    public static Lander Instance { get; private set; }


    public event EventHandler OnUpForce;
    public event EventHandler OnRightForce;
    public event EventHandler OnLeftForce;
    public event EventHandler OnBeforeForce;
    public event EventHandler OnCoinPickUp;
    public event EventHandler<OnLandedEventArgs> OnLanded;
    public class OnLandedEventArgs: EventArgs {
        public int score;
    }

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount = 10f;

    private void Awake() {
        Instance = this;
        landerRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        OnBeforeForce?.Invoke(this, EventArgs.Empty);

        if(fuelAmount <= 0f) {
            //No fuel
            return;
        }

        if(Keyboard.current.upArrowKey.isPressed ||
           Keyboard.current.leftArrowKey.isPressed ||
           Keyboard.current.rightArrowKey.isPressed) {
            //Pressing any input
            ConsumeFuel();
        }

        if (Keyboard.current.upArrowKey.isPressed) {
            float force = 700f;
            landerRigidbody2D.AddForce(force * transform.up * Time.deltaTime);
            OnUpForce?.Invoke(this, EventArgs.Empty);
        }
        if (Keyboard.current.leftArrowKey.isPressed) {
            float turnSpeed = +100f;
            landerRigidbody2D.AddTorque(turnSpeed * Time.deltaTime);
            OnLeftForce?.Invoke(this, EventArgs.Empty);
        }
        if (Keyboard.current.rightArrowKey.isPressed) {
            float turnSpeed = -100f;
            landerRigidbody2D.AddTorque(turnSpeed * Time.deltaTime);
            OnRightForce?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision2D) {
        if(!collision2D.gameObject.TryGetComponent(out LandingPad landingPad)) {
            Debug.Log("Crashed on the Terrain!");
            return;
        }
        
        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;
        if (relativeVelocityMagnitude > softLandingVelocityMagnitude) {
            //Landed too hard
            Debug.Log("Landed too hard");
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .9f;
        if(dotVector < minDotVector) {
            //Landed too steep
            Debug.Log("Landed too steep");
            return;
        }
        
        Debug.Log("Successful landing");

        float maxScoreAmountLandingAngle = 100f;
        float scoreDotVectorMultiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpeed = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude) * maxScoreAmountLandingSpeed;

        Debug.Log("Landing Angle Score: " + landingAngleScore);
        Debug.Log("Landing Speed Score: " + landingSpeedScore);

        int score = Mathf.RoundToInt((landingSpeedScore + landingAngleScore) * landingPad.GetScoreMultiplier());

        Debug.Log("Score: " + score);
        OnLanded?.Invoke(this, new OnLandedEventArgs {
            score = score,
        });
    }

    private void OnTriggerEnter2D(Collider2D collider2d) {
        //NOTE: The collieder 2d is the collider which this gameobj collide with, NOT the gameobj itself
        if(collider2d.gameObject.TryGetComponent(out FuelPickup fuelPickup)) {
            float addFuelAmmount = 10f;
            fuelAmount += addFuelAmmount;
            fuelPickup.DestroySelf();
        }

        if (collider2d.gameObject.TryGetComponent(out CoinPickup coinPickup)) {
            OnCoinPickUp?.Invoke(this, EventArgs.Empty);
            coinPickup.DestroySelf();
        }
    }
    private void ConsumeFuel() {
        float fuelConsumptionAmount = 1f;
        fuelAmount -= fuelConsumptionAmount * Time.deltaTime;
    }
}