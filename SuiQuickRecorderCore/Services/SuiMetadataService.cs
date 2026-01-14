using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SuiQuickRecorderCore.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SuiQuickRecorderCore.Services
{
    public class SuiMetadataService
    {
        private readonly SuiSessionService _sessionService;
        private readonly ILogger<SuiMetadataService> _logger;

        public SuiMetadataService(SuiSessionService sessionService, ILogger<SuiMetadataService> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<List<ReferenceItemDto>> FetchAccountsAsync()
        {
            var html = await _sessionService.Client.GetStringAsync("account/account.do");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rootNode = doc.GetElementbyId("r-list");
            if (rootNode == null)
            {
                _logger.LogWarning("Root element 'r-list' not found in the response.");
                return new List<ReferenceItemDto>();
            }

            var accountDivs = rootNode.SelectNodes(".//div[contains(@class, 'acc') and contains(@class, 'group-')]");
            if (accountDivs == null)
            {
                _logger.LogWarning("No accounts found in the response.");
                return new List<ReferenceItemDto>();
            }

            var accounts = new List<ReferenceItemDto>();
            foreach (var div in accountDivs)
            {
                var hasSubAccounts = div.SelectSingleNode(".//div[contains(@class, 'acc-child-card')]") != null;
                
                if (hasSubAccounts)
                {
                    var childDivs = div.SelectNodes(".//div[starts-with(@id, 'child-')]");
                    if (childDivs != null)
                    {
                        foreach (var childDiv in childDivs)
                        {
                            var idAttr = childDiv.GetAttributeValue("id", "");
                            var rawId = idAttr.Substring(6); // Remove "child-"
                            
                            var parentInput = childDiv.SelectSingleNode($".//input[@id='acc-val-parent-{rawId}']");
                            var parentValue = parentInput?.GetAttributeValue("value", "");
                            
                            if (parentValue == "-1")
                                continue;

                            var dbId = $"tb-outAccount-1_v_{rawId}";
                            var nameInput = childDiv.SelectSingleNode($".//input[@id='acc-val-name-{rawId}']");
                            var name = nameInput?.GetAttributeValue("value", "") ?? "Unknown";

                            accounts.Add(new ReferenceItemDto
                            {
                                Id = dbId,
                                Name = name,
                                Alts = Array.Empty<string>()
                            });
                        }
                    }
                }
                else { 
                    var idAttr = div.GetAttributeValue("id", "");
                    if (string.IsNullOrEmpty(idAttr) || !idAttr.StartsWith("acc-")) continue;

                    var rawId = idAttr.Substring(4); // Remove "acc-"
                    var dbId = $"tb-outAccount-1_v_{rawId}";

                    var nameInput = div.SelectSingleNode($".//input[@id='acc-val-name-{rawId}']");
                    var name = nameInput?.GetAttributeValue("value", "") ?? "Unknown";

                    accounts.Add(new ReferenceItemDto
                    {
                        Id = dbId,
                        Name = name,
                        Alts = Array.Empty<string>()
                    });
                }
            }
            return accounts;
        }

        public async Task<List<ReferenceItemDto>> FetchCategoriesOutAsync()
        {
            var html = await _sessionService.Client.GetStringAsync("category/budgetCategory.do");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rootNode = doc.GetElementbyId("list");
            if (rootNode == null) return new List<ReferenceItemDto>();

            var categoryDivs = rootNode.SelectNodes(".//div[contains(@class, 'lb-row')]");
            if (categoryDivs == null) return new List<ReferenceItemDto>();

            var categories = new List<ReferenceItemDto>();
            foreach (var div in categoryDivs)
            {
                var idAttr = div.GetAttributeValue("id", "");
                if (string.IsNullOrEmpty(idAttr) || !idAttr.StartsWith("category")) continue;

                var rawId = idAttr.Substring(8);
                var dbId = $"ls-li-payout-{rawId}";

                var nameNode = div.SelectSingleNode(".//li[contains(@class, 'li-level2')]");
                var name = nameNode?.GetAttributeValue("title", "") ?? "Unknown";

                categories.Add(new ReferenceItemDto { Id = dbId, Name = name, Alts = Array.Empty<string>() });
            }
            return categories;
        }

        public async Task<List<ReferenceItemDto>> FetchCategoriesInAsync()
        {
            var html = await _sessionService.Client.GetStringAsync("category/incomeCategory.do");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rootNode = doc.GetElementbyId("list");
            if (rootNode == null) return new List<ReferenceItemDto>();

            var categoryDivs = rootNode.SelectNodes(".//div[contains(@class, 'lb-row')]");
            if (categoryDivs == null) return new List<ReferenceItemDto>();

            var categories = new List<ReferenceItemDto>();
            foreach (var div in categoryDivs)
            {
                var idAttr = div.GetAttributeValue("id", "");
                if (string.IsNullOrEmpty(idAttr) || !idAttr.StartsWith("category")) continue;

                var rawId = idAttr.Substring(8);
                var dbId = $"ls-li-income-{rawId}";

                var nameNode = div.SelectSingleNode(".//li[contains(@class, 'li-level2')]");
                var name = nameNode?.InnerText ?? "Unknown";

                categories.Add(new ReferenceItemDto { Id = dbId, Name = name, Alts = Array.Empty<string>() });
            }
            return categories;
        }

        public async Task<List<ReferenceItemDto>> FetchStoresAsync()
        {
            var html = await _sessionService.Client.GetStringAsync("category/storeCategory.do");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rootNode = doc.GetElementbyId("list-display");
            if (rootNode == null) return new List<ReferenceItemDto>();

            var stores = new List<ReferenceItemDto>
            {
                new ReferenceItemDto { Id = "tb-store_v_0", Name = "нч", Alts = Array.Empty<string>() }
            };

            var storeDivs = rootNode.SelectNodes(".//div[contains(@class, 'lb-row')]");
            if (storeDivs != null)
            {
                foreach (var div in storeDivs)
                {
                    var idAttr = div.GetAttributeValue("id", "");
                    if (string.IsNullOrEmpty(idAttr) || !idAttr.StartsWith("category")) continue;

                    var rawId = idAttr.Substring(8);
                    var dbId = $"tb-store_v_{rawId}";

                    var nameNode = div.SelectSingleNode(".//li[contains(@class, 'li-level2')]");
                    var name = nameNode?.InnerText?.Trim() ?? "Unknown";

                    stores.Add(new ReferenceItemDto { Id = dbId, Name = name, Alts = Array.Empty<string>() });
                }
            }
            return stores;
        }

        public async Task<List<ReferenceItemDto>> FetchLoanersAsync()
        {
            try
            {
                var response = await _sessionService.Client.PostAsync("fresh/debt.rmi", new FormUrlEncodedContent(new Dictionary<string, string>{{"opt", "getDebtData"}}));
                response.EnsureSuccessStatusCode();

                var obj = await response.Content.ReadFromJsonAsync<DebtResponseDto>();
                if (obj == null || obj.DebtList == null)
                {
                    return new List<ReferenceItemDto>();
                }

                var loaners = new List<ReferenceItemDto>();
                foreach (var debt in obj.DebtList)
                {
                    var dbId = $"loan-name-{debt.Id}";
                    loaners.Add(new ReferenceItemDto { Id = dbId, Name = debt.Name ?? "Unknown", Alts = Array.Empty<string>() });
                }
                return loaners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch loaners.");
                return new List<ReferenceItemDto>();
            }
        }
    }
}
