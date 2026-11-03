using System;
using UnityEngine;

namespace Nard.Presentation
{
    /// <summary>One portrait technical layout in board-global-v1. No responsive layout or move policy.</summary>
    public static class BoardGeometry
    {
        public static Vector2 PointBase(int index)
        {
            if (index < 0 || index >= 24) throw new ArgumentOutOfRangeException(nameof(index));
            int column = index < 12 ? 11 - index : index - 12;
            float x = column < 6 ? -4.25f + .65f * column : .5f + .65f * (column - 6);
            return new Vector2(x, index < 12 ? -6.9f : 6.9f);
        }
        public static float Diameter(int count)
        {
            if (count < 1 || count > 15) throw new ArgumentOutOfRangeException(nameof(count));
            return Mathf.Min(.56f, 5.1f / count);
        }
        public static Vector2 CheckerCenter(Vector2 edge, bool top, int ordinal, int count)
        {
            float diameter = Diameter(count);
            if (ordinal < 0 || ordinal >= count) throw new ArgumentOutOfRangeException(nameof(ordinal));
            return edge + Vector2.up * (top ? -1 : 1) * diameter * (ordinal + .5f);
        }
    }
}
