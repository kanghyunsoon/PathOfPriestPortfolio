using System.Collections;
using UnityEngine;
using TMPro;

public class Dice : MonoBehaviour
{
    Animator animator;
    public GameObject CoinParticle;
    public TextMeshPro CoinText;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(Vector3 pos)
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        transform.position = pos;
        transform.SetParent(Base_Canvas.instance.HOLDER_LAYER(1));

        Base_Manager.Sound.Play(Sound.Effect, "Dice");

        StartCoroutine(CoinBlast());
    }

    IEnumerator CoinBlast()
    {
        for(int i =0; i<10; i++)
        {
            int RandomValue = Random.Range(1, 7);
            CoinText.text = RandomValue.ToString();
            yield return new WaitForSeconds(0.1f);
        }
        animator.SetTrigger("Dice_Open");

        CoinParticle.SetActive(true);
        yield return new WaitForSeconds(1.0f);


        animator.SetTrigger("Dice_Close");
        yield return new WaitForSeconds(0.5f);
        CoinParticle.GetComponent<ParticleSystem>().Stop();
        CoinParticle.SetActive(false);

        Base_Manager.instance.Return_Pool(0.1f, this.gameObject, "Dice");

    }
}
