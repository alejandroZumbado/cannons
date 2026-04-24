using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateManager : MonoBehaviour
{
    private PirateAnimatorManager animatorManager;
    private GameManager manager;
    [SerializeField]
    [Tooltip("La vida inicial del Enemigo.")]
    [Range(0, 100)]
    private int hp = 5;

    [SerializeField]
    private GameObject barrel;

    [SerializeField]
    private List<Sprite> spritesHp;

    [SerializeField]
    private GameObject uiHp;

    void Start()
    {
        animatorManager = gameObject.GetComponent<PirateAnimatorManager>();
        animatorManager.PlayIdle();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        SetHp(hp);
        animatorManager.PlayGetHit();
        if (hp <= 0)
        {
            manager.RemovePirate(this);
            GetComponentInChildren<PirateTakesDamage>().Delete();
            Invoke(nameof(Delete), 5f);
            animatorManager.PlayFall();
        }
    }

    public void SetHp(int newHp)
    {
        // Set the new hp value
        hp = newHp;
        // Ensure the hp value is within the bounds of the sprite list
        if (hp >= 0 && hp <= spritesHp.Count)
        {
            // Get the SpriteRenderer component of the uiHp GameObject
            SpriteRenderer spriteRenderer = uiHp.GetComponent<SpriteRenderer>();

            // Set the sprite to the corresponding one in the list
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spritesHp[hp];
            }
        }
    }
    public void SetManager(GameManager newManager) => manager = newManager;

    public void SetPositionPirate(Transform center)
    {
        transform.position = center.position;
    }
    public void Advance()
    {
        GameObject nextBarrel = manager.matriz.GetObjectInFrontOf(barrel);
        if (manager.matriz.GetObjectInFrontOf(nextBarrel))
        {
            StartCoroutine(RelocatePirate(nextBarrel));
        }
        else
        {
            animatorManager.PlayAttack();
            manager.GameOver(); // pirata llegó al frente → game over
        }
    }
    IEnumerator RelocatePirate(GameObject nextBarrel)
    {
        animatorManager.PlayAdvance();
        // float f = animatorManager.PirateAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(1.04f);              // hacer algo para que las 2 animaciones tengan su tiempo aqui

        SetBarrel(nextBarrel);
        SetPositionPirate(nextBarrel.transform.GetChild(0).gameObject.transform);
        
    }
    public void Delete() => Destroy(gameObject);

    public GameObject Barrel => barrel;

    public void SetBarrel(GameObject newBarrel) => barrel = newBarrel;
}
