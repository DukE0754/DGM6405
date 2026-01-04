using System;
using UnityEngine;

/// <summary>
/// Basic menu example that show
/// </summary>
public class SplashMenu : MenuBase
{
    [SerializeField] private Animator Animator;
    
    public void OnShow(Action onAnimationComplete)
    {
        Animator.Play(0);
    }

    /// <summary>
    /// Called by the animation event on the splash animation controller
    /// </summary>
    public void AnimationComplete()
    {
        SceneMgr.Instance.LoadScene(GameScenes.MainMenu, GameMenus.MainMenu);
    }
}
