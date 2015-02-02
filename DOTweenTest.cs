using DG.Tweening;
using UnityEngine;

public class DOTweenTest : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Application.targetFrameRate = 60;

        transform.DOMove(new Vector3(1, 2, 3), 10);
        //Invoke("Stop", 1);
    }

    private void Stop() {
        transform.DOPause();
    }

    // Update is called once per frame
    void Update() {
        Debug.Log(Time.deltaTime * 1000);
    }
}
