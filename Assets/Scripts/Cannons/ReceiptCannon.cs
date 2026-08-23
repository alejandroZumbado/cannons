using UnityEngine;

// Representa un slot de cañon en el tablero (una de las 5 columnas del frente).
// Es la unica fuente de verdad sobre que cañon ocupa este slot.
public class ReceiptCannon : MonoBehaviour
{
    public Transform parent;        // punto de anclaje donde se posiciona el cañon colocado
    public CannonManagement cannon; // cañon que ocupa este slot, o null si esta vacio

    private Collider hitbox;

    private void Awake()
    {
        hitbox = GetComponent<Collider>();
        // un cañon colocado queda centrado exactamente sobre este collider (mismo punto).
        // se desactiva para que no intercepte el click/drag destinado al cañon, pero
        // GetBounds() sigue funcionando porque .bounds no depende de "enabled"
        hitbox.enabled = false;
    }

    // area ocupada por este slot, en coordenadas de mundo
    public Bounds GetBounds() => hitbox.bounds;

    // libera este slot (se llama cuando su cañon se muda a otro slot)
    public void Clear() => cannon = null;

    // coloca newCannon en este slot.
    // devuelve true si hubo un cambio real (consume turno),
    // false si fue un no-op (soltado sobre el mismo slot donde ya estaba)
    public bool PlaceCannon(CannonManagement newCannon)
    {
        bool changed = cannon != newCannon;

        if (changed)
        {
            // si newCannon ya estaba colocado en otro slot, ese slot queda vacio
            if (newCannon.currentSlot != null)
                newCannon.currentSlot.Clear();

            if (cannon != null) // merge: el cañon nuevo absorbe el daño del que ya estaba aqui
            {
                newCannon.ShootUp(cannon.GetDamage());
                newCannon.GetManager().RemoveCannonInTable(cannon);
                Destroy(cannon.gameObject);
            }

            cannon = newCannon;
            newCannon.currentSlot = this;
            newCannon.GetManager().AddCannonInTable(newCannon);
        }

        newCannon.PlaceInSlot(this);
        return changed;
    }
}
