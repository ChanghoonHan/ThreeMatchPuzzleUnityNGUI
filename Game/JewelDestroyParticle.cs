using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelDestroyParticle : MonoBehaviour
{
    [Header("Set Dynamically")]
    ParticleSystem thisParticleSystem;
    ParticleSystem.MainModule particleSystemMain;

    // Start is called before the first frame update
    void Start()
    {
        thisParticleSystem = GetComponent<ParticleSystem>();
        particleSystemMain = thisParticleSystem.main;
    }

    public void PlayParticle(Vector2 pos, Color color)
    {
        this.transform.localPosition = pos;
        particleSystemMain.startColor = color;
        thisParticleSystem.Play();
    }
}
