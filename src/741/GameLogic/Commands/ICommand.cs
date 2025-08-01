namespace DarkAges.Library.GameLogic.Commands;

public interface ICommand
{
    void Execute(CommandContext context, string[] args);
}