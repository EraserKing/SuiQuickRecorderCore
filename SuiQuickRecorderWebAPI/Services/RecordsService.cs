using Microsoft.Extensions.Options;
using SuiQuickRecorderCore.Models;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Services;
using SuiQuickRecorderWebAPI.Models;

namespace SuiQuickRecorderWebAPI.Services
{
    public class RecordsService
    {
        private readonly ILogger<RecordsService> _logger;
        private readonly IOptions<SuiWebAPIOptions> _options;
        private readonly SuiQuickRecorderService _suiQuickRecorderService;
        private readonly SuiSessionService _suiSessionService;
        private readonly MetadataService _metadataService;

        public RecordsService(ILogger<RecordsService> logger, IOptions<SuiWebAPIOptions> options, SuiQuickRecorderService suiQuickRecorderService, SuiSessionService suiSessionService, MetadataService metadataService)
        {
            _logger = logger;
            _options = options;
            _suiQuickRecorderService = suiQuickRecorderService;
            _suiSessionService = suiSessionService;
            _metadataService = metadataService;
        }

        private async Task EnsureLoggedInAsync()
        {
            if (!_suiSessionService.IsLoggedIn)
            {
                _logger.LogInformation("Session checks failed. Attempting to log in...");
                await _suiSessionService.LoginAsync(_options.Value.Username, _options.Value.Password);
            }
        }

        public async Task<SuiQuickRecorderProcessResult> ProcessRecordsAsync(IEnumerable<SuiRecordOrigin> records)
        {
            await EnsureLoggedInAsync();
            return _suiQuickRecorderService.ProcessRecords(records, await _metadataService.GetReferencesAsync());
        }

        public async Task<SuiQuickRecorderSendResult> SendRecordsAsync(IEnumerable<SuiRecordOrigin> records)
        {
            await EnsureLoggedInAsync();
            return await _suiQuickRecorderService.SendRecordsAsync(records, await _metadataService.GetReferencesAsync());
        }
    }
}