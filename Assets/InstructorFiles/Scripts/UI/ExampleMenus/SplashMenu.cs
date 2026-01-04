using System;
using UnityEngine;

/// <summary>
/// Basic menu example that shows an animation and waits for it to complete.
/// Tells the original caller <see cref="BootloaderMgr"/> that it completes.
/// </summary>
public class SplashMenu : MenuBase
{
    [SerializeField] private Animator _animator;

    private Action _onAnimationComplete;

    public override GameMenus MenuType()
    {
        return GameMenus.Splash;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="onAnimationComplete"></param>
    public void OnShow(Action onAnimationComplete)
    {
        _onAnimationComplete = onAnimationComplete;
        _animator.Play(0);
    }

    /// <summary>
    /// Called by the animation event on the splash animation controller
    /// </summary>
    public void AnimationComplete()
    {
        _onAnimationComplete?.Invoke();
        _onAnimationComplete = null;
    }
}
