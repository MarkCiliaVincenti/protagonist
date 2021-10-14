using System;
using System.Collections.Generic;
using System.Linq;
using DLCS.HydraModel;

namespace DLCS.Mock.ApiApp
{
    public static class MockHelp
    {
        public static Customer GetByName(this List<Customer> customers, string name)
        {
            return customers.Single(c => c.Name == name);
        }
        public static AuthService GetByIdPart(this List<AuthService> authServices, string idPart)
        {
            return authServices.Single(a => a.ModelId == idPart);
        }
        public static Role GetByCustAndId(this List<Role> roles, int customerId, string idPart)
        {
            return roles.Single(r => r.CustomerId == customerId && r.ModelId == idPart);
        }

        public static Image MakeImage(string baseUrl, int customerId, int space, string modelId, 
            DateTime created, string? origin, string? initialOrigin,
            int? width, int? height, int? maxUnauthorised,
            DateTime? queued, DateTime? dequeued, DateTime? finished, bool ingesting, string error,
            string[]? tags, string? string1, string? string2, string? string3,
            long? number1, long? number2, long? number3,
            string imageOptimisationPolicy, string thumbnailPolicy)
        {
            var image = new Image(baseUrl, customerId, space, modelId);
            string mockDlcsPathTemplate = string.Format("/{0}/{1}/{2}", customerId, space, modelId);
            image.InfoJson = "https://mock.dlcs.io" + mockDlcsPathTemplate;
            image.DegradedInfoJson = "https://mock.degraded.dlcs.io" + mockDlcsPathTemplate;
            image.ThumbnailInfoJson = "https://mock.thumbs.dlcs.io" + mockDlcsPathTemplate;
            image.Thumbnail400 = "https://mock.thumbs.dlcs.io" + mockDlcsPathTemplate + "/full/400,/0/default.jpg";
            image.Created = created;
            image.Origin = origin;
            image.InitialOrigin = initialOrigin;
            image.Width = width;
            image.Height = height;
            image.MaxUnauthorised = maxUnauthorised;
            image.Queued = queued;
            image.Dequeued = dequeued;
            image.Finished = finished;
            image.Ingesting = ingesting;
            image.Error = error;
            image.Tags = tags;
            image.String1 = string1;
            image.String2 = string2;
            image.String3 = string3;
            image.Number1 = number1;
            image.Number2 = number2;
            image.Number3 = number3;
            image.ImageOptimisationPolicy = imageOptimisationPolicy;
            image.ThumbnailPolicy = thumbnailPolicy;
            return image;
        }
    }
}