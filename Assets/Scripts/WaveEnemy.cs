using UnityEngine;

public class WaveEnemy : MonoBehaviour
{
    public GameManager gameManager;

    bool reportedDeath;

    public void ReportDeath()
    {
        if (reportedDeath)
            return;

        reportedDeath = true;

        if (gameManager != null)
        {
            gameManager.EnemyKilled();
        }
    }
}