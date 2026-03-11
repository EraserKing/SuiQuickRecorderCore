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

        public SuiQuickRecorderProcessResult ProcessRecords(IEnumerable<SuiRecordOrigin> origins, SuiRecordReference reference)
        {
            var lineResults = BuildLineResults(origins, reference);

            var failedCount = lineResults.Count(r => !r.Success);
            if (failedCount > 0)
            {
                _logger.LogError("Failed to process {Count} line(s)", failedCount);
            }

            return new SuiQuickRecorderProcessResult { Lines = lineResults };
        }

        private List<SuiLineProcessResult> BuildLineResults(IEnumerable<SuiRecordOrigin> origins, SuiRecordReference reference)
        {
            return origins
                .Select((x, l) => SuiRecordFactory.Create(x, reference, l))
                .ToList();
        }

        public async Task<SuiQuickRecorderSendResult> SendRecordsAsync(IEnumerable<SuiRecordOrigin> origins, SuiRecordReference reference)
        {
            var result = new SuiQuickRecorderSendResult();

            if (!_sessionService.IsLoggedIn)
            {
                throw new InvalidOperationException("Not logged in.");
            }

            var lineResults = BuildLineResults(origins, reference);
            var failedLines = lineResults.Where(r => !r.Success).ToList();
            if (failedLines.Any())
            {
                result.Lines.AddRange(failedLines);
                return result;
            }

            // Flatten all records; remove loan records, combine and re-add
            var records = lineResults.SelectMany(r => r.Records).ToList();
            var loanRecords = SuiRecordLoan.AutoCombine(records.Where(x => x.RecordType == SuiRecordType.Loan).Cast<SuiRecordLoan>()).ToArray();
            records.RemoveAll(x => x.RecordType == SuiRecordType.Loan);
            records.AddRange(loanRecords);

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
                        var failedLine = result.Lines.FirstOrDefault(x => x.Line == totalRecords);
                        if (failedLine == null)
                        {
                            failedLine = new SuiLineProcessResult { Line = totalRecords };
                            result.Lines.Add(failedLine);
                        }

                        failedLine.Success = false;
                        failedLine.Errors.Add(msg);
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