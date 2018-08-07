using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTree.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentTree.Services
{
    public class ContentTreeNodeProvider : ITreeNodeProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IUrlHelper _urlHelper;
        private readonly ISession _session;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ContentTreeNodeProvider(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            ISession session,
            IAuthorizationService authorizationService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<ContentTreeNodeProvider> stringLocalizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;

            var ac = actionContextAccessor.ActionContext;
            _urlHelper = urlHelperFactory.GetUrlHelper(ac);


            T = stringLocalizer;
        }

        private readonly IStringLocalizer<ContentTreeNodeProvider> T;
        // todo: not needed? can we rely on the name and id of the root node?
        public string Name => T["Content Types"];
        public string Id => T["content-types"];

        //private readonly IContentManager _contentManager;
        //private readonly UrlHelper _url;


        public IEnumerable<TreeNode> GetChildren(string nodeType, string nodeId)
        {
            switch (nodeType)
            {
                case "root":
                    return new[] {
                        GetContentTypesNode()
                    };
                case "content-types":                    
                    // todo: see if it would be better to use ".Listable()"
                    return _contentDefinitionManager.ListTypeDefinitions()
                        .Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Creatable)
                        .OrderBy(ctd => ctd.DisplayName)
                        .Select(GetContentTypeNode);

            }

            return new TreeNode[0];
        }


        private TreeNode GetContentTypeNode(ContentTypeDefinition definition)
        {
            return new TreeNode
            {
                Title = definition.DisplayName,
                Type = "content-type",
                Id = definition.Name,
                Url = _urlHelper.Action(
                    "GetContentItems", "Admin",
                    new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.ContentTree"},
                        {"Controller", "Admin"},
                        {"Action", "GetContentItems"},
                        {"providerId", Id  },
                        {"providerParams[typename]", definition.Name}
                    })
            };
            //return new TreeNode
            //{
            //    Title = definition.DisplayName,
            //    Type = "content-type",
            //    Id = definition.Name,
            //    Url = _urlHelper.Action(
            //        "List", "Admin",
            //        new RouteValueDictionary
            //        {
            //            {"Area", "OrchardCore.Contents"},
            //            {"Controller", "Admin"},
            //            {"Action", "List"},
            //            {"model.Id", definition.Name}
            //        })
            //};
        }

        private TreeNode GetContentTypesNode()
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

        public async Task<IQuery<ContentItem, ContentItemIndex>> GetBaseQuery(Dictionary<string, string> parameters)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();

            
            if ((parameters != null) && (parameters.ContainsKey("typename")) && (parameters["typename"] != null))
            {
                var typeName = parameters["typename"];
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);

                // We display a specific type even if it's not listable so that admin pages
                // can reuse the Content list page for specific types.
                query = query.With<ContentItemIndex>(x => x.ContentType == typeName);
            }
            else
            {
                var listableTypes = (await GetListableTypesAsync()).Select(t => t.Name).ToArray();
                if (listableTypes.Any())
                {
                    query = query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes));
                }
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

