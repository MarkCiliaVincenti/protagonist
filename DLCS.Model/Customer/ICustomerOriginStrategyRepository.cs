﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DLCS.Core.Types;
using DLCS.Model.Assets;

namespace DLCS.Model.Customer
{
    public interface ICustomerOriginStrategyRepository
    {
        /// <summary>
        /// Get all available origin strategies for customer.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public Task<IEnumerable<CustomerOriginStrategy>> GetCustomerOriginStrategies(int customer);
        
        /// <summary>
        /// Get <see cref="CustomerOriginStrategy"/> for specified <see cref="Asset"/>.
        /// </summary>
        /// <param name="assetId">Id of asset to get <see cref="CustomerOriginStrategy"/> for.</param>
        /// <param name="origin">asset origin.</param>
        /// <returns><see cref="Asset"/> to use for <see cref="Asset"/>.</returns>
        public Task<CustomerOriginStrategy> GetCustomerOriginStrategy(AssetId assetId, string origin);
    }
}