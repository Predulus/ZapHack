using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public float hoverSpeed = 8f;
    public float hoverAmount = 1f;
    Transform initialTransform;
    float verticalMagnitude = 0.001f;
    float horizontalMagnitude = 0.003f;
    float timeScale;
    float initialTime;
    float timePhase;
 
    // Start is called before the first frame update
    void Start()
    {
        verticalMagnitude *= hoverAmount;
        horizontalMagnitude *= hoverAmount;
        initialTransform = transform;
        timeScale = hoverSpeed;
        Random.InitState(Mathf.RoundToInt(Time.time * 1000));
        timePhase = gameObject.GetInstanceID() + Random.Range(0, 10) + (int)gameObject.name[0] + (int)gameObject.name[1];
        //        initialTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = new Vector3(horizontalMagnitude, verticalMagnitude, 0f);
        transform.position = initialTransform.position +
            new Vector3(delta.x * Mathf.Sin(Time.realtimeSinceStartup * timeScale + timePhase),
            delta.y * Mathf.Sin(Time.realtimeSinceStartup * timeScale + timePhase), 0f);

    }
}
