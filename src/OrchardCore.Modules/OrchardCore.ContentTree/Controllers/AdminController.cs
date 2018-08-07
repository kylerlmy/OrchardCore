using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentTree.Services;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTree.Controllers
{
    public class AdminController: Controller, IUpdateModel
    {
        private readonly IEnumerable<ITreeNodeProvider> _treeNodeProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        public AdminController(
            IEnumerable<ITreeNodeProvider> treeNodeProviders,
            IAuthorizationService authorizationService,
            ISession session,
            IContentItemDisplayManager contentItemDisplayManager,
            IShapeFactory shapeFactory,
            ILogger<AdminController> logger)
        {
            _treeNodeProviders = treeNodeProviders;
            _authorizationService = authorizationService;
            _session = session;
            _contentItemDisplayManager = contentItemDisplayManager;
            Logger = logger;
        }

        public ILogger Logger { get; set; }
        public dynamic New { get; set; }

        public IActionResult List()
        {
            return View();
        }

        public async Task<ActionResult> GetTreeNodeProviders()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseContentTree))
            {
                return Unauthorized();
            }

            var result = _treeNodeProviders.Select(x => x.GetChildren("root", string.Empty).FirstOrDefault());

            return Json(result);
            //return Json(_treeNodeProviders.Select(x => x.Name).ToArray());
        }

        public async Task<ActionResult> GetChildren(string parentId, string parentType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseContentTree))
            {
                return Unauthorized();
            }

            var children = _treeNodeProviders
                .SelectMany(p => p.GetChildren(parentType, parentId)).ToArray();
            return Json(children);

            //var result = _treeNodeProviders.Select(x => x.GetChildren("root", string.Empty).FirstOrDefault());

            //return Json(result);
            //return Json(_treeNodeProviders.Select(x => x.Name).ToArray());
        }
        //, Dictionary<string,string> providerSpecificParams
        public async Task<IActionResult> GetContentItems(string providerId, Dictionary<string, string> providerParams)
        {
            var provider = _treeNodeProviders.Where(x => x.Id == providerId).FirstOrDefault();
            if (provider == null)
            {
                return NotFound();
            }

            var query = await provider.GetBaseQuery(providerParams); // _session.Query<ContentItem, ContentItemIndex>(); // await _filterBoxService.ApplyFilterBoxOptionsToQuery(, filterBoxModel);

            // todo: see if we should replicate what OrchardCore.Contents is doing
            // Invoke any service that could alter the query            
            // await _contentAdminFilters.InvokeAsync(x => x.FilterAsync(query, model, pagerParameters, this), Logger);

            //var maxPagedCount = siteSettings.MaxPagedCount;
            //if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
            //    pager.PageSize = maxPagedCount;

            //var pagerShape = (await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync());
            //var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();
            var pageOfContentItems = await query.ListAsync(); //.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();
            //var contentItemSummaries = new List<dynamic>();
            //foreach (var contentItem in pageOfContentItems)
            //{
            //    contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "SummaryAdmin"));
            //}

            //var viewModel = (await New.ViewModel())
            //    .ContentItems(contentItemSummaries);

            //return View(viewModel);
            return Json(pageOfContentItems);
        }
    }
}
