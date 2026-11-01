using System;

[Flags]
public enum PowerUpSelection { None = 0, Scouter = 1, PhoneDistraction = 2 }

[Serializable]
public sealed class PowerUpSaveData
{
    public int version = 1;
    public int points;
    public string attemptId;
    public string scene;
    public bool rewardClaimed;

    public PowerUpSaveData Copy() => (PowerUpSaveData)MemberwiseClone();
}

public sealed class PowerUpWallet
{
    private PowerUpSaveData state;
    private readonly Action<PowerUpSaveData> save;
    public int Points => state.points;

    public PowerUpWallet(PowerUpSaveData initial, Action<PowerUpSaveData> save)
    {
        state = initial?.Copy() ?? new PowerUpSaveData();
        state.points = Math.Max(0, state.points);
        this.save = save ?? throw new ArgumentNullException(nameof(save));
    }

    public bool TryBeginAttempt(string scene, int price, out string attemptId)
    {
        attemptId = null;
        if (string.IsNullOrWhiteSpace(scene) || price < 0 || price > Points) return false;
        PowerUpSaveData next = state.Copy();
        next.points -= price;
        next.scene = scene;
        next.attemptId = Guid.NewGuid().ToString("N");
        next.rewardClaimed = false;
        Commit(next);
        attemptId = state.attemptId;
        return true;
    }

    public bool TryAwardWin(string attemptId, int amount)
    {
        if (string.IsNullOrEmpty(attemptId) || attemptId != state.attemptId || state.rewardClaimed || amount <= 0) return false;
        PowerUpSaveData next = state.Copy();
        next.points = (int)Math.Min(int.MaxValue, (long)next.points + amount);
        next.rewardClaimed = true;
        Commit(next);
        return true;
    }

    private void Commit(PowerUpSaveData next)
    {
        // Save first: a failed write must not spend or award points in memory.
        save(next.Copy());
        state = next;
    }
}
