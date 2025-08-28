using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonTransformCalculator
{
    public static (Vector3, float) GetStartPointAndSpacing(Transform startPoint, Transform endPoint, int totalSlot, float maxSpacing)
    {
        List<Vector3> positions = new List<Vector3>();

        if (totalSlot <= 1)
        {
            return ((startPoint.position + endPoint.position) / 2, 0);
        }

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        float totalDistance = Vector3.Distance(start, end);

        float requiredSpacing = totalDistance / (totalSlot - 1);
        float actualSpacing = Mathf.Min(requiredSpacing, maxSpacing);

        float newTotalDistance = actualSpacing * (totalSlot - 1);

        float offset = (totalDistance - newTotalDistance) / 2;

        Vector3 direction = (end - start).normalized;

        for (int i = 0; i < totalSlot; i++)
        {
            float distance = offset + i * actualSpacing;
            Vector3 pos = start + direction * distance;
            positions.Add(pos);
        }

        float spacing = actualSpacing;
        positions.Reverse();
        return (positions[0], spacing);
    }
    public static (Vector3, float) GetStartPointAndSpacingReverse(Transform startPoint, Transform endPoint, int totalSlot, float maxSpacing)
    {
        List<Vector3> positions = new List<Vector3>();

        if (totalSlot <= 1)
        {
            return ((startPoint.position + endPoint.position) / 2, 0);
        }

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        float totalDistance = Vector3.Distance(start, end);

        float requiredSpacing = totalDistance / (totalSlot - 1);
        float actualSpacing = Mathf.Min(requiredSpacing, maxSpacing);

        float newTotalDistance = actualSpacing * (totalSlot - 1);

        float offset = (totalDistance - newTotalDistance) / 2;

        Vector3 direction = (end - start).normalized;

        for (int i = 0; i < totalSlot; i++)
        {
            float distance = offset + i * actualSpacing;
            Vector3 pos = start + direction * distance;
            positions.Add(pos);
        }

        float spacing = actualSpacing;
        positions.Reverse();
        return (positions[0], spacing);
    }

}
