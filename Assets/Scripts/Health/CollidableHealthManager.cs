using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using PhysX;

public class CollidableHealthManager : HealthManager, ICollisionEnterEvent
{
    public bool requiresData { get { return true; } }

    public void CollisionEnter() {}

    [Serializable]
    public struct CollisionArea {
        public bool show;
        public Vector3 rotationEuler;

        [HideInInspector]
        public Quaternion rotation;
        public float width;
        public float height;
        public float collisionResistance;
    }

    DriverCrashDetector driverCrashDetector;

    public float defaultCollisionResistance = 1;
    public GameObject audioSourcePrefab = null;
    public float crashSoundsSmallDamageThreshold = 5f;
    public float crashSoundsLargeDamageThreshold = 40f;
    public List<AudioClip> crashSoundsSmall = new List<AudioClip>();
    public List<AudioClip> crashSoundsLarge = new List<AudioClip>();
    public float crashMasterVolume = 1f;

    protected float deathForce = Mathf.Pow(10, 6.65f);
    protected float baseCollisionResistance = 1;
    public float environmentCollisionResistance = 1;

    protected HotPotatoManager hotPotatoManager;
    protected bool hasHotPotatoManager =false;

    [Header("Collision area 0 should be front")]
    public List<CollisionArea> collisionAreas = new List<CollisionArea>();

    protected bool resetting = false;
    
    public float rammingDamageMultiplier = 1f;
    
    protected float timeSinceLastRam = 0f;

    protected PhysXRigidBody myRb;

    protected NetworkPlayerVehicle collisionNpv;

    public float maxCollisionRate = 0.2f;
    float collisionTimer = 0f;

    public GameObject collisionSparks;

    protected List<int> markedByTeams = new List<int>();

    public float markedTeamDamageIncrease = 3f;
    protected new void Start(){
        baseCollisionResistance = deathForce / maxHealth;
        myRb = GetComponent<PhysXRigidBody>();
        if(GetComponent<HotPotatoManager>()!=null){
            hasHotPotatoManager = true;
            hotPotatoManager = GetComponent<HotPotatoManager>();
            collisionNpv = GetComponent<NetworkPlayerVehicle>();
            if (collisionNpv != null && !collisionNpv.botDriver) driverCrashDetector = GetComponent<DriverCrashDetector>();
        }
        
        base.Start();
    }

    public GameObject markedGameObjectPrefab;
    protected GameObject markedGameObjectInstance;

    [PunRPC]
    public void MarkTeam_RPC(int team, int driver, int gunner){
        markedByTeams.Add(team);
        if(PhotonNetwork.LocalPlayer.ActorNumber == driver || PhotonNetwork.LocalPlayer.ActorNumber == gunner){
            markedGameObjectInstance = Instantiate(markedGameObjectPrefab, transform.position, transform.rotation);
            markedGameObjectInstance.transform.parent = GetComponentInChildren<TeamNameBehaviour>().transform;
        }
    }

    [PunRPC]
    public void RemoveMarkedTeam_RPC(int team, int driver, int gunner){
        markedByTeams.Remove(team);
        if(PhotonNetwork.LocalPlayer.ActorNumber == driver || PhotonNetwork.LocalPlayer.ActorNumber == gunner){
            Destroy(markedGameObjectInstance);
        }
    }

    protected void Update(){
        timeSinceLastRam += Time.deltaTime;
        collisionTimer -= Time.deltaTime;
    }

    

