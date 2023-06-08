using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPiece : MonoBehaviour
{
    public float activeTime = 1.2f;
    public float shrinkTime = 0.5f;
    public float finalScaleMultiplier = 0.2f;

    private Vector3 currentScale;
    // Start is called before the first frame update
    void Start()
    {
        currentScale = transform.localScale;
    }

    // Update is called once per frame
    float currentTime = 0.0f;
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime < activeTime) {

        } else if (currentTime < activeTime + shrinkTime) {
            float alpha = (currentTime - activeTime) / shrinkTime;
            transform.localScale = currentScale * (1.0f - alpha) + (currentScale * finalScaleMultiplier) * alpha;
        } else {
            Object.Destroy(gameObject);
        }
        
    }
}
