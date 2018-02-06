using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SkillInfo
{
    public string animName;
    public GameObject particlePrefab;
}

public class CastSkill : MonoBehaviour
{
    public Animator animator;
    Subscriber subscriber;
    public List<SkillInfo> skills;

    public void OnEnable()
    {
        subscriber = new Subscriber();
        subscriber.Add<int>("ui_use_skill", DoSkill);
    }

    public void DoSkill(int id)
    {
        animator.Play(skills[id].animName);
        GameObject particleObj = Instantiate<GameObject>(skills[id].particlePrefab);
        particleObj.transform.position = transform.position;
        particleObj.transform.rotation = transform.rotation;
        ParticleSystem particle = particleObj.GetComponent<ParticleSystem>();
        if (particle) {
            particle.Play();
            GameObject.Destroy(particleObj,particle.main.duration + 5);
        }
    }
}
