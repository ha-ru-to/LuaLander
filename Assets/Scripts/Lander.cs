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
        public LandingType landingType;

        public int score;
        public float dotVector;
        public float landingSpeed;
        public float scoreMultiplier;
    }

    public enum LandingType {
        Success,
        WrongLandingArea,
        TooSteepAngle,
        TooFastLanding,
    }

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount;
    private float fuelAmountMax = 10f;

    private void Awake() {
        fuelAmount = fuelAmountMax;

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
            OnLanded?.Invoke(this, new OnLandedEventArgs {
                landingType = LandingType.WrongLandingArea,
                dotVector = 0f,
                landingSpeed = 0f,
                scoreMultiplier = 0,
                score = 0,
            });
            return;
        }
        
        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;
        if (relativeVelocityMagnitude > softLandingVelocityMagnitude) {
            //Landed too hard
            Debug.Log("Landed too hard");
            OnLanded?.Invoke(this, new OnLandedEventArgs {
                landingType = LandingType.TooFastLanding,
                dotVector = 0f,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .9f;
        if(dotVector < minDotVector) {
            //Landed too steep
            Debug.Log("Landed too steep");
            OnLanded?.Invoke(this, new OnLandedEventArgs {
                landingType = LandingType.TooSteepAngle,
                dotVector = dotVector,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });
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
            landingType = LandingType.Success,
            dotVector = dotVector,
            landingSpeed = relativeVelocityMagnitude,
            scoreMultiplier = landingPad.GetScoreMultiplier(),
            score = score,
        });
    }

    private void OnTriggerEnter2D(Collider2D collider2d) {
        //NOTE: collieder2d is the collider which THIS GameObject collide with, NOT the GameObject itself
        if(collider2d.gameObject.TryGetComponent(out FuelPickup fuelPickup)) {
            float addFuelAmmount = 10f;
            fuelAmount += addFuelAmmount;
            if(fuelAmount > fuelAmountMax) {
                fuelAmount = fuelAmountMax;
            }
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

    public float GetFuelAmountNormalied() {
        return fuelAmount / fuelAmountMax;
    }

    public float GetSpeedX() {
        return landerRigidbody2D.linearVelocityX;
    }

    public float GetSpeedY() {
        return landerRigidbody2D.linearVelocityY;
    }

    public float GetFuel() {
        return fuelAmount;
    }
}