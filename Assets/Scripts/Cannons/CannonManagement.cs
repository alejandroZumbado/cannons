using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonManagement : MonoBehaviour
{
    private Vector3 mousePosition;
    private bool findBarral = false;
    public bool isSetCannon = false;
    public GameObject deployCannon;
    public GameObject createCannon;
    public GameObject shootPrefab;
    public Transform shootPoint;
    private GameManager manager;
    [Range(0, 10)]
    public float shootSpeed = 10f;
    public int damage = 1;
    public ReceiptCannon receipt;
    public Transform center;

    public void ShootUp(int up) {
        damage += up;
        LvlUpSkin();
        Debug.Log("Cannon damage increased to: " + damage);

    }

    public void LvlUpSkin()
    {
        deployCannon.GetComponentInChildren<LvlUpCannonSkin>().SetLevel(damage);
        createCannon.GetComponentInChildren<LvlUpCannonSkin>().SetLevel(damage);
    }

    public void Shoot()
    {
        //deployCannon.GetComponentInChildren
        GameObject newShoot = Instantiate(shootPrefab, shootPoint.position, shootPoint.rotation);

        Rigidbody balaRigidbody = newShoot.GetComponent<Rigidbody>();

        balaRigidbody.linearVelocity = shootPoint.forward * shootSpeed;
    }

    public void SetManager(GameManager newManager) => manager = newManager;

    public void SetCenter(Transform newCenter) => center = newCenter;

    public void SetReceipt(ReceiptCannon newReceipt) => receipt = newReceipt;

    public int GetDamage() => damage;

    public GameObject getDeployCannon() {
        return deployCannon;
    }

    public GameObject getCreateCannon()
    {
        return createCannon;
    }

    public void SetPositionCannon(Transform center)
    {
        transform.position = center.position;
    }
    public void CenterCannon() {
        SetPositionCannon(center);
    }
    public void UnsetCannon() => isSetCannon = false;

    public void SetCannon() => isSetCannon = true;
    public void SetFindBarral(bool value) => findBarral = value;


    private Vector3 MousePos => Camera.main.WorldToScreenPoint(transform.position);

    private void OnMouseDown()
    {
        mousePosition = Input.mousePosition - MousePos;
    }

    private void OnMouseDrag()
    {
        if (isSetCannon) // deberia dejarse arrastrar aunque ya se haya seteado, validar esta parte primero antes de modificar 
        {
            return;
        }
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePosition);
        
    }

    private void OnMouseUp()
    {
        if (findBarral && receipt)
        {
            receipt.UnsetReceipt();
            receipt.SetCannon(this);
            manager.GameFlow2();
        }
        else
        {
            CenterCannon();
        }
    }
}
