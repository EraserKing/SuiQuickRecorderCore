using Microsoft.AspNetCore.Mvc;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderWebAPI.Services;

namespace SuiQuickRecorderWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly RecordsService _recordsService;

        public RecordsController(RecordsService recordsService)
        {
            _recordsService = recordsService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessRecords(IEnumerable<SuiRecordOrigin> records)
        {
            try
            {
                var result = await _recordsService.ProcessRecordsAsync(records);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendRecords(IEnumerable<SuiRecordOrigin> records)
        {
            try
            {
                var result = await _recordsService.SendRecordsAsync(records);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}