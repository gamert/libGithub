using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XmlExample
{

    class Program
    {
        static void Main(string[] args)
        {
            Helper.CXmlVcxproj xml = new Helper.CXmlVcxproj();
            xml.CreateAndroid("G:/xProject_dp/dp/Classes/libGraphic/libGraphic_gl.vcxproj");
        }
    }
}

