using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using LDtkUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WitchlightWoods.Levels
{
    public class CustomLevel : MonoBehaviour, ILDtkImportedFields
    {
        [SerializedDictionary("LDTK Profile", "Volume Profile")]
        public AYellowpaper.SerializedCollections.SerializedDictionary<PostProcessProfile, VolumeProfile> postProcessProfiles;
        [SerializedDictionary("LDTK Light Level", "Light intensity")]
        public AYellowpaper.SerializedCollections.SerializedDictionary<Light, float> lightLevels;
        public new Collider2D collider;
        public CinemachineCamera follower;
        public CinemachineConfiner2D confiner;
        public Volume scanLinesVolume;
     
        [Header("Compile time resolved")]
        public Biome biome;
        public PostProcessProfile postProcessProfile;
        public Light lightLevel;
        public float lightIntensity = 1f;
        
        public static GameObject Player;
        public static Light2D MainLight;
        
        public void OnLDtkImportFields(LDtkFields fields)
        {
            collider = GetComponent<Collider2D>();
            
            biome = fields.GetEnum<Biome>("Biome");
            postProcessProfile = fields.GetEnum<PostProcessProfile>("PostProcessProfile");
            lightLevel = fields.GetEnum<Light>("Light");
            
            if (confiner != null)
                confiner.BoundingShape2D = collider;
            
            if (postProcessProfiles.TryGetValue(postProcessProfile, out var profile))
            {
                var volumeCollider = scanLinesVolume.GetComponent<BoxCollider>();
                volumeCollider.center = collider.bounds.center - transform.position;
                volumeCollider.size = new Vector3(collider.bounds.size.x, collider.bounds.size.y, 100);
                scanLinesVolume.sharedProfile = profile;
            }
            lightLevels.TryGetValue(lightLevel, out lightIntensity);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                follower.Priority.Value = 10;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                follower.Priority.Value = 11;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                MainLight.intensity = Mathf.Lerp(MainLight.intensity, lightIntensity, Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            if(Player == null)
                Player = GameObject.FindWithTag("Player");
            if(MainLight == null)
                MainLight = GameObject.FindWithTag("Main Light")?.GetComponent<Light2D>();
            if (Player != null) 
                follower.Target.TrackingTarget = Player.transform;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}