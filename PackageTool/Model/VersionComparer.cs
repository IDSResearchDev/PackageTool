using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTool.Model
{
    public static class VersionComparer
    {
        public static bool IsUptoDate(string updateVersion, string currentVersion)
        {
            Version v1 = new Version(currentVersion);
            Version v2 = new Version(updateVersion);

            return v1.CompareTo(v2) >= 0;
        }
    }
}
