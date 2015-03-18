using UnityEngine;

// TODO Add namespace.
public class ReloadLevel : MonoBehaviour {

    private void Reload() {
        Application.LoadLevel(Application.loadedLevel);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Reload();
        }
    }

}