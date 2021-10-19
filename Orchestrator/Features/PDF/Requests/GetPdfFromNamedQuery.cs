﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DLCS.Model.Assets.NamedQueries;
using DLCS.Model.PathElements;
using MediatR;
using Orchestrator.Infrastructure.NamedQueries;

namespace Orchestrator.Features.PDF.Requests
{
    /// <summary>
    /// Mediatr request for generating PDF via named query
    /// </summary>
    public class GetPdfFromNamedQuery : IRequest<PdfFromNamedQuery>
    {
        public string CustomerPathValue { get; }
        
        public string NamedQuery { get; }
        
        public string? NamedQueryArgs { get; }
        
        // What does this return? A path to the S3 location that contains the PDF?
    }
    
    public class GetPdfFromNamedQueryHandler : IRequestHandler<GetPdfFromNamedQuery, PdfFromNamedQuery>
    {
        private readonly IPathCustomerRepository pathCustomerRepository;
        private readonly NamedQueryConductor namedQueryConductor;
        private readonly PdfNamedQueryService pdfNamedQueryService;

        public GetPdfFromNamedQueryHandler(
            IPathCustomerRepository pathCustomerRepository,
            NamedQueryConductor namedQueryConductor, 
            PdfNamedQueryService pdfNamedQueryService
        )
        {
            this.pathCustomerRepository = pathCustomerRepository;
            this.namedQueryConductor = namedQueryConductor;
            this.pdfNamedQueryService = pdfNamedQueryService;
        }
        public async Task<PdfFromNamedQuery> Handle(GetPdfFromNamedQuery request, CancellationToken cancellationToken)
        {
            var customerPathElement = await pathCustomerRepository.GetCustomer(request.CustomerPathValue);

            var namedQueryResult =
                await namedQueryConductor.GetNamedQueryResult<PdfParsedNamedQuery>(request.NamedQuery,
                    customerPathElement, request.NamedQueryArgs);

            if (namedQueryResult.Query is { IsFaulty: true }) return PdfFromNamedQuery.BadRequest();

            var pdfResult = await pdfNamedQueryService.GetPdfResults(namedQueryResult, request.NamedQuery);

            if (pdfResult.Status == PdfStatus.InProcess) return PdfFromNamedQuery.InProcess();

            return new PdfFromNamedQuery(pdfResult.Stream);
        }
    }

    public class PdfFromNamedQuery
    {
        public Stream PdfStream { get; } = Stream.Null;

        public bool IsEmpty => PdfStream == Stream.Null;
        
        public bool IsBadRequest { get; private init; }
        
        public bool IsInProcess { get; private init; }

        public static PdfFromNamedQuery BadRequest() => new() { IsBadRequest = true };
        
        public static PdfFromNamedQuery InProcess() => new() { IsInProcess = true };

        public PdfFromNamedQuery()
        {
        }

        public PdfFromNamedQuery(Stream pdfStream)
        {
            PdfStream = pdfStream;
        }
    }
}