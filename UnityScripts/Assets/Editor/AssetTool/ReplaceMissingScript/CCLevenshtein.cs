using System;
using UnityEditor;

namespace AssetTool.RMS
{
    public class CCLevenshtein
    {
        //public static void CheckEditDistance()
        //{
        //    Int32 Distance;
        //    Double Similarity;

        //    while (true)
        //    {
        //        Console.WriteLine("------------------------------------");
        //        Console.Write("源串 = ");
        //        String Source = Console.ReadLine();
        //        if (Source == "q") break;

        //        Console.Write("目标串 = ");
        //        String Target = Console.ReadLine();

        //        Distance = LevenshteinDistance(Source, Target, out Similarity, true);
        //        Console.WriteLine("编辑距离 = " + Distance.ToString());
        //        Console.WriteLine("相似度 = " + Similarity.ToString("0.####"));
        //    }
        //}

//        [MenuItem("tool/LevenshteinDistance")]
//        public static void CheckEditDistance()
//        {
//            Int32 Distance;
//            Double Similarity;
////            Distance = LevenshteinDistance("BornPoint", "SpawnPoint", out Similarity, true);
////            Distance = LevenshteinDistance("KillBulletinType", "KillDetailInfoType", out Similarity, true);
//            Distance = LevenshteinDistance("CSkillGridHash", "skillSlotDist", out Similarity, false);
//            UnityEngine.Debug.Log("编辑距离 = " + Distance.ToString()+ ";相似度 = " + Similarity.ToString("0.####"));
//        }
        /// <summary>  
        /// 编辑距离（Levenshtein Distance）  
        /// </summary>  
        /// <param name="source">源串</param>  
        /// <param name="target">目标串</param>  
        /// <param name="similarity">输出：相似度，值在0～１</param>  
        /// <param name="isCaseSensitive">是否大小写敏感</param>  
        /// <returns>源串和目标串之间的编辑距离</returns>  
        public static Int32 LevenshteinDistance(String source, String target, out Double similarity, Boolean isCaseSensitive = false)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target))
                {
                    similarity = 1;
                    return 0;
                }
                else
                {
                    similarity = 0;
                    return target.Length;
                }
            }
            else if (String.IsNullOrEmpty(target))
            {
                similarity = 0;
                return source.Length;
            }

            String From, To;
            if (isCaseSensitive)
            {   // 大小写敏感  
                From = source;
                To = target;
            }
            else
            {   // 大小写无关  
                From = source.ToLower();
                To = target.ToLower();
            }

            // 初始化  
            Int32 m = From.Length;
            Int32 n = To.Length;
            Int32[,] H = new Int32[m + 1, n + 1];
            for (Int32 i = 0; i <= m; i++) H[i, 0] = i;  // 注意：初始化[0,0]  
            for (Int32 j = 1; j <= n; j++) H[0, j] = j;

            // 迭代  
            for (Int32 i = 1; i <= m; i++)
            {
                Char SI = From[i - 1];
                for (Int32 j = 1; j <= n; j++)
                {   // 删除（deletion） 插入（insertion） 替换（substitution）  
                    if (SI == To[j - 1])
                        H[i, j] = H[i - 1, j - 1];
                    else
                        H[i, j] = Math.Min(H[i - 1, j - 1], Math.Min(H[i - 1, j], H[i, j - 1])) + 1;
                }
            }

            // 计算相似度  
            Int32 MaxLength = Math.Max(m, n);   // 两字符串的最大长度  
            similarity = ((Double)(MaxLength - H[m, n])) / MaxLength;

            return H[m, n];    // 编辑距离  
        }
    }
}
