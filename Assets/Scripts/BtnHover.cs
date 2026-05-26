using UnityEngine;
using UnityEngine.EventSystems;

// agrega a cualquier boton: se escala al hacer hover
public class BtnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float speed = 8f;

    private Vector3 _baseScale;
    private Vector3 _targetScale;

    void Awake()
    {
        _baseScale = transform.localScale;
        _targetScale = _baseScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData _) => _targetScale = _baseScale * hoverScale;
    public void OnPointerExit(PointerEventData _)  => _targetScale = _baseScale;
}
