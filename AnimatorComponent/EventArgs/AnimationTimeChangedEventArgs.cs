/* 
 * Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using System;

public sealed class AnimationTimeChangedEventArgs : EventArgs {

    public float DeltaTime { get; set; }

    public AnimationTimeChangedEventArgs(float deltaTime) {
        DeltaTime = deltaTime;
    }

}

