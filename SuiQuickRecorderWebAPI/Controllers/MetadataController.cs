using Microsoft.AspNetCore.Mvc;
using SuiQuickRecorderWebAPI.Services;

namespace SuiQuickRecorderWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly MetadataService _metadataService;

        public MetadataController(MetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [HttpPost("refresh-accounts")]
        public async Task<IActionResult> RefreshAccounts()
        {
            var count = await _metadataService.RefreshAccountsAsync();
            return Ok($"{count} account(s) refreshed successfully.");
        }

        [HttpPost("refresh-categories-out")]
        public async Task<IActionResult> RefreshCategoriesOut()
        {
            var count = await _metadataService.RefreshCategoriesOutAsync();
            return Ok($"{count} output categori(es) refreshed successfully.");
        }

        [HttpPost("refresh-categories-in")]
        public async Task<IActionResult> RefreshCategoriesIn()
        {
            var count = await _metadataService.RefreshCategoriesInAsync();
            return Ok($"{count} input categori(es) refreshed successfully.");
        }

        [HttpPost("refresh-stores")]
        public async Task<IActionResult> RefreshStores()
        {
            var count = await _metadataService.RefreshStoresAsync();
            return Ok($"{count} store(s) refreshed successfully.");
        }

        [HttpPost("refresh-loaners")]
        public async Task<IActionResult> RefreshLoaners()
        {
            var count = await _metadataService.RefreshLoanersAsync();
            return Ok($"{count} loaners refreshed successfully.");
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts() => Ok(await _metadataService.GetAccountsAsync());

        [HttpPost("accounts/{id}/alts")]
        public async Task<IActionResult> AddAccountAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.AddAccountAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpPost("accounts/{id}/alts/remove")]
        public async Task<IActionResult> RemoveAccountAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.RemoveAccountAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpGet("categories-out")]
        public async Task<IActionResult> GetCategoriesOut() => Ok(await _metadataService.GetCategoriesOutAsync());

        [HttpPost("categories-out/{id}/alts")]
        public async Task<IActionResult> AddCategoryOutAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.AddCategoryOutAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpPost("categories-out/{id}/alts/remove")]
        public async Task<IActionResult> RemoveCategoryOutAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.RemoveCategoryOutAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpGet("categories-in")]
        public async Task<IActionResult> GetCategoriesIn() => Ok(await _metadataService.GetCategoriesInAsync());

        [HttpPost("categories-in/{id}/alts")]
        public async Task<IActionResult> AddCategoryInAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.AddCategoryInAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpPost("categories-in/{id}/alts/remove")]
        public async Task<IActionResult> RemoveCategoryInAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.RemoveCategoryInAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetStores() => Ok(await _metadataService.GetStoresAsync());

        [HttpPost("stores/{id}/alts")]
        public async Task<IActionResult> AddStoreAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.AddStoreAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpPost("stores/{id}/alts/remove")]
        public async Task<IActionResult> RemoveStoreAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.RemoveStoreAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpGet("loaners")]
        public async Task<IActionResult> GetLoaners() => Ok(await _metadataService.GetLoanersAsync());

        [HttpPost("loaners/{id}/alts")]
        public async Task<IActionResult> AddLoanerAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.AddLoanerAltAsync(id, alt)) return Ok();
            return NotFound();
        }

        [HttpPost("loaners/{id}/alts/remove")]
        public async Task<IActionResult> RemoveLoanerAlt(string id, [FromBody] string alt)
        {
            if (await _metadataService.RemoveLoanerAltAsync(id, alt)) return Ok();
            return NotFound();
        }
    }
}
