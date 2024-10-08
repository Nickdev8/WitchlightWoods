using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace WitchlightWoods
{
    public class TooltipController : MonoBehaviour
    {
        [NotNull] public new Camera camera;
        [NotNull] public InputActionReference pointer;
        [SerializeField] private ContactFilter2D filter;
        
        public static TooltipController Instance;

        [NotNull] private readonly Dictionary<int, TooltipUITemplate> _tooltipInstances = new ();
        private bool _shown;
        private bool _worldTooltip;
        private TooltipUITemplate _shownTooltip;
        [NotNull] private RaycastHit2D[] _buffer = new RaycastHit2D[20];

        private void OnEnable()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        [NotNull] public TooltipUITemplate GetTooltipInstance(TooltipUITemplate prefab)
        {
            var id = prefab.GetInstanceID();
            if (_tooltipInstances.TryGetValue(id, out var tooltip))
                return tooltip!;
            _tooltipInstances.Add(id, Instantiate(prefab, transform));
            return _tooltipInstances[id]!;
        }

        public void TryShow([NotNull] ITooltipProvider provider, Vector2 mousePosition, bool world)
        {
            if (_shown) return;
            _worldTooltip = world;
            var instance = GetTooltipInstance(provider.TooltipUITemplate);
            instance.Show(provider, mousePosition, camera);
            provider.UpdateContent(instance);
            _shownTooltip = instance;
            _shown = true;
        }

        public void TryHide(bool world)
        {
            if (!_shown || (world != _worldTooltip)) return;
            _shownTooltip!.Hide();
            _shown = false;
            _worldTooltip = false;
        }

        private void Update()
        {
            var mousePosition = pointer.action!.ReadValue<Vector2>();
            var ray = camera.ScreenPointToRay(mousePosition);

            var hits = Physics2D.Raycast(ray.origin, ray.direction, filter, _buffer);
            if (hits > 0)
            {
                var index = Array.FindIndex(_buffer, 0, hits, hit => hit.transform!.GetComponent<ITooltipProvider>() != null);
                if (index >= 0)
                {
                    var provider = _buffer[index].transform!.GetComponent<ITooltipProvider>();
                    if (_shownTooltip != null && _shownTooltip.Source != provider)
                        TryHide(true);
                    _worldTooltip = true;
                    TryShow(provider!, mousePosition, true);
                }
            }
            else
            {
                TryHide(true);
            }
        }
    }
}