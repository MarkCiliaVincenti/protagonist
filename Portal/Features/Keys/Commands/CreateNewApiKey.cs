﻿using System.Threading;
using System.Threading.Tasks;
using API.Client;
using API.Client.JsonLd;
using MediatR;

namespace Portal.Features.Keys.Commands
{
    /// <summary>
    /// Create a new API key for current customer.
    /// </summary>
    public class CreateNewApiKey : IRequest<ApiKey>
    {
    }
    
    public class CreateNewApiKeyHandler : IRequestHandler<CreateNewApiKey, ApiKey>
    {
        private readonly IDlcsClient dlcsClient;

        public CreateNewApiKeyHandler(IDlcsClient dlcsClient)
        {
            this.dlcsClient = dlcsClient;
        }
        
        public Task<ApiKey> Handle(CreateNewApiKey request, CancellationToken cancellationToken)
        {
            var newApiKey = dlcsClient.CreateNewApiKey();
            return newApiKey;
        }
    }
}