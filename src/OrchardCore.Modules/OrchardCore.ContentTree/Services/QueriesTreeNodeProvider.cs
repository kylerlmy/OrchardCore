using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTree.Models;
using OrchardCore.ContentTree.ViewModels;
using OrchardCore.Queries;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentTree.Services
{
    public class QueriesTreeNodeProvider : ITreeNodeProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IUrlHelper _urlHelper;
        private readonly YesSql.ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryManager _queryManager;


        public QueriesTreeNodeProvider(
            IQueryManager queryManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IHttpContextAccessor httpContextAccesor,
            YesSql.ISession session,
            IAuthorizationService authorizationService,
            IStringLocalizer<ContentTreeNodeProvider> stringLocalizer)
        {
            _queryManager = queryManager;

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _httpContextAccessor = httpContextAccesor;
            _authorizationService = authorizationService;

            var ac = actionContextAccessor.ActionContext;
            _urlHelper = urlHelperFactory.GetUrlHelper(ac);


            T = stringLocalizer;
        }

        private readonly IStringLocalizer<ContentTreeNodeProvider> T;

        // todo: not needed? can we rely on the name and id of the root node?
        public string Name => T["Queries"];
        public string Id => T["queries"];

        // todo: make async?
        // handle authorization: return empty node or throw exception?
        public IEnumerable<TreeNode> GetChildren(string nodeType, string nodeId)
        {
            switch (nodeType)
            {
                case "root":
                    return new[] {
                        GetQueriesNode()
                    };
                case "queries":
                    var result = _queryManager.ListQueriesAsync().Result.Select(GetQueryNode);
                    return result;

            }

            return new TreeNode[0];
        }

        private TreeNode GetQueryNode(Query query)
        {
            return new TreeNode
            {
                Title = query.Name,
                Type = "query",
                Id = query.Name,
                IsLeaf = true,
                Url = _urlHelper.Action(
                    "GetContentItems", "Admin",
                    new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.ContentTree"},
                        {"Controller", "Admin"},
                        {"Action", "GetContentItems"},
                        {"providerId", Id  },
                        {"providerParams[queryName]", query.Name}
                    })
            };        
        }

        private TreeNode GetQueriesNode()
        {
            return new TreeNode
            {
                Title = Name,
                Type = Id,
                Id = Id
            };
        }

        public TreeNode Get(string nodeType, string nodeId)
        {
            throw new NotImplementedException("Get is not implemented: ContentTreeNodeProvider");
        }


        public async Task<IEnumerable<ContentItem>> GetContentItems(
                Dictionary<string, string> specificParams, 
                CommonContentTreeParams commonParams)
        {
            if ((specificParams == null) || (!specificParams.ContainsKey("queryName")) || (specificParams["queryName"] == null))
            {
                return new List<ContentItem>();
            }


            var query = await _queryManager.GetQueryAsync(specificParams["queryName"]);

            if (query == null)
            {
                return new List<ContentItem>();
            }

            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, OrchardCore.Queries.Permissions.CreatePermissionForQuery(query.Name)))
            {
                return new List<ContentItem>();
            }

            var queryParameters = new Dictionary<string, object>(); // JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");

            var result = new List<ContentItem>();
            var rawResult =  await _queryManager.ExecuteQueryAsync(query, queryParameters) as IEnumerable<object>;

            foreach (object o in rawResult)
            {
                var casted = o as ContentItem;
                if (casted != null)
                {
                    result.Add(casted);
                }
            }
            return result;
        }


        private IQuery<ContentItem, ContentItemIndex> ApplyCommonParametersToQuery(
                               IQuery<ContentItem, ContentItemIndex> query,
                               CommonContentTreeParams commonParams)
        {

            if (query == null)
            {
                throw new System.ArgumentNullException(nameof(query));
            }

            if (commonParams == null)
            {
                return query;
            }
            


            switch (commonParams.ContentStatusFilter)
            {
                case ContentsStatusFilter.Published:
                    query = query.With<ContentItemIndex>(x => x.Published);
                    break;
                case ContentsStatusFilter.Draft:
                    query = query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                    break;
                case ContentsStatusFilter.AllVersions:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
                default:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
            }

            if (commonParams.OwnedByMe)
            {
                var UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                query = query.With<ContentItemIndex>(x => x.Owner == UserName);
            }

            switch (commonParams.SortBy)
            {
                case ContentsOrder.Modified:
                    query = commonParams.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(x => x.ModifiedUtc) : query.OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = commonParams.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(x => x.PublishedUtc) : query.OrderByDescending(x => x.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = commonParams.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(x => x.CreatedUtc) : query.OrderByDescending(x => x.CreatedUtc);
                    break;
                default:
                    query = commonParams.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(x => x.ModifiedUtc) : query.OrderByDescending(x => x.ModifiedUtc);
                    break;
            }

            return query;
        }

        // todo: this should be available on a central location, it is a common requirement
        private async Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync()
        {
            var listable = new List<ContentTypeDefinition>();

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return listable;
            }

            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(user, Contents.Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                    if (authorized)
                    {
                        listable.Add(ctd);
                    }
                }
            }
            return listable;

        }



    }
}

