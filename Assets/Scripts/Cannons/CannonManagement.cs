using UnityEngine;

public class CannonManagement : MonoBehaviour
{
    private Vector3 mousePosition;
    public GameObject deployCannon;
    public GameObject createCannon;
    public GameObject shootPrefab;
    public Transform shootPoint;
    private GameManager manager;
    [Range(0, 10)]
    public float shootSpeed = 10f;
    public int damage = 1;
    public ReceiptCannon currentSlot; // slot donde esta colocado, o null si nunca se coloco
    public Transform center;          // posicion a la que vuelve si se suelta fuera de un slot valido
    private Collider hitbox;
    private Vector3 deployCannonLocalPos; // posicion local "correcta" del modelo grande respecto al root
    private Plane dragPlane; // plano horizontal a la altura del cañón al agarrarlo, usado durante el drag

    private void Awake()
    {
        hitbox = GetComponent<Collider>();
        deployCannonLocalPos = deployCannon.transform.localPosition;
    }

    // area ocupada por el cañon, en coordenadas de mundo (usada para detectar sobre que slot se suelta)
    public Bounds GetBounds() => hitbox.bounds;

    public void ShootUp(int up)
    {
        damage += up;
        LvlUpSkin();
    }

    public void LvlUpSkin()
    {
        deployCannon.GetComponentInChildren<LvlUpCannonSkin>().SetLevel(damage);
        createCannon.GetComponentInChildren<LvlUpCannonSkin>().SetLevel(damage);
    }

    public void Shoot()
    {
        GameObject newShoot = Instantiate(shootPrefab, shootPoint.position, shootPoint.rotation);
        newShoot.GetComponent<BulletDamage>().SetDamage(damage); // aplica el damage real del cañon (merges incluidos)
        newShoot.GetComponent<Rigidbody>().linearVelocity = shootPoint.forward * shootSpeed;
    }

    public void SetManager(GameManager newManager) => manager = newManager;

    public GameManager GetManager() => manager;

    public void SetCenter(Transform newCenter) => center = newCenter;

    public int GetDamage() => damage;

    public void SetPositionCannon(Transform newCenter) => transform.position = newCenter.position;

    public void CenterCannon() => SetPositionCannon(center);

    // muestra el icono pequeño (estado durante el arrastre, sea un cañon nuevo o ya colocado)
    public void ShowIcon()
    {
        createCannon.SetActive(true);
        deployCannon.SetActive(false);
        deployCannon.transform.localPosition = deployCannonLocalPos; // limpia cualquier preview previo
    }

    // muestra el modelo grande y centra el cañon en el slot dado
    public void PlaceInSlot(ReceiptCannon slot)
    {
        SetCenter(slot.parent);
        CenterCannon();
        deployCannon.transform.localPosition = deployCannonLocalPos; // restaura el offset correcto (pudo moverse en el preview)
        createCannon.SetActive(false);
        deployCannon.SetActive(true);
    }

    // mientras se arrastra, muestra el modelo grande "fantasma" en el slot bajo el cursor (si hay).
    // reemplaza al icono (no se muestran ambos a la vez, para que no se tapen entre si)
    private void UpdatePreview(ReceiptCannon hoverSlot)
    {
        if (hoverSlot != null)
        {
            createCannon.SetActive(false);
            deployCannon.SetActive(true);
            deployCannon.transform.position = hoverSlot.parent.position;
        }
        else
        {
            ShowIcon();
        }
    }

    // punto en el mundo donde el rayo cámara->mouse cruza dragPlane.
    // reemplaza el viejo truco de Camera.ScreenToWorldPoint(mouse - offset), que
    // congelaba la profundidad (distancia a la camara) del momento del agarre: como
    // el tablero se aleja de la camara en diagonal, cada slot esta a una profundidad
    // distinta, y esa profundidad fija nunca dejaba llegar el cañon a los slots mas
    // lejanos del punto de spawn (solo el mas cercano en profundidad "conectaba").
    private Vector3 GetMouseOnDragPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        dragPlane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    private void OnMouseDown()
    {
        if (!manager.canDrag) return; // ignora agarrar fuera de fase de agarre
        dragPlane = new Plane(Vector3.up, transform.position);
        mousePosition = transform.position - GetMouseOnDragPlane();
        ShowIcon(); // pasa a modo icono mientras se arrastra
    }

    private void OnMouseDrag()
    {
        if (!manager.canDrag) return; // solo permite drag en fase de agarre
        transform.position = GetMouseOnDragPlane() + mousePosition;
        UpdatePreview(manager.FindSlotAt(GetBounds()));
    }

    private void OnMouseUp()
    {
        if (!manager.canDrag) return; // ignora soltar fuera de fase de agarre

        ReceiptCannon targetSlot = manager.FindSlotAt(GetBounds());

        if (targetSlot != null)
        {
            if (targetSlot.PlaceCannon(this))
                manager.GameFlow2(); // hubo colocacion/merge real -> termina el turno
        }
        else if (currentSlot != null)
        {
            PlaceInSlot(currentSlot); // ya estaba colocado, vuelve a su slot
        }
        else
        {
            ShowIcon();
            CenterCannon(); // cañon nuevo soltado afuera -> vuelve al punto de spawn
        }
    }
}
