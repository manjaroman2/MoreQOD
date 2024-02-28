using System;
using UnityEngine;

namespace MoreQOD
{
    [RequireComponent(typeof(LineRenderer))]
    public class GUI_XpRange : MonoBehaviour
    {
        [Range(0, 50)] public int segments;
        [Range(0, 5)] public float xradius = 5;
        [Range(0, 5)] public float yradius = 5;
        
        private LineRenderer line;

        private void Start()
        {
            line = gameObject.GetComponent<LineRenderer>();
            line.positionCount = segments + 1;
            line.useWorldSpace = true;
            // float z;
            // CreatePoints();
        }

        private void Update()
        {
            float angle = 20f;
            for (int i = 0; i < (segments + 1); i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;
                line.SetPosition(i, new Vector3(x, y, 0));

                angle += (360f / segments);
            }
        }

        private void OnDrawGizmos()
        {
            throw new NotImplementedException();
        }
    }
}