﻿# Orchestrator

Orchestrator is a reverse-proxy that handles requests for all assets (image, timebased, files), description resources (info.json, single item manifests and named-query projections) and PDF generation.

## Technology :robot:

* [YARP](https://microsoft.github.io/reverse-proxy/) - native dotnet reverse-proxy.
* [Dapper](https://github.com/DapperLib/Dapper) - high performance object mapper, prefered in place of EF for performance.

## YARP Routes

In addition to standard controller route handling the following YARP configuration is used:

### Custom Handling

The below routes are handled by custom logic and YARP's [Direct Forwarding](https://microsoft.github.io/reverse-proxy/articles/direct-forwarding.html) behaviour:

#### `/iiif-img/{customer}/{space}/{image}/{**assetRequest}`

Handle image asset requests. Will:

* Validate access for restricted assets.
* Proxy `thumbs` for known thumbnail sizes.
* Proxy `thumbsresize` for requests that can be served by resizing thumbs.
* Proxy `specialserver` for `/full/` requests that cannot be handled by thumbs (e.g. due to size, quality, format, rotation).
* Ensure asset copied to fast disk and proxy to appropriate `image-server` for other sizes and tile requests. 

All image requests will have "CustomHeader" headers appended as required.

Decision logic in `ImageRequestHandler` and routing logic in `ImageRouteHandler`. 

> Note: this route also handles `/iiif-img/{version}/{customer}/{space}/{image}/{**assetRequest}` as we manually parse the query rather than using RouteValues.

#### `/iiif-av/{customer}/{space}/{image}/{**assetRequest}`

Handle requests for TimeBased assets. Will:

* Validate access for restricted assets.
* Redirect to s3 for open assets.
* Proxy s3 for restricted assets.

Decision logic in `TimeBasedRequestHandler` and routing logic in `TimeBasedRouteHandlers`.

## Clusters

The following YARP Clusters are used:

* cantaloupe - Cantaloupe image-server.
* specialserver - Cantaloupe image-server configured to understand `s3://` URL from ImageLocation table and read directly from bucket, without needing to orchestrate.
* iip - IIP image-server
* thumbs - Protagonist thumbs service.
* thumbsresize - Protagonist thumbs service, configured to resize thumbs on the fly.

> Note: `thumbs` and `thumbsresize` can reference the same uri if required.

## Configuration

See `OrchestratorSettings` object for available settings.

### ImageServer

`ImageServer` config value specified which image-server to use for serving tile requests. See `ImageServer` enum for options, defaults to `Cantaloupe` if not specified.

`ImageServerPathConfig` defines supported image-services and redirect URLs. This is strongly typed as a dictionary, keyed by `ImageServer` enum with `ImageServerConfig` as value. 

It specifies path templates for supported version - if a version is not found in `VersionPathTemplates` then that version is not supported. 

E.g., the following shows IIPImage supports v2 only and Cantaloupe supports v2 + v3 of IIIF ImageApi spec.

```json
{
  "ImageServerPathConfig": {
    "IIPImage": {
      "Separator": "/",
      "VersionPathTemplates": {
        "V2": "/fcgi-bin/iipsrv.fcgi?IIIF="
      }
    },
    "Cantaloupe": {
      "Separator": "%2F",
      "VersionPathTemplates": {
        "V3": "/iiif/3/",
        "V2": "/iiif/2/"
      }
    }
  }
}
```

### PathTemplates

The default path template for requests is `/{prefix}/{customer}/{space}/{assetPath}`, where:

* `prefix` is route path (e.g. `iiif-manifest`, `iiif-av`, `iiif-img`) and includes version.
* `customer` and `space` are self explanatory
* `assetPath` is the asset identifier plus any specific elements for the current request - e.g. for image requests it will contain the full IIIF image request.

By default the above format is reflected on info.json and single-item manifests.

To facilitate using proxy servers to receive alternative URLs that are then rewritten to standard DLCS URLs, overrides to the default rules can be specified. These are used when outputting any self-referencing URIs (e.g. info.json `id` element).

> For the below to work the expectation is that the `x-forwarded-host` header is set in the proxy.

```
"PathRules": {
  "Default": "/{prefix}/{customer}/{space}/{assetPath}",
  "Overrides": {
    "exclude-space.com": "/{prefix}/{customer}/extra/{assetPath}/",
    "customer-specific.io": "/{prefix}/{assetPath}"
  }
}
```

As an convenience you can specify "PathRules:OverridesAsJson" appSetting that includes a string-based config. This makes it easier to configure via environment variables etc

#### Auth PathTemplates

There is a similar config block availabe for authentication under the `"Auth"` key.

For auth the path replacements are simpler:
* `customer` is the customer the auth service is for
* `behaviour` is the name of the auth service.

```
"Auth": {
  "AuthPathRules": {
    "Default": "/auth/{customer}/{behaviour}",
    "Overrides": {
      "exclude-space.com": "/auth/{behaviour}"
    }
  }
},
```

### Versioned Requests

`DefaultIIIFImageVersion` and `DefaultIIIFPresentationVersion` specify the default IIIF Image and Presentation API's supported.

Alternative versions can be requested by adding `/v2/` or `/v3/` slug before customer, or by using content negotiation.

Info.json requests for canonical version will be redirected to the canonical URL.

Image requests for a specific version that is not supported by downstream image-server will return a `400 BadRequest`.

E.g. assuming defaults of `Cantaloupe` and `V3` for both Image and Presentation:

* `/iiif-img/1/1/test-image/0,0,512,512/512,512/0/default.jpg` will redirect to `Cantaloupe` using `/iiif/3/` path.
* `/iiif-img/v2/1/1/test-image/0,0,512,512/512,512/0/default.jpg` will redirect to `Cantaloupe` using `/iiif/2/`.
* `/iiif-img/v3/1/1/test-image/0,0,512,512/512,512/0/default.jpg` will redirect to `Cantaloupe` using `/iiif/3/`.
* `/iiif-img/1/1/test-image/info.json` will return info.json for `V3` info.json.
* `/iiif-img/v3/1/1/test-image/info.json` redirect to `/iiif-img/1/1/test-image/info.json`.
* `/iiif-img/v2/1/1/test-image/info.json` will return info.json for `V2` info.json.
* `/iiif-manifest/1/1/test-image` will return `V3` manifest.
* `/iiif-manifest/v3/1/1/test-image` will return `V3` manifest.
* `/iiif-manifest/v2/1/1/test-image` will return `V2` manifest.

`DefaultIIIFImageVersion` also specified which version to target on downstream image-server.

## Deployment

See `Dockerfile.Orchestrator` in the solution root for deployment artifacts.

```bash
cd..
docker build -f Dockerfile.Orchestrator -t orchestrator:local .
```

## Known Issues

* Proxying authenticated TimeBased media fails when running locally (S3 permissions issue).