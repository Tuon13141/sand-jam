using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class AnimationSupport
{
    public static void Play(this Animation animationComponent, string animName, float speed)
    {
        animationComponent[animName].speed = speed;
        animationComponent.Play(animName);
    }

    public static async UniTask PlayAsync(this Animation animationComponent, string animName, float speed)
    {
        var animationClip = animationComponent.GetClip(animName);
        var lenghClip = animationClip.length;

        animationComponent[animName].speed = speed;
        animationComponent.Play(animName);

        await UniTask.Delay(Mathf.RoundToInt(lenghClip * speed * 1000));
    }

    public static void PlayWithDuration(this Animation animationComponent, string animName, float timePlay)
    {
        var animationClip = animationComponent.GetClip(animName);
        var lenghClip = animationClip.length;

        animationComponent[animName].speed = lenghClip / timePlay;
        animationComponent.Play(animName);
    }

    public static async UniTask PlayWithDurationAsync(this Animation animationComponent, string animName, float timePlay)
    {
        var animationClip = animationComponent.GetClip(animName);
        var lenghClip = animationClip.length;

        animationComponent[animName].speed = lenghClip / timePlay;
        animationComponent.Play(animName);

        await UniTask.Delay(Mathf.RoundToInt(timePlay * 1000));
    }

    public static void PlayReverse(this Animation animationComponent, string animName, float speed = 1)
    {
        var animationClip = animationComponent.GetClip(animName);
        var lenghClip = animationClip.length;

        animationComponent[animName].speed = -1.0f * speed;
        animationComponent[animName].time = lenghClip;
        animationComponent.Play(animName);
    }

    public static async UniTask PlayReverseAsync(this Animation animationComponent, string animName, float speed = 1)
    {
        var animationClip = animationComponent.GetClip(animName);
        var lenghClip = animationClip.length;

        animationComponent[animName].speed = -1.0f * speed;
        animationComponent[animName].time = lenghClip;
        animationComponent.Play(animName);

        await UniTask.Delay(Mathf.RoundToInt(lenghClip * speed * 1000));
    }

    public static void SetupFirstFrameAnimation(this Animation animationComponent, string animName)
    {
        //Debug.LogFormat("SetupFirstFrameAnimation = {0} ({1})", animName, animationClip);

        var animationClip = animationComponent.GetClip(animName);
        if (animationClip == null) return;
        if (animationComponent.isPlaying) animationComponent.Stop();

        animationClip.SampleAnimation(animationComponent.gameObject, 0);
    }

    public static void SetupLastFrameAnimation(this Animation animationComponent, string animName)
    {
        //Debug.LogFormat("SetupLastFrameAnimation = {0} ({1})", animName, animationClip);

        var animationClip = animationComponent.GetClip(animName);
        if (animationClip == null) return;
        if (animationComponent.isPlaying) animationComponent.Stop();

        animationClip.SampleAnimation(animationComponent.gameObject, 1);
    }
}
