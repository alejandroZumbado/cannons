using UnityEngine;
using UnityEngine.UI;

// Conecta un Slider (0-1) al volumen general. Se agrega a cualquier Slider de UI.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private Slider _slider;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
    }

    void OnEnable()
    {
        // muestra el valor guardado sin disparar onValueChanged (evita un guardado inútil)
        _slider.SetValueWithoutNotify(VolumeSettings.Master);
        _slider.onValueChanged.AddListener(VolumeSettings.SetMaster);
    }

    void OnDisable() => _slider.onValueChanged.RemoveListener(VolumeSettings.SetMaster);
}
