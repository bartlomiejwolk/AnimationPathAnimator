using UnityEngine;
using System.Collections;

public class Settings : ScriptableObject{

    private KeyCode playPauseKey = KeyCode.Space;

    public KeyCode PlayPauseKey {
        get { return playPauseKey; }
        set { playPauseKey = value; }
    }

}
