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
    }
}