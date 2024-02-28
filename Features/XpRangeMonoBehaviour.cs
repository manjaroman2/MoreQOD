using System;
using MelonLoader;
using UnityEngine;

namespace MoreQOD
{
    [RequireComponent(typeof(LineRenderer))]
    public class XpRangeMonoBehaviour : MonoBehaviour
    {
        public float radius = 5;
        [Range(1, 256)] private int _segments = 128;
        private float _thetaScale = 0.1f;
        private LineRenderer line;

        public int segments
        {
            set
            {
                _segments = value;
                _thetaScale = (2.0f * Mathf.PI) / _segments;
                if (line != null) line.positionCount = _segments;
            }
        }

        private void Start()
        {
            MelonLogger.Msg("XpRange start");
            // line = gameObject.AddComponent<LineRenderer>();
            line = GetComponent<LineRenderer>();
    
            line.material = MoreQOD.Materials["Sprite-Lit-Default (Instance)"];
            // line.material = MoreQualityOfDeath.Materials["Mat_None (Instance)"];
            line.startColor = Color.red;
            line.endColor = Color.red;
            line.startWidth = 2.2f;
            line.endWidth = 2.2f;
            line.positionCount = _segments;
            line.sortingLayerName = "Foreground";
            Update();
        }

        private void Update()
        {
            MelonLogger.Msg("XpRange update");
            int i = 0;
            for (float theta = 0; theta < 2.0f * Mathf.PI; theta += _thetaScale)
            {
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);

                Vector3 pos = new Vector3(x, y, 10);
                line.SetPosition(i, pos);
                i += 1;
            }
        }
    }
}