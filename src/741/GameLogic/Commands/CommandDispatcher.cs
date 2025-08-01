using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DarkAges.Library.World;
using DarkAges.Library.Graphics;
using DarkAges.Library.Audio;
using DarkAges.Library.UI;
using DarkAges.Library.Core.Events;
using DarkAges.Library.GameLogic.Commands.Handlers;

namespace DarkAges.Library.GameLogic.Commands;

public class CommandDispatcher
{
    private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
    private readonly CommandContext _context;

    public event EventHandler<string>? MessageDisplayed;

    public CommandDispatcher(CommandContext context)
    {
        _context = context;
        RegisterCommands();
    }

    private void RegisterCommands()
    {
        // Register commands here, e.g.:
        _commands["set_tile"] = new SetTileCommand();
        _commands["set_color"] = new SetColorCommand();
        _commands["effect"] = new EffectCommand();
        _commands["motion"] = new MotionCommand();
        _commands["move"] = new MoveCommand();
        _commands["put_item"] = new PutItemCommand();
        _commands["put_monster"] = new PutMonsterCommand();
        _commands["put_human"] = new PutHumanCommand();
        _commands["human_to_monster"] = new HumanToMonsterCommand();
        _commands["sound"] = new SoundCommand();
        _commands["auto_use_skill"] = new AutoUseSkillCommand();
        _commands["aus"] = _commands["auto_use_skill"];
        _commands["set_attr_tracker"] = new SetAttrTrackerCommand();
        _commands["sat"] = _commands["set_attr_tracker"];
        _commands["auto_move"] = new AutoMoveCommand();
        _commands["set_merchant_auto_answer"] = new SetMerchantAutoAnswerCommand();
        _commands["set_timer"] = new SetTimerCommand(this);
        _commands["wait_event"] = new WaitEventCommand();
        var messageCommand = new MessageCommand();
        messageCommand.MessageDisplayed += (sender, message) => DisplayMessage(message);
        _commands["message"] = messageCommand;
        _commands["auto_use_spell"] = new AutoUseSpellCommand();
        _commands["set_gnd_tile"] = new SetGndTileCommand();
        _commands["set_stc_tile"] = new SetStcTileCommand();
        _commands["play_minigame"] = new PlayMinigameCommand();
        _commands["send_fieldmap"] = new SendFieldmapCommand();
        var showCommand = new ShowCommand();
        showCommand.MessageDisplayed += (sender, message) => DisplayMessage(message);
        _commands["show"] = showCommand;
        var hideCommand = new HideCommand();
        hideCommand.MessageDisplayed += (sender, message) => DisplayMessage(message);
        _commands["hide"] = hideCommand;
        _commands["teleport"] = new TeleportCommand();
        _commands["give_experience"] = new GiveExperienceCommand();
        _commands["spawn"] = new SpawnCommand();
        var weatherCommand = new WeatherCommand();
        weatherCommand.MessageDisplayed += (sender, message) => DisplayMessage(message);
        _commands["weather"] = weatherCommand;
        var timeCommand = new SetTimeCommand();
        timeCommand.MessageDisplayed += (sender, message) => DisplayMessage(message);
        _commands["time"] = timeCommand;
    }

    public void Dispatch(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return;

        var parts = commandLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var commandName = parts[0];
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var command))
        {
            try
            {
                command.Execute(_context, args);
            }
            catch (Exception ex)
            {
                DisplayMessage($"Command error: {ex.Message}");
            }
        }
        else
        {
            DisplayMessage($"Unknown command: {commandName}");
        }
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}