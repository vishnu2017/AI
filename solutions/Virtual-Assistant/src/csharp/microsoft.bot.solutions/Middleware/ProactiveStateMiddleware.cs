﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Solutions.Model.Proactive;
using static Microsoft.Bot.Solutions.Model.Proactive.ProactiveModel;

namespace Microsoft.Bot.Solutions.Middleware
{
    public class ProactiveStateMiddleware : IMiddleware
    {
        private ProactiveState _proactiveState;
        private IStatePropertyAccessor<ProactiveModel> _proactiveStateAccessor;

        public ProactiveStateMiddleware(ProactiveState proactiveState)
        {
            _proactiveState = proactiveState;
            _proactiveStateAccessor = _proactiveState.CreateProperty<ProactiveModel>(nameof(ProactiveModel));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var proactiveState = await _proactiveStateAccessor.GetAsync(turnContext, () => new ProactiveModel());
            ProactiveData data;
            if (turnContext.Activity.From.Name.Equals("user", StringComparison.InvariantCultureIgnoreCase))
            {
                // only persist the conversation reference if it's from a user
                var userId = turnContext.Activity.From.Id;
                var conversationReference = turnContext.Activity.GetConversationReference();
                if (proactiveState.TryGetValue(userId, out data))
                {
                    data.Conversation = conversationReference;
                }
                else
                {
                    data = new ProactiveData { Conversation = conversationReference };
                }

                proactiveState[userId] = data;
                await _proactiveStateAccessor.SetAsync(turnContext, proactiveState);
                await _proactiveState.SaveChangesAsync(turnContext);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}