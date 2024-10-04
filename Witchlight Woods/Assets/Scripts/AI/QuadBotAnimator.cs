using System;
using UnityEngine;

namespace WitchlightWoods
{
    public class QuadBotAnimator : MonoBehaviour
    {
        public Transform testRoot;
        public Transform fBody;
        public Transform bBody;
        public Limb fr = new ();
        public Limb fl = new ();
        public Limb br = new ();
        public Limb bl = new ();

        public Vector2 feetOffset;

        [Range(0f, 1f)]
        public float t;

        public bool frontSide;
        public float frontTimer;
        public float translateSpeed;
        
        public bool backSide;
        public float backTimer;
        
        public QuadBotStep idle;
        public QuadBotStep walk;
        public QuadBotStep trot;
        public QuadBotStep run;

        private float _fBodyAngle;
        private float _bBodyAngle;
        
        private void Start()
        {
            fr.animator.currentTarget = fr.target;
            fl.animator.currentTarget = fl.target;
            br.animator.currentTarget = br.target;
            bl.animator.currentTarget = bl.target;

            _fBodyAngle = fBody.rotation.eulerAngles.z;
            _bBodyAngle = bBody.rotation.eulerAngles.z;
        }

        private void Update()
        {
            testRoot.Translate(Vector3.right * (translateSpeed * Time.deltaTime * t), Space.World);
            var config = Blend(t, idle, walk, trot, run);
            TryToStep(fr, fl, fBody ?? transform, config.front, ref frontSide, ref frontTimer);
            TryToStep(br, bl, bBody ?? transform, config.back, ref backSide, ref backTimer);
            
            fBody.rotation = Quaternion.Euler(0, 0, _fBodyAngle + config.front.bodyFlange * Mathf.Sin((config.front.bodyFlangeOffset + config.front.bodyFlangeSpeed) * Time.time * Mathf.Deg2Rad));
            bBody.rotation = Quaternion.Euler(0, 0, _bBodyAngle + config.back.bodyFlange * Mathf.Sin((config.back.bodyFlangeOffset + config.back.bodyFlangeSpeed) * Time.time * Mathf.Deg2Rad));
        }

        private void TryToStep(Limb right, Limb left, Transform relativeTo, LimbStepDetails config, ref bool side, ref float time)
        {
            time -= Time.deltaTime;
            
            right.animator.Update();
            left.animator.Update();
            
            if (time <= 0)
            {
                side = !side;
                time = config.switchTime;
            }

            var limb = side ? left : right;
            var stepTarget = (Vector2) relativeTo.position + (side ? config.lTarget : config.rTarget);
            
            if (!limb.animator.moving)
            {
                var currentFeetPosition = (Vector2)limb.target.position;
                var distance = config.maxDistance;
                var speed = config.speed;
                
                distance = Mathf.Lerp(distance, distance * translateSpeed * t, config.scaleWithSpeed);
                speed = Mathf.Lerp(speed, speed * translateSpeed * t, config.scaleWithSpeed);
                
                
                if ((currentFeetPosition - stepTarget).sqrMagnitude >= distance * distance)
                {
                    limb.animator.AnimateStep(currentFeetPosition, stepTarget, config.groundOffset, speed);
                }
            }
        }

        public static QuadBotStep Blend(float t, params QuadBotStep[] steps)
        {
            var min = Mathf.FloorToInt(t * steps.Length);
            var max = Mathf.CeilToInt(t * steps.Length);

            if (min < 0)
                return steps[0];
            
            if (max >= steps.Length)
                return steps[^1];
            
            var from = steps[min];
            var to = steps[max];
            var time = (t * steps.Length) - min;

            return QuadBotStep.Lerp(from, to, time);
        }

        private void OnDrawGizmosSelected()
        {
            var config = Blend(t, idle, walk, trot, run);
            var fPos = fBody == null ? (Vector2)transform.position : (Vector2)fBody.position;
            var bPos = fBody == null ? (Vector2)transform.position : (Vector2)bBody.position;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(fPos + config.front.lTarget, 0.2f);
            Gizmos.DrawWireSphere(bPos + config.back.lTarget, 0.2f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fPos + config.front.rTarget, 0.2f);
            Gizmos.DrawWireSphere(bPos + config.back.rTarget, 0.2f);
        }
    }

    [Serializable]
    public class TargetAnimator
    {
        public bool moving;
        public Vector2 sourcePosition;
        public Vector2 tangentPosition;
        public Vector2 targetPosition;
        public Transform currentTarget;
        public float t;
        public float speed;

        public void Update()
        {
            if (moving)
            {
                if (currentTarget == null)
                {
                    moving = false;
                    return;
                }
                
                t += Time.deltaTime * speed;

                if (t >= 1f)
                    t = 1f;
                
                var a = Vector2.Lerp(sourcePosition, tangentPosition, t);
                var b = Vector2.Lerp(tangentPosition, targetPosition, t);
                var p = Vector2.Lerp(a, b, t);

                currentTarget.position = p;

                if (t >= 1f)
                    moving = false;
            }
        }

        public void AnimateStep(Vector2 from, Vector2 to, float groundOffset, float newSpeed)
        {
            moving = true;
            sourcePosition = from;
            targetPosition = to;
            tangentPosition = ((from + to) / 2f) + new Vector2(0, groundOffset);
            speed = newSpeed;
            t = 0f;
        }
    }

    [Serializable]
    public struct QuadBotStep
    {
        public LimbStepDetails front;
        public LimbStepDetails back;

        public static QuadBotStep Lerp(QuadBotStep from, QuadBotStep to, float t)
        {
            return new QuadBotStep()
            {
                front = LimbStepDetails.Lerp(from.front, to.front, t),
                back = LimbStepDetails.Lerp(from.back, to.back, t)
            };
        }
    }

    [Serializable]
    public struct LimbStepDetails
    {
        [Range(0f, 1f)]
        public float scaleWithSpeed;
        public Vector2 rTarget;
        public Vector2 lTarget;
        [Range(0f, 10f)]
        public float maxDistance;
        [Range(0f, 4f)]
        public float switchTime;
        [Range(0f, 10f)]
        public float speed;
        [Range(0f, 4f)]
        public float groundOffset;
        
        [Range(0f, 90f)]
        public float bodyFlange;
        public float bodyFlangeSpeed;
        public float bodyFlangeOffset;

        public static LimbStepDetails Lerp(LimbStepDetails from, LimbStepDetails to, float t)
        {
            return new LimbStepDetails()
            {
                scaleWithSpeed = Mathf.Lerp(from.scaleWithSpeed, to.scaleWithSpeed, t),
                rTarget = Vector2.Lerp(from.rTarget, to.rTarget, t),
                lTarget = Vector2.Lerp(from.lTarget, to.lTarget, t),
                maxDistance = Mathf.Lerp(from.maxDistance, to.maxDistance, t),
                switchTime = Mathf.Lerp(from.switchTime, to.switchTime, t),
                speed = Mathf.Lerp(from.speed, to.speed, t),
                groundOffset = Mathf.Lerp(from.groundOffset, to.groundOffset, t),
                
                bodyFlange = Mathf.Lerp(from.bodyFlange, to.bodyFlange, t),
                bodyFlangeSpeed = Mathf.Lerp(from.bodyFlangeSpeed, to.bodyFlangeSpeed, t),
                bodyFlangeOffset = Mathf.Lerp(from.bodyFlangeOffset, to.bodyFlangeOffset, t)
            };
        }
    }

    [Serializable]
    public class Limb
    {
        public Transform target;
        public TargetAnimator animator = new ();
    }
}