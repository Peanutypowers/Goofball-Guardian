using System.Collections.Generic;
using UnityEngine;

//mix of mines, found online, and her previous script since I never worked with mocap and stuff
public class ShieldController : MonoBehaviour
{
    public Transform creature;
public Transform trackedProp; // the object with the OptitrackRigidBody component
    public OptitrackStreamingClient client; 
        public int rigidBodyId = 1; // Streaming ID from Motive

    public bool useMouse = false; // press M to switch (for testing without mocap)
        public float distanceFromCreature = 1.6f;
    public float deadzone = 0.06f;
 public bool useZForLeftRight = false;
    public bool flipLeftRight = false;
        public bool flipUpDown = false;

    public float shieldUpTime = 0.35f;
public float cooldown = 0.3f;
    public bool alwaysOn = false; // backup plan if the LED doesn't work
        public int markersWhenLedOff = 3; // how many markers Motive sees on the shield normally
    public float ledSearchRadius = 0.3f;

    public Vector3 propOffset;
        public int markersNearShield;
    public bool ledOn;
 public bool shieldUp;
    public bool buttonPressed;

    float angle = 90f;
    Vector3 center;
        float shieldUpTimer;
    float cooldownTimer;
    bool wasHeld;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    Calibrate();
    }

    // saves where the player is holding the prop right now as the middle
    public void Calibrate()
    {
       if (trackedProp != null) center = trackedProp.position;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) useMouse = !useMouse;
      if (Input.GetKeyDown(KeyCode.C)) Calibrate();

        // which direction is the player pointing?
        Vector2 dir;
        float minDistance;
    if (useMouse || trackedProp == null)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dir = mouse - creature.position;
             minDistance = 0.2f;
        }
        else
        {
            propOffset = trackedProp.position - center;
        dir.x = useZForLeftRight ? propOffset.z : propOffset.x;
            dir.y = propOffset.y;
            if (flipLeftRight) dir.x = -dir.x;
          if (flipUpDown) dir.y = -dir.y;
            minDistance = deadzone;
        }

        // turn that direction into an angle and move the shield around the creature
        if (dir.magnitude > minDistance)
        {
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, 15f * Time.deltaTime);
        }
        float rad = angle * Mathf.Deg2Rad;
        transform.position = creature.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * distanceFromCreature;
     transform.rotation = Quaternion.Euler(0, 0, angle);

        // check the button
        ledOn = CheckLed();
        bool held = ledOn || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
    buttonPressed = held && !wasHeld;
        wasHeld = held;

        shieldUpTimer -= Time.deltaTime;
    cooldownTimer -= Time.deltaTime;
        if (buttonPressed && cooldownTimer <= 0)
        {
            shieldUpTimer = shieldUpTime;
        cooldownTimer = shieldUpTime + cooldown;
        }
        shieldUp = alwaysOn || shieldUpTimer > 0;

    // change color so the player can see when the shield is up
        if (shieldUp) sr.color = Color.cyan;
        else if (cooldownTimer > 0) sr.color = Color.gray;
     else sr.color = new Color(0.4f, 0.5f, 1f, 0.5f);
    }
    // The LED shows up in motive as one extra marker next to the shield
    bool CheckLed()
    {
        markersNearShield = 0;
    if (client == null) return false;

        OptitrackRigidBodyState rb = client.GetLatestRigidBodyState(rigidBodyId);
        List<OptitrackMarkerState> markers = client.GetLatestMarkerStates();
    if (rb == null || markers == null) return false;

        foreach (OptitrackMarkerState m in markers)
        {
            if (Vector3.Distance(m.Position, rb.Pose.Position) < ledSearchRadius)
              markersNearShield++;
        }
        return markersNearShield > markersWhenLedOff;
    }
    // projectiles
    public bool CanBlock(Vector3 pos)
    {
        if (!shieldUp) return false;

        Vector3 offset = pos - creature.position;
    if (Mathf.Abs(offset.magnitude - distanceFromCreature) > 0.35f) return false; 

        float projectileAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        return Mathf.Abs(Mathf.DeltaAngle(projectileAngle, angle)) < 35f; 
    }
}