
using UnityEngine;

public class LineController : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                transform.position = new Vector2(transform.position.x, Input.mousePosition.y);
            }
        }
    }
}
