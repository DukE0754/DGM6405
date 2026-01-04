using System.Collections;
using UnityEngine;

public class TestLoadGameOver : MonoBehaviour
{
    [SerializeField] private float _gameOverDelay = 5f;

    private void Start()
    {
        StartCoroutine(GameOverAfterDelay(_gameOverDelay));
    }

    private IEnumerator GameOverAfterDelay(
        float duration)
    {
        var elapsed = 0f;

        // Edge case: instant
        if (duration <= 0f)
        {
            EndGame();
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        EndGame();
    }

    private void EndGame()
    {
        GameMgr.Instance.GameOver();
    }
}
