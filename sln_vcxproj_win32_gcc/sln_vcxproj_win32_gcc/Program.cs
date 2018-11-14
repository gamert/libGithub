using Helper;
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
            if (args[0].EndsWith(".sln"))
            {
                CSln.Convert(args[0]);
            }
            //"G:/xProject_dp/dp/Classes/libGraphic/libGraphic_gl.vcxproj"
            else if (args[0].EndsWith(".vcxproj"))
            {
                CVcxproj xml = new Helper.CVcxproj();
                xml.CreateAndroid(args[0]);
            }
        }
    }
}

