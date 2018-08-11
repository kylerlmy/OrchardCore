using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTree.Models;
using OrchardCore.ContentTree.ViewModels;
using YesSql;

namespace OrchardCore.ContentTree.Services
{
    public interface ITreeNodeProvider
    {
        string Name { get; }
        string Id { get; }
        IEnumerable<TreeNode> GetChildren(string nodeType, string nodeId);
        TreeNode Get(string nodeType, string nodeId);
        Task<IEnumerable<ContentItem>>GetContentItems(Dictionary<string, string> specificParams,CommonContentTreeParams commonParams);
    }
}
