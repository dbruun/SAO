namespace Backend.Models;

public enum LivenessStatus { NotPerformed, Passed, Failed }

public class LivenessResult
{
    public LivenessStatus Status { get; set; } = LivenessStatus.NotPerformed;
    public double Score { get; set; } = 0.5;
}
