using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenRocket : MonoBehaviour
{

    public GameObject rocketPiece;
    public GameObject output;
    public float radius;
    public float height;
    public int piecesPerRow;
    public float scale = 1.0f;

    // Start is called before the first frame update
    void Start() 
    {
        int countY = (int) (height / scale);

        // pre-generate
        for (int i = 0; i < countY; i++) {
            float y = (float) i / countY * height;

            for (int j = 0; j < piecesPerRow; j++) {
                float angle = ((float) j / piecesPerRow) * Mathf.PI * 2;

                GameObject piece = Instantiate(
                    rocketPiece,
                    new Vector3(Mathf.Cos(angle) * (radius - scale), y, Mathf.Sin(angle) * (radius - scale)),
                    Quaternion.Euler(0, Mathf.Rad2Deg * angle, 0),
                    output.transform
                );

                piece.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
