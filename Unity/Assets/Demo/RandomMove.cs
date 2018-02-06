using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour
{
    float nextTimeToAddForce;
    Vector3 velocity;
    void Start()
    {
    }

    public void FixedUpdate()
    {
        if (transform.position.x > Terrain.activeTerrain.terrainData.size.x - 1 && velocity.x > 0) {
            velocity = new Vector3(-velocity.x, velocity.y, velocity.z);
        }
        if (transform.position.z > Terrain.activeTerrain.terrainData.size.z - 1 && velocity.z > 0) {
            velocity = new Vector3(velocity.x, velocity.y, -velocity.z);
        }
        if (transform.position.x < 1 && velocity.x < 0) {
            velocity = new Vector3(-velocity.x, velocity.y, velocity.z);
        }
        if (transform.position.z < 1 && velocity.z < 0) {
            velocity = new Vector3(velocity.x, velocity.y, -velocity.z);
        }
        //rigid.velocity *= 0.99f;
        if(Time.time > nextTimeToAddForce) {
            nextTimeToAddForce = Time.time + Random.Range(1, 4);
            velocity = (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 10);
        }
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
