using System;
using System.Collections.Generic;
using System.Text;
using XTI_App;

namespace XTI_Version
{
    public sealed class XtiVersionBranch
    {
        private readonly string branchName;

        public XtiVersionBranch(AppVersion version)
            : this(getBranchName(version))
        {
        }

        private static string getBranchName(AppVersion version)
        {
            string type;
            if (version.IsMajor())
            {
                type = "major";
            }
            else if (version.IsMinor())
            {
                type = "minor";
            }
            else if (version.IsPatch())
            {
                type = "patch";
            }
            else
            {
                type = "";
            }
            return $"xti/{type}/{version.ID}";
        }

        public XtiVersionBranch(string branchName)
        {
            this.branchName = branchName;
        }

        public string BranchName() => branchName;
        public int VersionID() => int.Parse(branchName.Substring(branchName.LastIndexOf("/") + 1));

        public override string ToString() => $"{nameof(XtiVersionBranch)} {branchName}";
    }
}
