using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiptCannon : MonoBehaviour
{
    public Transform parent;
    public CannonManagement cannon;    // valor que dicta si ya hay un cannon aqui, al inicio del turno 
    static ReceiptCannon where;       // valor que dicta donde esta el cannon seteado


    public void SetCannon(CannonManagement newCannon) {
        if (cannon)
        {
            cannon.ShootUp(newCannon.GetDamage());
        }
        cannon = newCannon;
    }
    public void UnsetCannon() => cannon = null;

    public void UnsetReceipt() => where = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Canon"))
        {
            if (!where)
            {
                where = this;
                CannonManagement managementCannon = other.GetComponent<CannonManagement>();
                managementCannon.getDeployCannon().GetComponent<Transform>().parent = null;
                managementCannon.getCreateCannon().SetActive(false);
                managementCannon.getDeployCannon().SetActive(true);
                managementCannon.getDeployCannon().GetComponent<Transform>().position = parent.position;
                managementCannon.SetFindBarral(true);
                managementCannon.SetReceipt(this);
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Canon"))
        {
            if (this == where)
            {
                where = null;
                CannonManagement managementCannon = other.GetComponent<CannonManagement>();
                managementCannon.getDeployCannon().GetComponent<Transform>().parent = other.transform;
                managementCannon.getCreateCannon().SetActive(true);
                managementCannon.getDeployCannon().SetActive(false);
                Debug.Log("exit");
                managementCannon.SetFindBarral(false);

            }

        }
    }
}
