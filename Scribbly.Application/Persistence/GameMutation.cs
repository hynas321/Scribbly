namespace Scribbly.Application.Persistence;

public record GameMutation<T>(T Value, GameMutationAction Action)
{
    public static GameMutation<T> Persist(T value) => new(value, GameMutationAction.Persist);
    public static GameMutation<T> Delete(T value) => new(value, GameMutationAction.Delete);
    public static GameMutation<T> NoChange(T value) => new(value, GameMutationAction.NoChange);
}
