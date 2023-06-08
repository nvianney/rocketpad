using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    enum State {
        PLAYING, WAITING, RESTARTING
    }
    public float waitTime = 1.0f;
    public float restartTime = 5.0f;
    public GameObject text;

    State state = State.PLAYING;
    float time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.WAITING) {
            time += Time.deltaTime;
            if (time >= waitTime) {
                state = State.RESTARTING;
                time = restartTime;
            }
            
        } else if (state == State.RESTARTING) {
            time -= Time.deltaTime;
            text.GetComponent<TMPro.TextMeshProUGUI>().text = "Restarting in " + Mathf.CeilToInt(time);

            if (time <= 0) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void OnRocketExplode() {
        state = State.WAITING;
        time = 0.0f;
    }
}
