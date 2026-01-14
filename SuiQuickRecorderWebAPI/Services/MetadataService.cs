using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SuiQuickRecorderCore.Models.KeyValue;
using SuiQuickRecorderCore.Models.Options;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Services;
using SuiQuickRecorderWebAPI.Data;
using SuiQuickRecorderWebAPI.Entities;
using SuiQuickRecorderWebAPI.Models;

namespace SuiQuickRecorderWebAPI.Services
{
    public class MetadataService
    {
        private readonly SuiMetadataService _suiMetadataService;
        private readonly SuiContext _context;
        private readonly ILogger<MetadataService> _logger;
        private readonly SuiSessionService _suiSessionService;
        private readonly IOptions<SuiWebAPIOptions> _options;

        public MetadataService(
            SuiMetadataService suiMetadataService,
            SuiContext context,
            ILogger<MetadataService> logger,
            SuiQuickRecorderService suiQuickRecorderService,
            SuiSessionService suiSessionService,
            IOptions<SuiWebAPIOptions> options)
        {
            _suiMetadataService = suiMetadataService;
            _context = context;
            _logger = logger;
            _suiSessionService = suiSessionService;
            _options = options;
        }

        private async Task EnsureLoggedInAsync()
        {
            if (!_suiSessionService.IsLoggedIn)
            {
                _logger.LogInformation("Session checks failed. Attempting to log in...");
                await _suiSessionService.LoginAsync(_options.Value.Username, _options.Value.Password);
            }
        }


        private SuiKVPairs BuildPairs<T>(IEnumerable<T> items, Func<T, string> idSelector, Func<T, string> nameSelector, Func<T, string[]> altsSelector)
        {
            var pairs = new SuiKVPairs();
            foreach (var item in items)
            {
                var id = idSelector(item);
                id = id.Substring(id.LastIndexOf('_') + 1);
                id = id.Substring(id.LastIndexOf('-') + 1);

                var name = nameSelector(item);
                if (!pairs.Contains(name)) pairs.Add(name, id);

                foreach (var alt in altsSelector(item) ?? Array.Empty<string>())
                {
                    if (!pairs.Contains(alt)) pairs.Add(alt, id);
                }
            }
            return pairs;
        }

        public async Task<SuiRecordReference> GetReferencesAsync()
        {
            return new SuiRecordReference(
                BuildPairs(_context.Accounts.ToList(), x => x.Id, x => x.Name, x => x.Alts),
                BuildPairs(_context.CategoriesIn.ToList(), x => x.Id, x => x.Name, x => x.Alts),
                BuildPairs(_context.CategoriesOut.ToList(), x => x.Id, x => x.Name, x => x.Alts),
                BuildPairs(_context.Stores.ToList(), x => x.Id, x => x.Name, x => x.Alts),
                BuildPairs(_context.Loaners.ToList(), x => x.Id, x => x.Name, x => x.Alts)
            );
        }

        public async Task<int> RefreshAccountsAsync()
        {
            await EnsureLoggedInAsync();
            var accountsDto = await _suiMetadataService.FetchAccountsAsync();
            var newAccounts = accountsDto.Select(x => new Account
            {
                Id = x.Id,
                Name = x.Name,
                Alts = x.Alts
            }).ToList();

            // 1. Fetch existing to preserve Alts
            var existingAccounts = await _context.Accounts.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Alts);

            // 2. Prepare data for insert
            foreach (var acc in newAccounts)
            {
                if (existingAccounts.TryGetValue(acc.Id, out var existingAlts))
                {
                    acc.Alts = existingAlts;
                }
            }

            // 3. Clear and Rewrite
            await _context.Accounts.ExecuteDeleteAsync();
            
            await _context.Accounts.AddRangeAsync(newAccounts);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refreshed {Count} accounts.", accountsDto.Count);
            return accountsDto.Count;
        }

        public async Task<int> RefreshCategoriesOutAsync()
        {
            await EnsureLoggedInAsync();
            var categoriesDto = await _suiMetadataService.FetchCategoriesOutAsync();
            var newCategories = categoriesDto.Select(x => new CategoryOut
            {
                Id = x.Id,
                Name = x.Name,
                Alts = x.Alts
            }).ToList();

            var existingCategories = await _context.CategoriesOut.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Alts);

            foreach (var cat in newCategories)
            {
                if (existingCategories.TryGetValue(cat.Id, out var existingAlts))
                {
                    cat.Alts = existingAlts;
                }
            }

