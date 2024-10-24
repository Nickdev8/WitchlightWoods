﻿using System;
using LDtkUnity;
using LDtkUnity.Editor;
using Pathfinding;
using UnityEngine;
using WitchlightWoods;
using WitchlightWoods.Levels;

namespace Editor
{
    public class LevelPostprocessor : LDtkPostprocessor
    {
        protected override void OnPostprocessLevel(GameObject root, LdtkJson _)
        {
            if (AstarPath.active == null || AstarPath.active.data == null) return;
            var level = root.GetComponent<LDtkComponentLevel>();
            var graphIndex = Array.FindIndex(AstarPath.active.data.graphs, graph => graph is CustomGridLevelGraph levelGraph && levelGraph.ParentGuid == level.Identifier);
            CustomGridLevelGraph graph;
            if (graphIndex < 0)
            {
                try
                {
                    graph = (CustomGridLevelGraph)AstarPath.active.data.AddGraph(typeof(CustomGridLevelGraph));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
                graph.ParentGuid = level.Identifier;
            }
            else
                graph = (CustomGridLevelGraph)AstarPath.active.data.graphs[graphIndex];

            
            var w = Mathf.CeilToInt(level.Size.x);
            var h = Mathf.CeilToInt(level.Size.y);
            
            graph.is2D = true;
            graph.collision.use2D = true;
            graph.center = root.transform.position + new Vector3(w / 2f, h / 2f) + AstarPath.active.transform.position;
            graph.SetDimensions(w, h, 1f);

            graph.erodeIterations = 0;
            graph.collision.collisionCheck = true;
            graph.collision.diameter = 0.49f;
            graph.collision.mask = LayerMask.GetMask("Default", "Climbable");

            graph.showMeshOutline = false;
            graph.showMeshSurface = false;
            graph.showNodeConnections = false;

            graph.penaltyPosition = true;
            graph.penaltyPositionFactor = 1;

            graph.penaltyRaycastPosition = true;
        }

        protected override void OnPostprocessProject(GameObject root)
        {
            var player = root.GetComponentInChildren<Player>();
            if (player != null)
            {
                foreach (var level in root.GetComponentsInChildren<CustomLevel>())
                {
                    level.follower.Target.TrackingTarget = player.transform;
                    level.follower.Priority.Enabled = true;
                    level.follower.Priority.Value = 10;
                }

                player.GetComponentInParent<CustomLevel>().Prioritize();
            }
        }
    }
}