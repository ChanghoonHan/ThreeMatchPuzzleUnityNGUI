using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject particlePrefab;
    public int maxParticleCount = 80;

    [Header("Set Dynamically")]
    [SerializeField]
    private Queue<GameObject> particleQueue;

    // Start is called before the first frame update
    void Start()
    {
        particleQueue = new Queue<GameObject>();

        GameObject tempParticle;

        for (int i = 0; i < maxParticleCount; i++)
        {
            tempParticle = Instantiate(particlePrefab);
            tempParticle.transform.parent = this.transform;
            tempParticle.transform.localScale = Vector3.one;
            particleQueue.Enqueue(tempParticle);
        }
    }

    public void PlayParticle(Vector2 pos, Color color)
    {
        GameObject tempParticle = particleQueue.Dequeue();

        tempParticle.GetComponent<JewelDestroyParticle>().PlayParticle(pos, color);
        particleQueue.Enqueue(tempParticle);
    }
}
