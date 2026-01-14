using Microsoft.Extensions.Logging;
using SuiQuickRecorderCore.Models;
using SuiQuickRecorderCore.Models.Interfaces;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Models.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SuiQuickRecorderCore.Services
{
    public class SuiQuickRecorderService
    {
        private readonly SuiSessionService _sessionService;
        private readonly ILogger<SuiQuickRecorderService> _logger;

        public SuiQuickRecorderService(SuiSessionService sessionService, ILogger<SuiQuickRecorderService> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public List<ISuiRecord> ProcessRecords(IEnumerable<SuiRecordOrigin> origins, SuiRecordReference reference)
        {
            var createExceptions = new List<Exception>();
            var records = origins
                .SelectMany((x, l) => SuiRecordFactory.Create(x, reference, l, createExceptions))
                .ToList();

            if (createExceptions.Any())
            {
                var aggregateEx = new AggregateException($"Some error(s) occurred when processing records: {Environment.NewLine}{string.Join(Environment.NewLine, createExceptions.Select(x => $"{x.Message}: {x.InnerException?.Message}"))}", createExceptions);
                _logger.LogError(aggregateEx, "Failed to process records");
                throw aggregateEx;
            }

            // Remove loan records, combine and re-add
            var loanRecords = SuiRecordLoan.AutoCombine(records.Where(x => x.RecordType == SuiRecordType.Loan).Cast<SuiRecordLoan>()).ToArray();
            records.RemoveAll(x => x.RecordType == SuiRecordType.Loan);
            records.AddRange(loanRecords);

            return records;
        }

        public async Task<SuiQuickRecorderProcessResult> SendRecordsAsync(IEnumerable<SuiRecordOrigin> origins, SuiRecordReference reference)
        {
            var result = new SuiQuickRecorderProcessResult();

            if (!_sessionService.IsLoggedIn)
            {
                throw new InvalidOperationException("Not logged in.");
            }

            // Processing Stage
            List<ISuiRecord> records;
            try
            {
                records = ProcessRecords(origins, reference);
            }
            catch (AggregateException ex)
            {
                result.Errors.Add(ex.Message);
                throw;
            }

            _logger.LogInformation("Processed {Count} records to send.", records.Count);

            // Sending Stage
            int successRecords = 0;
            int totalRecords = 0;

            int successRequests = 0;
            int totalRequests = 0;

            foreach (var record in records)
            {
                totalRecords++;
                bool recordSuccess = true;

                foreach (var networkRequest in record.CreateNetworkRequests())
                {
                    totalRequests++;
                    int retryCount = 0;
                    bool requestSuccess = false;

                    do
                    {
                        if (retryCount > 0)
                        {
                            var msg = $"RETRY ATTEMPT #{retryCount}";
                            _logger.LogWarning(msg);
                            result.Warnings.Add(msg + $" (Record {totalRecords})");
                        }
                        
                        var formStr = string.Join('&', networkRequest.Content.Select(kv => $"{kv.Key}={System.Web.HttpUtility.UrlEncode(kv.Value)}"));
                        var formCtx = new StringContent(formStr, Encoding.UTF8, "application/x-www-form-urlencoded");
                        var response = await _sessionService.Client.PostAsync(networkRequest.Endpoint, formCtx);
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        networkRequest.ResponseContent = responseContent;

                        _logger.LogInformation("{ResponseContent}", responseContent);
                        requestSuccess = networkRequest.IsSuccess(response, responseContent);
                    }
                    while (!requestSuccess && retryCount++ < 10);

                    if (retryCount >= 10)
                    {
                        var msg = "ALL RETRY ATTEMPT FAILED";
                        _logger.LogError(msg);
                        result.Errors.Add(msg + $" (Record {totalRecords})");
                        recordSuccess = false;
                    }

                    if (requestSuccess)
                    {
                        successRequests++;
                    }
                }

                if (recordSuccess)
                {
                    successRecords++;
                }
            }
            _logger.LogInformation("Records success {SuccessRecords} / total {TotalRecords}, requests success {SuccessRequests} / total {TotalRequests}", successRecords, totalRecords, successRequests, totalRequests);

            result.TotalRecords = totalRecords;
            result.SuccessRecords = successRecords;
            result.TotalRequests = totalRequests;
            result.SuccessRequests = successRequests;

            return result;
        }
    }
}