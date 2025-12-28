using UnityEngine;

namespace dev.vanHoof.ManusTest.Utils
{
    /// <summary>
    /// Generic Math-Functions.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Returns Random point in Rect.
        /// </summary>
        public static Vector2 RandomPointInRect(Rect rect)
            => new Vector2(
                    Random.Range(rect.xMin, rect.xMax),
                    Random.Range(rect.yMin, rect.yMax)
                );

        /// <summary>
        /// Returns Random Point In Circle.
        /// </summary>
        public static Vector2 RandomPointInCircle(Vector2 center, float radius)
            => center + Random.insideUnitCircle * radius;
    }
}
