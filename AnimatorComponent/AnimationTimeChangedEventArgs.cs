using System;

public sealed class AnimationTimeChangedEventArgs : EventArgs {

    public float DeltaTime { get; set; }

    public AnimationTimeChangedEventArgs(float deltaTime) {
        DeltaTime = deltaTime;
    }

}

