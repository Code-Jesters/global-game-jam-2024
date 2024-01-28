using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MakeMeLaugh.Assets.Scripts.Bird
{
    public class LocationRandomizer
    {
        public static Vector3 GetLocationInProjectedSphere(Vector3 position, float distance)
        {
            Vector3 eulerVectorDirection = UnityEngine.Random.insideUnitSphere;
            Vector3 offsetPosition = position + eulerVectorDirection * distance;
            return offsetPosition;
        }
        
        public static Vector3 GetDemiSphereProjectionRange(Vector3 center, float minimum, float maximum)
        {
            Vector3 eulerVectorDirection = UnityEngine.Random.insideUnitSphere;
            float distance = UnityEngine.Random.Range(minimum, maximum);
            Vector3 offsetPosition = center + eulerVectorDirection * distance;

            if (offsetPosition.y < center.y)
            {
                float difference = center.y - offsetPosition.y;
                offsetPosition.y += difference * 2;
            }
            return offsetPosition;
        }
    }
}