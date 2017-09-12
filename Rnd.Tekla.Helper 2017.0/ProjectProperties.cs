using System;
using Rnd.Common.Resources;
using Tekla.Structures.Model;

namespace Rnd.TeklaStructure.Helper
{
    public class ProjectProperties
    {

        private string _jobCode;

        private string _jobnumber;

        private string _fabricator;

        private string _fabaddress;

        public string Fabricator
        {
            get { return _fabricator; }
            set { _fabricator = value; }
        }

        public string JobNumber
        {
            get { return _jobnumber; }
            set
            {
                _jobnumber = value;
            }
        }

        public string JobCode
        {
            get { return _jobCode; }
            set { _jobCode = value; }
        }

        
        public string Fabaddress
        {
            get { return _fabaddress; }

            set
            {
                _fabaddress = value;
            }
        }


        public ProjectProperties()
        {
            Model model = new Model();
            if (!model.GetConnectionStatus()) { throw new ArgumentException(ErrorCollection.TeklaNotRunning); }

            ProjectInfo projectInfo = model.GetProjectInfo();
            if (projectInfo == null) { throw new ArgumentException(ErrorCollection.NoOpenModel); }

            string fab="",addrs="";
            projectInfo.GetUserProperty("FAB_NAME", ref fab);
            projectInfo.GetUserProperty("FAB_ADDRESS", ref addrs);

            _fabaddress = addrs;
            _fabricator = fab;
            _jobCode = projectInfo.Info2;
            _jobnumber = projectInfo.ProjectNumber;

        }    
    }
}
