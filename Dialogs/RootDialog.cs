// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    /// <summary>
    /// This is an example root dialog. Replace this with your applications.
    /// </summary>
    public class RootDialog : ComponentDialog
    {
        /// <summary>
        /// QnA Maker initial dialog
        /// </summary>
        private const string InitialDialog = "initial-dialog";
        private readonly string _connectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDialog"/> class.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        public RootDialog(IBotServices services, IConfiguration configuration)
            : base("root")
        {
            _connectionName = configuration.GetSection("ConnectionName")?.Value;
            if (string.IsNullOrWhiteSpace(_connectionName))
            {
                throw new ArgumentException("\"ConnectionName\" is not set in configuration");
            }
            AddDialog(new SsoSkillSignInDialog(_connectionName));
            AddDialog(new QnAMakerBaseDialog(services, configuration));

            AddDialog(new WaterfallDialog(InitialDialog)
               .AddStep(InitialStepSignAsync));

            // The initial child Dialog to run.
            InitialDialogId = InitialDialog;
        }
        
        private async Task<DialogTurnResult> InitialStepSignAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity?.From?.Id;
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();
            var token = await userTokenClient.GetUserTokenAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, null, cancellationToken);
            if (token == null)
            {
                return await stepContext.BeginDialogAsync(nameof(SsoSkillSignInDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);
        
            }
        }

        
    }
}
