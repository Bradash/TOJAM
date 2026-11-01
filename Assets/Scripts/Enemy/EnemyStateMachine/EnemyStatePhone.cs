using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyStatePhone : EnemyState
{
    public override string StateName => "Answering phone";
    private readonly StorePhone phone;
    private readonly NavMeshPath path;
    private bool answered;
    private float finishAt;

    public EnemyStatePhone(EnemyAI enemy, StorePhone phone, NavMeshPath path) : base(enemy)
    {
        this.phone = phone;
        this.path = path;
    }

    public override void Enter()
    {
        Agent.isStopped = false;
        Agent.speed = Enemy.searchSpeed;
        Agent.acceleration = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.65f;
        Agent.SetPath(path);
    }

    public override void Tick()
    {
        if (Time.timeScale <= 0 || (GameManager.Instance != null && GameManager.Instance.GameOver)) return;
        if (!Agent.enabled || !Agent.isOnNavMesh)
        {
            if (phone != null) phone.ReleaseClerk(Enemy);
            return;
        }
        if (phone == null || !phone.HasCallFor(Enemy))
        {
            Enemy.GoToWander();
            return;
        }
        if (!answered)
        {
            if (!Agent.pathPending && Agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                Enemy.GoToWander();
                return;
            }
            if (!HasReachedDestination()) return;
            if (!phone.TryAnswer(Enemy)) { Enemy.GoToWander(); return; }
            answered = true;
            Agent.isStopped = true;
            Vector3 facing = phone.transform.position - EnemyTransform.position;
            facing.y = 0;
            if (facing.sqrMagnitude > 0.001f) EnemyTransform.rotation = Quaternion.LookRotation(facing);
            PowerUpCatalog settings = PowerUpProgress.Catalog;
            finishAt = Time.time + (settings != null ? Mathf.Max(1, settings.phoneAnswerDuration) : 6);
        }
        if (Time.time >= finishAt) Enemy.GoToWander();
    }

    public override void Exit()
    {
        if (phone != null) phone.ReleaseClerk(Enemy);
        if (Agent.enabled && Agent.isOnNavMesh) Agent.isStopped = false;
        Enemy.CurrentChaseSpeed = Enemy.chaseBaseSpeed;
    }
}