    public void CollisionEnter(PhysXCollision collision) {
        // Debug.Log("crashed into: " + collision.gameObject);
        float dSpeed = myRb.velocity.magnitude;

        float impulse = collision.impulse.magnitude;

        if(collision.rigidBody != null){
           dSpeed = (myRb.velocity - collision.rigidBody.velocity).magnitude;
        }
        if (myPhotonView.IsMine && collision.contactCount > 0 && dSpeed > 1.5 && collisionTimer < 0) {
            collisionTimer = maxCollisionRate;
            Vector3 collisionNormal = collision.GetContact(0).normal;
            Vector3 collisionForce = collision.impulse;
            if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
            collisionForce /= Time.fixedDeltaTime;
            collisionForce = transform.InverseTransformDirection(collisionForce);

            VehicleHealthManager otherVehicleManager = collision.gameObject.GetComponent<VehicleHealthManager>();

            Vector3 collisionPoint = Vector3.zero;
            for (int i = 0; i < collision.contactCount; i++) {
                collisionPoint += collision.GetContact(i).point;
            }
            collisionPoint /= collision.contactCount;

            Vector3 contactDirection = transform.InverseTransformPoint(collisionPoint);
            float damage = CalculateCollisionDamage(collisionForce, contactDirection, otherVehicleManager != null);
           
            // instantiate damage sound over network
            if((damage > crashSoundsSmallDamageThreshold || (otherVehicleManager!=null) ) && timeSinceLastRam > 0.25f) PlayDamageSoundNetwork(damage);

            damage = damage / rammingDamageResistance;

            if (myPhotonView.IsMine && hasHotPotatoManager && otherVehicleManager != null  && timeSinceLastRam > 0.25f){
                if (collisionNpv.GetDriverID() == PhotonNetwork.LocalPlayer.ActorNumber || collisionNpv.GetGunnerID() == PhotonNetwork.LocalPlayer.ActorNumber){
                    // Debug.LogError("Slow down should happen");
                    hotPotatoManager.SlowedCollision();
                }
                if(collisionSparks!=null){
                    GameObject a = Instantiate(collisionSparks, collisionPoint, Quaternion.identity);
                    a.transform.parent = transform;
                }

                if (collisionNpv != null && !collisionNpv.botDriver) {
                    if(damage > 4) driverCrashDetector.CrashCollisionCamera(collision, false);
                    else driverCrashDetector.CrashCollisionCamera(collision, true);
                }
            }
            
            if(damage > 5){
                if (otherVehicleManager != null) {
                    
                    
                    if(otherVehicleManager!=null)damage  *= otherVehicleManager.rammingDamageMultiplier;
                    Weapon.WeaponDamageDetails rammingDetails = otherVehicleManager.rammingDetails;
                    
                    rammingDetails.damage = damage;
                    if(markedByTeams.Contains(rammingDetails.sourceTeamId)) rammingDetails.damage *= markedTeamDamageIncrease;
                    
                    TakeDamage(rammingDetails);
                }
                else {
                    TakeDamage(damage);
                }
            }
            
        }
        if(collision.rigidBody!=null) timeSinceLastRam= 0f;
    }

    protected IEnumerator ResetPreviousCOM(Vector3 com, float t)
    {
        yield return new WaitForSeconds(t);
        GetComponent<Rigidbody>().centerOfMass = com;
        resetting = false;
    }

    protected float CalculateCollisionDamage(Vector3 collisionForce, Vector3 collisionDirection, bool hitVehicle) {
        float collisionResistance = defaultCollisionResistance;

        foreach (CollisionArea collisionArea in collisionAreas) {
            Vector3 verticalComponent = Vector3.ProjectOnPlane(collisionDirection, collisionArea.rotation * Vector3.right).normalized;
            Vector3 horizontalComponent = Vector3.ProjectOnPlane(collisionDirection, collisionArea.rotation * Vector3.up).normalized;
            Vector3 areaCentre = collisionArea.rotation * Vector3.forward;

            if (Vector3.Dot(areaCentre, verticalComponent) > Mathf.Cos(collisionArea.height / 2) &&
                Vector3.Dot(areaCentre, horizontalComponent) > Mathf.Cos(collisionArea.width / 2)) {

                collisionResistance = collisionArea.collisionResistance;
                break;
            }
        }

        float reducedForce = collisionForce.magnitude / baseCollisionResistance;
        if (!hitVehicle) reducedForce /= environmentCollisionResistance;
        reducedForce /= collisionResistance;

        return reducedForce;
    }


    [PunRPC]
    protected void PlayDamageSoundNetwork(float damage)
    {
        if (audioSourcePrefab != null) {
            GameObject crashSound = Instantiate(audioSourcePrefab, transform.position, Quaternion.identity);
            AudioSource a = crashSound.GetComponent<AudioSource>();
            if (damage > crashSoundsLargeDamageThreshold && crashSoundsLarge.Count > 0)
            {
                int randInt = Random.Range(0, crashSoundsLarge.Count - 1);
                a.clip = crashSoundsLarge[randInt];
            }
            else if(crashSoundsSmall.Count > 0)
            {
                int randInt = Random.Range(0, crashSoundsSmall.Count - 1);
                a.clip = crashSoundsLarge[randInt];
            }

            if (a.clip != null)
            {
                a.Play();
                Destroy(crashSound, a.clip.length);
            }
            else
            {
                Debug.LogError("audioSourcePrefab has no AudioSource");
                Destroy(gameObject);
            }
        }
    }
}
