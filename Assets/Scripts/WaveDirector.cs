// WaveDirector.cs
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    public EnemySpawner spawner;
    [Header("Scaling Curves vs minutes")]
    public AnimationCurve enemiesPerWave = AnimationCurve.Linear(0, 4, 10, 35);
    public AnimationCurve enemyHealthMult = AnimationCurve.Linear(0, 1, 10, 3);
    public float waveInterval = 2f;

    float timer;

    void Update()
    {
        if (GameLoop.I == null || GameLoop.I.State != GameState.Playing) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            float tMin = GameLoop.I.Elapsed / 60f;
            int count = Mathf.RoundToInt(enemiesPerWave.Evaluate(tMin));
            float hpMult = enemyHealthMult.Evaluate(tMin);

            spawner.SpawnWaveScaled(count, hpMult);
            timer = waveInterval;
        }
    }
}
