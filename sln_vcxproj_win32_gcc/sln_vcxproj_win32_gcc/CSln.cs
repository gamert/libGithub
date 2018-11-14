using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Helper
{
    public class CSln
    {
        static string m_sln;
        static string m_path;
        public static void Convert(string _sln)
        {
            m_sln = _sln;
            // string line = "Project(\"{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}\") = \"libcocos2d\", \"..\\cocos2d\\cocos\\2d\\libcocos2d.vcxproj\", \"{98A51BA8-FC3A-415B-AC8F-8C7BD464E93E}\"";
            m_path = System.IO.Path.GetDirectoryName(m_sln);
            int count = 0;
            string[] ss = File.ReadAllLines(m_sln);
            if (ss != null)
            {
                for (int i = 0; i < ss.Length; ++i)
                {
                    if(DoLine(ss[i]))
                    {
                        ss[i] = ss[i].Replace(".vcxproj\"", "_a.vcxproj\"");
                        count++;
                    }
                }
            }
            if(count > 0)
            {
                string newsln = m_sln.Replace(".sln", "_a.sln");
                File.WriteAllLines(newsln, ss);
            }
        }

        static string pattern = "Project\\(\"\\{([0-9a-zA-Z-]+)\\}\"\\) \\= \"([0-9a-zA-Z_-]+)\"\\, \"([0-9a-zA-Z-_\\\\\\.]+)\"\\, \"\\{([0-9a-zA-Z-]+)\\}\"";
        static Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);

        public static bool DoLine(string line)
        {

            if (line.StartsWith("Project(\"{"))
            {
                //guid = 8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942
                //Project("{guid1}") = "libcocos2d", "..\cocos2d\cocos\2d\libcocos2d.vcxproj", "{guid2}"
                MatchCollection matches = rgx.Matches(line);
                if (matches.Count > 0)
                {
                    Console.WriteLine("{0} ({1} matches):", line, matches.Count);
                    foreach (Match match in matches)
                    {
                        Group gg = match.Groups[3];
                        Console.WriteLine(" " + gg);
                        string proj = gg.ToString();
                        if(proj.EndsWith(".vcxproj"))
                        {
                            CVcxproj xml = new Helper.CVcxproj();
                            xml.CreateAndroid(m_path + "\\" + proj);
                            return true;

                        }
                    }
                }
            }
            return false;
        }
    }
}
