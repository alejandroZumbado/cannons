using UnityEngine;

// "Salto" de tamaño corto (crece y vuelve) para remarcar un evento, p. ej. una
// fusión de cañones (2026-10-20). Recuerda la escala original la primera vez,
// así repetirlo seguido nunca deja el objeto agrandado.
public class PunchScale : MonoBehaviour
{
    const float Duration = 0.3f;
    const float Peak = 1.35f; // tamaño máximo relativo

    Vector3 baseScale;
    float age = Duration;     // terminado: no anima hasta el primer Play

    public static void Play(Transform target)
    {
        var punch = target.GetComponent<PunchScale>();
        if (punch == null)
        {
            punch = target.gameObject.AddComponent<PunchScale>();
            punch.baseScale = target.localScale;
        }
        punch.age = 0f;
    }

    void Update()
    {
        if (age >= Duration) return;
        age = Mathf.Min(age + Time.deltaTime, Duration);
        // sube y baja en forma de seno: 1 -> Peak -> 1
        float factor = 1f + (Peak - 1f) * Mathf.Sin(Mathf.PI * age / Duration);
        transform.localScale = baseScale * factor;
    }
}
