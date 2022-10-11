using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] RectTransform photo;
    [SerializeField] Transform line;

    [SerializeField] Vector2 inputVector;

    private void Update()
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(line.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(photo, screenPoint, Camera.main, out Vector2 localPosition);

        localPosition.x += photo.sizeDelta.x / 2;
        localPosition.y = photo.sizeDelta.y / 2 - localPosition.y;

        Debug.Log(localPosition);
    }
}
