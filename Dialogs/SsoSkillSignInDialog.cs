// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class SsoSkillSignInDialog : ComponentDialog
    {
        object connection_name = null;
        public SsoSkillSignInDialog(string connectionName)
            : base(nameof(SsoSkillSignInDialog))
        {
            this.connection_name = connectionName;
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt), new OAuthPromptSettings
            {
                ConnectionName = connectionName,
                Text = "Sign In to with your AD account",
                Title = "Sign In",
                EndOnInvalidMessage = true
            }));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SignInStepAsync, DisplayTokenAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SignInStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            // This prompt won't show if the user is signed in to the root using SSO.
            return await context.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            if (!(context.Result is TokenResponse result))
            {
                await context.Context.SendActivityAsync("Please Sign-In to continue. Type 'signin' to generate new sign-in card.", cancellationToken: cancellationToken);
            }
            else
            {
                await context.Context.SendActivityAsync("You're Signed In. Type 'signout' to Sign Out from the bot", cancellationToken: cancellationToken);
            }

            return await context.EndDialogAsync(null, cancellationToken);
        }
    }
}
