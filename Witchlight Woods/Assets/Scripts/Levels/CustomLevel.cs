using System;
using LDtkUnity;
using Unity.Cinemachine;
using UnityEngine;

namespace WitchlightWoods.Levels
{
    public class CustomLevel : MonoBehaviour, ILDtkImportedFields
    {
        public Biome biome;
        public new Collider2D collider;
        public CinemachineCamera follower;
        public CinemachineConfiner2D confiner;
        
        public void OnLDtkImportFields(LDtkFields fields)
        {
            collider = GetComponent<Collider2D>();
            biome = fields.GetEnum<Biome>("Biome");
            if (confiner != null)
                confiner.BoundingShape2D = collider;
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

        private void OnEnable()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) 
                follower.Target.TrackingTarget = player.transform;
        }
    }
}