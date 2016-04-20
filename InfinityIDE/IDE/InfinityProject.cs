using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace InfinityIDE.IDE
{
    public class InfinityProject
    {
        private DirectoryInfo _project_folder;
        public DirectoryInfo ProjectFolder { get { return _project_folder; } set { _project_folder = value; } }
        private ProjectInfo _project_info;
        public ProjectInfo Info { get { return _project_info; } set { _project_info = value; } }

        public void Save()
        {
            XmlSerializer xs = GetSerializer();
            DirectoryInfo pd = new DirectoryInfo(_project_folder.FullName);
            if (!pd.Exists)
                pd.Create();
            StreamWriter pinfo = new StreamWriter(pd.FullName + "\\" + _project_info.Name + ".iproj");
            xs.Serialize(pinfo, _project_info);
            pinfo.Close();
        }

        public static XmlSerializer GetSerializer()
        {
            Type[] types = { 
                               typeof(ProjectElement), 
                               typeof(File), 
                               typeof(Directory), 
                               typeof(Reference),
                               typeof(ReferenceType)
                           };
            XmlSerializer xs = new XmlSerializer(typeof(ProjectInfo), types);
            return xs;
        }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public List<ProjectElement> Children { get; set; }
        public List<Reference> References { get; set; }

        public ProjectInfo()
        {
            Children = new List<ProjectElement>();
            References = new List<Reference>();
        }
    }

    public class ProjectElement 
    {
        public string Name { get; set; } 
    }

    public class File : ProjectElement
    {
        public string Type { get; set; }
    }

    public class Directory : ProjectElement
    {
        public List<ProjectElement> Children { get; set; }

        public Directory()
        {
            Children = new List<ProjectElement>();
        }
    }

    public enum ReferenceType
    {
        Library,
        Script
    }

    public class Reference 
    {
        public string ReferencePath { get; set; }
        public string ReferenceType { get; set; }
    }
}
