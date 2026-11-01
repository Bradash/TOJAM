public sealed class PhoneCallBudget
{
    public const int MaximumAnswers = 3;
    public int AnsweredCalls { get; private set; }
    public int Remaining => MaximumAnswers - AnsweredCalls;
    public bool Blocked => Remaining == 0;
    public bool Ringing { get; private set; }
    public bool Answered { get; private set; }
    public bool Busy => Ringing || Answered;

    public bool TryRing()
    {
        if (Busy || Blocked) return false;
        Ringing = true;
        return true;
    }

    public bool TryAnswer()
    {
        if (!Ringing) return false;
        Ringing = false;
        Answered = true;
        AnsweredCalls++;
        return true;
    }

    public void EndCall()
    {
        Ringing = Answered = false;
    }
}
