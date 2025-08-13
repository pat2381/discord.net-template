using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TicketBot.Common;

namespace TicketBot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    class RequirePermissionsAttribute : PreconditionAttribute
    {
        private readonly GuildPermission[] _permissions;

        public RequirePermissionsAttribute(params GuildPermission[] permissions)
        {
            _permissions = permissions;
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Guild == null)
            {
                return Task.FromResult(PreconditionResult.FromError("This command can only be executed from within server channels."));
            }

            var user = context.User as SocketGuildUser;

            if (user == null)
            {
                return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a server."));
            }

            var missingPermissions = _permissions
                .Where(permission => !user.GuildPermissions.Has(permission))
                .ToList();

            if (missingPermissions.Any() && !Config.Admins.Contains(user.Id.ToString()))
            {
                var missingPermissionNames = string.Join(", ", missingPermissions.Select(permission => permission.ToString()));
                return Task.FromResult(PreconditionResult.FromError($"You must have the following permissions to run this command: {missingPermissionNames}"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
