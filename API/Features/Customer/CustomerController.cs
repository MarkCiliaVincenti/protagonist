using System;
using System.Linq;
using System.Threading.Tasks;
using API.Converters;
using API.Features.Customer.Requests;
using API.Settings;
using DLCS.Core.Strings;
using DLCS.HydraModel;
using DLCS.Web.Auth;
using DLCS.Web.Requests;
using Hydra.Collections;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace API.Features.Customer
{
    /// <summary>
    /// DLCS REST API Operations for customers.
    /// This controller does not do any data access; it creates Mediatr requests and passes them on.
    /// It converts to and from the Hydra form of the DLCS API.
    /// </summary>
    [Route("/customers/")]
    [ApiController]
    public class CustomerController : HydraController
    {
        private readonly IMediator mediator;

        /// <inheritdoc />
        public CustomerController(
            IMediator mediator,
            IOptions<ApiSettings> options) : base(options.Value)
        {
            this.mediator = mediator;
        }
        
        // ################# GET /customers #####################
        /// <summary>
        /// Get all the customers.
        /// Although it returns a paged collection, the page size is always the total number of customers:
        /// clients don't need to page this collection, it contains all customers.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<HydraCollection<JObject>> Index()
        {
            var baseUrl = getUrlRoots().BaseUrl;
            var dbCustomers = await mediator.Send(new GetAllCustomers());
            
            return new HydraCollection<JObject>
            {
                WithContext = true,
                Members = dbCustomers.Select(c => c.ToCollectionForm(baseUrl)).ToArray(),
                TotalItems = dbCustomers.Count,
                PageSize = dbCustomers.Count,
                Id = Request.GetJsonLdId()
            };
        }

        
        // ################# POST /customers #####################
        /// <summary>
        /// The /customers/ path is not access controlled, but only an admin may call this.
        /// </summary>
        /// <param name="newCustomer"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DLCS.HydraModel.Customer newCustomer)
        {
            if (!User.IsAdmin())
            {
                return Forbid();
            }
            
            var basicErrors = HydraCustomerValidator.GetNewHydraCustomerErrors(newCustomer);
            if (basicErrors.Any())
            {
                return HydraProblem(basicErrors, null, 400, "Invalid Customer", null);
            }

            var command = new CreateCustomer(newCustomer.Name!, newCustomer.DisplayName!);

            try
            {
                var result = await mediator.Send(command);
                if (result.Customer == null || result.ErrorMessages.Any())
                {
                    int statusCode = result.Conflict ? 409 : 500;
                    return HydraProblem(result.ErrorMessages, null, statusCode, "Could not create Customer", null);
                }
                var newApiCustomer = result.Customer.ToHydra(getUrlRoots().BaseUrl);
                return Created(newApiCustomer.Id, newApiCustomer);
            }
            catch (Exception ex)
            {
                // Are exceptions the way this info should be passed back to the controller?
                return HydraProblem(ex);
            }
        }
        
        
        // ################# GET /customers/id #####################
        /// <summary>
        /// Get a Customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{customerId}")]
        public async Task<IActionResult> Index(int customerId)
        {
            var dbCustomer = await mediator.Send(new GetCustomer(customerId));
            if (dbCustomer == null)
            {
                return HydraNotFound();
            }
            return Ok(dbCustomer.ToHydra(getUrlRoots().BaseUrl));
        }
        
        
        // ################# GET /customers/id/keys #####################
        [HttpGet]
        [Route("{customerId}/keys")]
        public async Task<IActionResult> GetApiKeys(int customerId)
        {
            var dbCustomer = await mediator.Send(new GetCustomer(customerId));
            if (dbCustomer == null)
            {
                return HydraNotFound();
            }

            var urlRoots = getUrlRoots();
            var collection = new HydraCollection<ApiKey>
            {
                WithContext = true,
                Members = dbCustomer.Keys.Select(
                    key => new ApiKey(urlRoots.BaseUrl, customerId, key, null))
                    .ToArray(),
                TotalItems = dbCustomer.Keys.Length,
                PageSize = dbCustomer.Keys.Length,
                Id = Request.GetJsonLdId()
            };
            return Ok(collection);
        }
        
        
        // ################# POST /customers/id/keys #####################
        [HttpPost]
        [Route("{customerId}/keys")]
        public async Task<IActionResult> CreateNewApiKey(int customerId)
        {
            var result = await mediator.Send(new CreateApiKey(customerId));
            if (result.Key.HasText() && result.Secret.HasText())
            {
                return Ok(new ApiKey(getUrlRoots().BaseUrl, customerId, result.Key, result.Secret));
            }

            return HydraProblem("Unable to create API key", null, 500, "API Key", null);
        }
        
        
    }

}