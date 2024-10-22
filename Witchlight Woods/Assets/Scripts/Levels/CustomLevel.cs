using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        public int levelIndex = -1;
        
        public static GameObject Player;
        public static Light2D MainLight;

        public static List<CustomLevel> levels = new ();
        private static int _activeCameraIndex = -1;
        
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                
            }
        }

        public void Prioritize()
        {
            if(_activeCameraIndex >= 0 && levels.Count > _activeCameraIndex)
                levels[_activeCameraIndex].follower.Priority.Value = 10;
            _activeCameraIndex = levels.IndexOf(this);
            follower.Priority.Value = 11;

            if(Time.timeSinceLevelLoad > 1f && Application.isPlaying)
                Transition().Forget();
        }

        private async UniTaskVoid Transition()
        {
            var rb = Player.GetComponent<Rigidbody2D>();
            var position = rb.position;
            var normal = ((Vector2)collider.bounds.center - position).normalized;
            // rb.MovePosition(collider.ClosestPoint(position) + (normal * 0.5f));
            // await UniTask.Yield(PlayerLoopTiming.Update);
            // Physics2D.simulationMode = SimulationMode2D.Script;
            // if (!Physics2D.Simulate(0.15f)) throw new Exception("Physics can't be simulated from physics callback");
            // Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            // await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
            // await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
            // await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
            Time.timeScale = 0f;
            var cancelled = await UniTask.Delay(600, cancellationToken: destroyCancellationToken, ignoreTimeScale: true).SuppressCancellationThrow();
            Time.timeScale = 1f;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                MainLight.intensity = Mathf.Lerp(MainLight.intensity, lightIntensity, Time.deltaTime);

                //if fully contains player and is inactive
                if (_activeCameraIndex != levelIndex &&
                    collider.bounds.Contains(other.bounds.min) &&
                    collider.bounds.Contains(other.bounds.max))
                {
                    Prioritize();
                }
            }
        }

        private void OnEnable()
        {
            levelIndex = levels.Count;
            levels.Add(this);
            if(Player == null)
                Player = GameObject.FindWithTag("Player");
            if(MainLight == null)
                MainLight = GameObject.FindWithTag("Main Light")?.GetComponent<Light2D>();
            if (Player != null) 
                follower.Target.TrackingTarget = Player.transform;
            confiner.InvalidateBoundingShapeCache();
        }

        private void OnDisable()
        {
            levels.Remove(this);
        }
    }
}