            await _context.CategoriesOut.ExecuteDeleteAsync();
            await _context.CategoriesOut.AddRangeAsync(newCategories);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refreshed {Count} output categories.", categoriesDto.Count);
            return categoriesDto.Count;
        }

        public async Task<int> RefreshCategoriesInAsync()
        {
            await EnsureLoggedInAsync();
            var categoriesDto = await _suiMetadataService.FetchCategoriesInAsync();
            var newCategories = categoriesDto.Select(x => new CategoryIn
            {
                Id = x.Id,
                Name = x.Name,
                Alts = x.Alts
            }).ToList();

            var existingCategories = await _context.CategoriesIn.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Alts);

            foreach (var cat in newCategories)
            {
                if (existingCategories.TryGetValue(cat.Id, out var existingAlts))
                {
                    cat.Alts = existingAlts;
                }
            }

            await _context.CategoriesIn.ExecuteDeleteAsync();
            await _context.CategoriesIn.AddRangeAsync(newCategories);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refreshed {Count} input categories.", categoriesDto.Count);
            return categoriesDto.Count;
        }

        public async Task<int> RefreshStoresAsync()
        {
            await EnsureLoggedInAsync();
            var storesDto = await _suiMetadataService.FetchStoresAsync();
            var newStores = storesDto.Select(x => new Store
            {
                Id = x.Id,
                Name = x.Name,
                Alts = x.Alts
            }).ToList();

            var existingStores = await _context.Stores.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Alts);

            foreach (var store in newStores)
            {
                if (existingStores.TryGetValue(store.Id, out var existingAlts))
                {
                    store.Alts = existingAlts;
                }
            }

            await _context.Stores.ExecuteDeleteAsync();
            await _context.Stores.AddRangeAsync(newStores);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refreshed {Count} stores.", storesDto.Count);
            return storesDto.Count;
        }

        public async Task<int> RefreshLoanersAsync()
        {
            await EnsureLoggedInAsync();
            var loanersDto = await _suiMetadataService.FetchLoanersAsync();
            var newLoaners = loanersDto.Select(x => new Loaner
            {
                Id = x.Id,
                Name = x.Name,
                Alts = x.Alts
            }).ToList();

            var existingLoaners = await _context.Loaners.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Alts);

            foreach (var loaner in newLoaners)
            {
                if (existingLoaners.TryGetValue(loaner.Id, out var existingAlts))
                {
                    loaner.Alts = existingAlts;
                }
            }

            await _context.Loaners.ExecuteDeleteAsync();
            await _context.Loaners.AddRangeAsync(newLoaners);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refreshed {Count} loaners.", loanersDto.Count);
            return loanersDto.Count;
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            return await _context.Accounts.AsNoTracking().ToListAsync();
        }

        public async Task<List<CategoryOut>> GetCategoriesOutAsync()
        {
            return await _context.CategoriesOut.AsNoTracking().ToListAsync();
        }

        public async Task<List<CategoryIn>> GetCategoriesInAsync()
        {
            return await _context.CategoriesIn.AsNoTracking().ToListAsync();
        }

        public async Task<List<Store>> GetStoresAsync()
        {
            return await _context.Stores.AsNoTracking().ToListAsync();
        }

        public async Task<List<Loaner>> GetLoanersAsync()
        {
            return await _context.Loaners.AsNoTracking().ToListAsync();
        }

        public async Task<bool> AddAccountAltAsync(string id, string alt)
        {
            var entity = await _context.Accounts.FindAsync(id);
            if (entity == null) return false;

            if (!entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Append(alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemoveAccountAltAsync(string id, string alt)
        {
            var entity = await _context.Accounts.FindAsync(id);
            if (entity == null) return false;

            if (entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Where(x => x != alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AddCategoryOutAltAsync(string id, string alt)
        {
            var entity = await _context.CategoriesOut.FindAsync(id);
            if (entity == null) return false;

            if (!entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Append(alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemoveCategoryOutAltAsync(string id, string alt)
        {
            var entity = await _context.CategoriesOut.FindAsync(id);
            if (entity == null) return false;

            if (entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Where(x => x != alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AddCategoryInAltAsync(string id, string alt)
        {
            var entity = await _context.CategoriesIn.FindAsync(id);
            if (entity == null) return false;

            if (!entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Append(alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemoveCategoryInAltAsync(string id, string alt)
        {
            var entity = await _context.CategoriesIn.FindAsync(id);
            if (entity == null) return false;

            if (entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Where(x => x != alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AddStoreAltAsync(string id, string alt)
        {
            var entity = await _context.Stores.FindAsync(id);
            if (entity == null) return false;

            if (!entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Append(alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemoveStoreAltAsync(string id, string alt)
        {
            var entity = await _context.Stores.FindAsync(id);
            if (entity == null) return false;

            if (entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Where(x => x != alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AddLoanerAltAsync(string id, string alt)
        {
            var entity = await _context.Loaners.FindAsync(id);
            if (entity == null) return false;

            if (!entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Append(alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemoveLoanerAltAsync(string id, string alt)
        {
            var entity = await _context.Loaners.FindAsync(id);
            if (entity == null) return false;

            if (entity.Alts.Contains(alt))
            {
                entity.Alts = entity.Alts.Where(x => x != alt).ToArray();
                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}
