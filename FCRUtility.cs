using UnityEngine;

namespace Rumi.FixCameraResolutions
{
    public static class FCRUtility
    {
        public static Vector2 Multiply(this Vector2 position, float x, float y) => new(position.x * x, position.y * y);
    }
}
