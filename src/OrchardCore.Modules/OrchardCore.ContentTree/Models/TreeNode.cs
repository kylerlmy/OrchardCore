using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.ContentTree.Models
{
    public class TreeNode
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string Url { get; set; }
        public string CssClass { get; set; }
        public bool IsLeaf { get; set; }
    }
}
