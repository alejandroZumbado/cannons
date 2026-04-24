using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiptCannon : MonoBehaviour
{
    public Transform parent;
    public CannonManagement cannon;    // valor que dicta si ya hay un cannon aqui, al inicio del turno 
    static ReceiptCannon where;       // valor que dicta donde esta el cannon seteado
    private Transform saveParent;

    public void SetCannon(CannonManagement newCannon)
    {
        if (cannon) // ya tengo un cannon y subimos nivel al cannon nuevo
        {
            newCannon.ShootUp(cannon.GetDamage());
            cannon.GetManager().RemoveCannonInTable(cannon);
            newCannon.GetManager().RemoveCannonInTable(newCannon);
            Destroy(cannon.gameObject);
        }

        newCannon.SetReceipt(this);
        newCannon.SetCenter(parent);
        newCannon.GetManager().AddCannonInTable(newCannon);
        newCannon.getDeployCannon().GetComponent<Transform>().parent = saveParent;
        saveParent = null;
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
                saveParent = managementCannon.getDeployCannon().GetComponent<Transform>().parent;
                managementCannon.getDeployCannon().GetComponent<Transform>().parent = null;
                managementCannon.getCreateCannon().SetActive(false);
                managementCannon.getDeployCannon().SetActive(true);
                managementCannon.getDeployCannon().GetComponent<Transform>().position = parent.position;
               
                managementCannon.SetReceipt(this); // enlaza receipt para que OnMouseUp pueda colocar/merge
                managementCannon.SetFindBarral(true);
                managementCannon.getDeployCannon().GetComponent<Transform>().parent = null;
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
                managementCannon.SetReceipt(null); // limpia receipt al salir del trigger
                managementCannon.SetFindBarral(false);

            }

        }
    }
}
