using Newtonsoft.Json;

namespace Hydra.Model;

/// <summary>
/// The DLCS API Client expects Hydra error responses and will try to parse them.
/// Other callers might prefer a JSON Problem response as defined in RFC 7807.
/// https://datatracker.ietf.org/doc/html/rfc7807
///
/// This class is both a Hydra:Error AND a valid JSON Problem:
/// https://www.hydra-cg.com/spec/latest/core/#example-31-rfc-7807-compatible-error-description 
/// </summary>
public class Error : Status
{
    /// <summary>
    /// Hydra JSON-LD @type
    /// </summary>
    public override string Type => "Error";
    
    /// <summary>
    /// rfc7807 Json Problem field:
    /// A URI reference [RFC3986] that identifies the problem type.  This specification encourages that, when
    /// dereferenced, it provide human-readable documentation for the problem type (e.g., using
    /// HTML [W3C.REC-html5-20141028]).  
    /// </summary>
    [JsonProperty(Order = 3, PropertyName = "type")]
    public string? ErrorTypeUri { get; set; }
    
    
    // (rfc7807 JSON Problem `title` shares the same-named field in Hydra:Error)
    
    
    /// <summary>
    /// rfc7807 JSON Problem field - an alias for Hydra `StatusCode`
    /// The HTTP status code ([RFC7231], Section 6) generated by the origin server
    /// for this occurrence of the problem.
    /// </summary>
    [JsonProperty(Order = 11, PropertyName = "status")]
    public int Status
    {
        get => StatusCode;
        set => StatusCode = value;
    }
    
    /// <summary>
    /// rfc7807 JSON Problem field - an alias for Hydra `Description`
    /// (string) - A human-readable explanation specific to this occurrence of the problem.
    ///
    /// The "detail" member, if present, ought to focus on helping the client
    /// correct the problem, rather than giving debugging information.
    /// </summary>
    [JsonProperty(Order = 12, PropertyName = "detail")]
    public string? Detail
    {
        get => Description;
        set => Description = value;
    }

    /// <summary>
    /// rfc7807 JSON Problem field. No Hydra equivalent.
    /// A URI reference that identifies the specific occurrence of the problem.
    /// It may or may not yield further information if dereferenced.
    /// 
    /// Use this to convey the resource that the problem applies to (an Asset URI, a Customer URI etc).
    /// </summary>
    [JsonProperty(Order = 12, PropertyName = "instance")]
    public string? Instance { get; set; }
}