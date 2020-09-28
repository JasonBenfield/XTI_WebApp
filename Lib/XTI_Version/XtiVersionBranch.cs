using System.Text.RegularExpressions;
using XTI_App;

namespace XTI_Version
{
    public sealed class XtiVersionBranch
    {
        private readonly string branchName;
        private static readonly Regex branchNameRegex = new Regex("^xti\\/((major)|(minor)|(patch))\\/(\\d+)$");

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
            if (branchNameRegex.IsMatch(branchName))
            {
                this.branchName = branchName;
            }
            else
            {
                throw new InvalidBranchException(branchName);
            }
        }

        public string BranchName() => branchName;

        public int VersionID()
        {
            var match = branchNameRegex.Match(branchName);
            return int.Parse(match.Groups[5].Value);
        }

        public override string ToString() => $"{nameof(XtiVersionBranch)} {branchName}";
    }
}
