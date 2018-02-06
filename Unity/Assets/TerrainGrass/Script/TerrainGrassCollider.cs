using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grass
{
    public class TerrainGrassCollider : MonoBehaviour
    {
        public float radius = 1.5f;
        public float force = 1.0f;
        public float threshold = 0.3f;//移动多远就发一次扰动
        //public TerrainGrassManager manager;
        Vector3 lastPos;
        float lastMoveTime;

        public void OnEnable()
        {
            StartCoroutine(StartUpdate());
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator StartUpdate()
        {
            while (true) {
                if((transform.position - lastPos).sqrMagnitude > threshold * threshold) {
                    lastMoveTime = Time.time;
                    lastPos = transform.position;
                    //manager.AddDisturbance(transform.position, radius, force);
                    //EventSystem.Publish("add_disturbance", "grass", transform.position, radius, force);
                    if (TerrainGrassManager.Instance) {
                        TerrainGrassManager.Instance.AddDisturbance(transform.position, radius, force);
                    }
                } else {
                    if(Time.time > lastMoveTime + 1.0f) {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                yield return null;
            }
        }
    }
}