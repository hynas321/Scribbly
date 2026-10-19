using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Settings;

public record GameSettingsCommandResult(GameSettingsCommandStatus Status, GameSettings? Settings = null);
