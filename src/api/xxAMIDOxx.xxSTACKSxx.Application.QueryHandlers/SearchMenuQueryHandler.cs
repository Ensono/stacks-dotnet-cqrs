using System;
using System.Linq;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.Queries;
using Amido.Stacks.Data.Documents.Abstractions;
using xxAMIDOxx.xxSTACKSxx.CQRS.Queries.SearchMenu;
using xxAMIDOxx.xxSTACKSxx.Domain;

namespace xxAMIDOxx.xxSTACKSxx.Application.QueryHandlers
{
    public class SearchMenuQueryHandler : IQueryHandler<SearchMenu, SearchMenuResult>
    {
        readonly IDocumentSearch<Menu> storage;

        public SearchMenuQueryHandler(IDocumentSearch<Menu> storage)
        {
            this.storage = storage;
        }

        public Task<SearchMenuResult> ExecuteAsync(SearchMenu criteria)
        {
            if (criteria == null)
                throw new ArgumentException("A valid SearchMenuQueryCriteria os required!");

            // https://rules.sonarsource.com/csharp/RSPEC-4457
            return ExecuteInternalAsync(criteria);
        }

        private async Task<SearchMenuResult> ExecuteInternalAsync(SearchMenu criteria)
        {
            int pageSize = 10;
            int pageNumber = 1;
            var searchTerm = string.Empty;
            Guid tenantId = criteria.TenantId.HasValue ? criteria.TenantId.Value : Guid.Empty;

            if (criteria.PageSize.HasValue && criteria.PageSize > 0)
                pageSize = criteria.PageSize.Value;

            if (criteria.PageNumber.HasValue && criteria.PageNumber > 0)
                pageNumber = criteria.PageNumber.Value;

            if (!string.IsNullOrEmpty(criteria.SearchText))
                searchTerm = criteria.SearchText.Trim();

            bool restaurantIdProvided = criteria.TenantId.HasValue;

            var results = await storage.Search(
                itemFilter =>
                    (string.IsNullOrEmpty(searchTerm) || itemFilter.Name.Contains(searchTerm)) &&
                    //Nullable types must have a value when passed to a seach, this is why we convert it to non nullable and pass a boolean check
                    (!restaurantIdProvided || itemFilter.TenantId == tenantId)
                    ,
                null,
                pageSize,
                pageNumber);

            var result = new SearchMenuResult();
            result.PageSize = pageSize;
            result.PageNumber = pageNumber;

            if (!results.IsSuccessful)
                return result;

            result.Results = results.Content.Select(SearchMenuResultItem.FromDomain);

            return result;
        }
    }
}
