namespace Common.Grpc;

public interface ICallOutcome
{
    string Section { get; }
    bool Required { get; }
    AppError? Error { get; }
}
public record CallOutcome<T>
(string Section, bool Required, T? Data, AppError? Error): ICallOutcome;