using System.Collections;
using UnityEngine;

public static class CharacterMotion
{
    public static IEnumerator MoveToTarget(
        Transform objectTransform,
        Vector3 targetPoint,
        float speed,
        float stopDistance,
        bool invertFacing = false)
    {
        FaceTarget(objectTransform, targetPoint, invertFacing);

        while (Vector3.Distance(objectTransform.position, targetPoint) > stopDistance)
        {
            objectTransform.position = Vector3.MoveTowards(
                objectTransform.position,
                targetPoint,
                speed * Time.deltaTime);
            yield return null;
        }

        objectTransform.position = targetPoint;
    }

    public static void FaceTarget(
        Transform objectTransform,
        Vector3 targetPoint,
        bool invertFacing = false)
    {
        if (objectTransform == null) return;

        bool movingRight = targetPoint.x > objectTransform.position.x;
        Vector3 scale = objectTransform.localScale;
        float direction = movingRight ? -1f : 1f;
        scale.x = Mathf.Abs(scale.x) * (invertFacing ? -direction : direction);
        objectTransform.localScale = scale;
    }

    public static void SetUniformScale(Transform objectTransform)
    {
        float scaleX = objectTransform.localScale.x;
        objectTransform.localScale = new Vector3(scaleX, scaleX, scaleX);
    }
}
