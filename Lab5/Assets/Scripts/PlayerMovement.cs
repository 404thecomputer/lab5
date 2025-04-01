using System;
using System.Xml.Schema;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance {get; private set;}

    public ParticleSystem sprintParticles;

    public float stamina = 100;
    public float maxStamina = 100;

    public float stamRecoverySpeed = 10;
    public Vector3 maxSpeed = new Vector3(100, 100, 0);
    public Vector3 defaultSpeed = new Vector3(100, 100, 0);
    public float adaptSpeed = 0.1f;
    public float decaySpeed = (float)(1 * Math.Pow(10, -3));

    public float dashMult = 5;

    public Vector3 currentSpeed = new Vector3(0, 0, 0);
    private Rigidbody2D thisBody;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        thisBody = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        sprintParticles = transform.GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        float goalSpeedY = 0;
        float goalSpeedX = 0;

        bool xMove = false;
        bool yMove = false;
        if (Input.GetKey(KeyCode.W)) {
            goalSpeedY = defaultSpeed.y;
            yMove = true;
        }
        if (Input.GetKey(KeyCode.S)) {
            goalSpeedY = defaultSpeed.y * -1;
            yMove = true;
        }
        if (Input.GetKey(KeyCode.A)) {
            goalSpeedX = defaultSpeed.x * -1;
            xMove = true;
        }
        if (Input.GetKey(KeyCode.D)) {
            goalSpeedX = defaultSpeed.x;
            xMove = true;
        }
        float xDif = goalSpeedX - currentSpeed.x;
        float yDif = goalSpeedY - currentSpeed.y;
        float xSpeed;
        float ySpeed;
        if (Math.Abs(xDif) < 0.1f) {
            xSpeed = goalSpeedX;
        } else if (goalSpeedX != 0){
            if (Math.Sign(goalSpeedX) == Math.Sign(currentSpeed.x)) {
                if (Math.Abs(goalSpeedX) > Math.Abs(currentSpeed.x)) {
                    xSpeed = currentSpeed.x + (Math.Sign(xDif) * decaySpeed * Time.deltaTime);
                } else {
                    xSpeed = currentSpeed.x + (Math.Sign(xDif) * adaptSpeed * Time.deltaTime);
                }
            } else {
                xSpeed = currentSpeed.x + (Math.Sign(xDif) * adaptSpeed * Time.deltaTime);
            }
        } else {
            xSpeed = currentSpeed.x + (Math.Sign(xDif) * decaySpeed * Time.deltaTime);
        }
        if (Math.Abs(yDif) < 0.1f) {
            ySpeed = goalSpeedY;
        } else if (goalSpeedY != 0){
            if (Math.Sign(goalSpeedY) == Math.Sign(currentSpeed.y)) {
                if (Math.Abs(goalSpeedY) > Math.Abs(currentSpeed.y)) {
                    ySpeed = currentSpeed.y + (Math.Sign(yDif) * decaySpeed * Time.deltaTime);
                } else {
                    ySpeed = currentSpeed.y + (Math.Sign(yDif) * adaptSpeed * Time.deltaTime);
                }
            } else {
                ySpeed = currentSpeed.y + (Math.Sign(yDif) * adaptSpeed * Time.deltaTime);
            }
        } else {
            ySpeed = currentSpeed.y + (Math.Sign(yDif) * decaySpeed * Time.deltaTime);
        }
        currentSpeed = new Vector3(xSpeed, ySpeed, 0);
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (stamina > 10) {
                stamina -= 10;

                audioSource.Play();

                sprintParticles.Play();
                if(xMove && yMove) {
                    currentSpeed = new Vector3(goalSpeedX * dashMult, goalSpeedY *dashMult, 0);
                } else {
                    currentSpeed = new Vector3(1.5f * goalSpeedX * dashMult, 1.5f * goalSpeedY *dashMult, 0);
                }
            }
            //currentSpeed = new Vector3(Math.Max(Math.Min(goalSpeedX + currentSpeed.x, defaultSpeed.x), defaultSpeed.x * -1) * dashMult, Math.Max(Math.Min(goalSpeedY + currentSpeed.y, defaultSpeed.y), defaultSpeed.y * -1) * dashMult, 0);
        }
        if(stamina < maxStamina)
            stamina += stamRecoverySpeed * Time.deltaTime;
        if(stamina > maxStamina) {
            stamina = maxStamina;
        }
        thisBody.MovePosition(gameObject.transform.position + new Vector3(currentSpeed.x * Time.deltaTime, currentSpeed.y * Time.deltaTime, 0));
    }
}
