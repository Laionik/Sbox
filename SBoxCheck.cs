using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBox
{
    class SBoxCheck
    {
        /// <summary>
        /// Get sboxFunctions
        /// </summary>
        /// <returns>List of sbox functions</returns>
        /// Console Color DarkYellow
        static List<string> GetSboxFunctions()
        {
            // read all bytes from sbox
            var sboxFile = File.ReadAllBytes("sbox_08x08_NL86.sbx").Where((x, i) => i % 2 == 0).ToList();
            string[] sboxTab = new string[8];
            for (int i = 0; i < sboxTab.Count(); i++)
            {
                sboxTab[i] = "";
            }

            // create functions
            foreach (var line in sboxFile)
            {
                var binaryLine = Convert.ToString(line, 2).PadLeft(8, '0');
                for (int i = 0; i < binaryLine.Count(); i++)
                {
                    sboxTab[i] += binaryLine[i];
                }
            }

            Console.WriteLine("Lista funkcji Sbox");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            // display functions
            for (int i = 0; i < sboxTab.Count(); i++)
            {
                Console.WriteLine("Funkcja {0}. Liczba jedynek {1}", i, sboxTab[i].Count(x => x == '1'));
            }
            Console.ResetColor();

            return sboxTab.ToList();
        }

        /// <summary>
        /// Get all combinations of X elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        static IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
        {
            return from m in Enumerable.Range(0, 1 << list.Count)
                   select
                       from i in Enumerable.Range(0, list.Count)
                       where (m & (1 << i)) != 0
                       select list[i];
        }

        /// <summary>
        /// Xor two strings
        /// </summary>
        /// <param name="baseString">base string</param>
        /// <param name="newString">string to xor</param>
        /// <returns>xored string</returns>
        static string xorStrings(string baseString, string newString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < newString.Length; i++)
                sb.Append(int.Parse(newString[i].ToString()) ^ int.Parse(baseString[i].ToString()));
            String result = sb.ToString();

            return result;
        }

        /// <summary>
        /// Generate line functions
        /// </summary>
        /// <returns>List of line functions</returns>
        static List<string> GenerateBentFunctions(List<string> sboxList)
        {
            string[] bentFunctionsTab = new string[256];
            int bentFunctionsTabIndex = 8;
           
                for (int i = 0; i < bentFunctionsTab.Count(); i++)
                {
                    bentFunctionsTab[i] = "";
                }

                if (sboxList != null) //Take first 8 functions form sbox
                {
                    for (int tabElement = 0; tabElement < sboxList.Count(); tabElement++)
                    {
                        bentFunctionsTab[tabElement] = sboxList[tabElement];
                    }
                }
                else
                {
                    for (int i = 0; i < 256; i++)//Generate first 8 functions
                    {
                        var binaryCode = Convert.ToString(i, 2).PadLeft(8, '0');
                        for (int tabElement = 0; tabElement < binaryCode.Count(); tabElement++)
                        {
                            bentFunctionsTab[tabElement] += binaryCode[tabElement];
                        }
                    }
                }

            //Generate all possible combinations of first 8 bent functions
            var combinations = GetPowerSet(bentFunctionsTab.Take(8).ToList()).Select(subset => string.Join(" ", subset.Select(clr => clr.ToString()).ToArray())).Where(notExist => !bentFunctionsTab.Contains(notExist)).ToList();

            //Xoring 
            foreach (var comb in combinations)
            {
                var temp = comb.Split(' ');
                var result = temp[0];
                for (int i = 1; i < temp.Count(); i++)
                {
                    result = xorStrings(result, temp[i]);
                }
                bentFunctionsTab[bentFunctionsTabIndex] = result;
                bentFunctionsTabIndex++;
            }

            //Add last function with only zeros
            bentFunctionsTab[bentFunctionsTabIndex] = Convert.ToString(0, 2).PadLeft(256, '0');
            var test = bentFunctionsTab.Max(x => x.Length);
            return bentFunctionsTab.ToList();
        }

        /// <summary>
        /// Checking if a functions is nonlinearity
        /// </summary>
        /// <param name="sboxList">List of sbox functions</param>
        /// <param name="bentFunctions">List of bent functions</param>
        /// <returns>Table of nonlinearity</returns>
        static int[] CheckNonlinearity(List<string> sboxList, List<string> bentFunctionsList)
        {
            var resultTab = new int[sboxList.Count()];
            var resultTabIndex = 0;
            int elementTotal = 0;
            elementTotal = sboxList.Count() == 256 ? 255 : 256;
            foreach(var sbox in sboxList)
            {
                resultTab[resultTabIndex] = 256;
                foreach (var bentFunction in bentFunctionsList.Take(elementTotal))
                {
                    resultTab[resultTabIndex] = Math.Min(resultTab[resultTabIndex], xorStrings(sbox, bentFunction).Count(x => x == '1'));
                    resultTab[resultTabIndex] = Math.Min(resultTab[resultTabIndex], xorStrings(sbox, bentFunction).Count(x => x == '0'));
                }
                resultTabIndex++;
            }
            return resultTab;
        }

        static void Main(string[] args)
        {
            try
            {
                var sboxList = GetSboxFunctions();
                var bentFunctionsList = GenerateBentFunctions(null);
                var resultTab = CheckNonlinearity(sboxList, bentFunctionsList);
                var sboxFunctionsList = GenerateBentFunctions(sboxList);
                var resultSboxTab = CheckNonlinearity(sboxFunctionsList, bentFunctionsList);
                
                Console.WriteLine("\n\nWynik funkcji");
                // display results
                for (int i = 0; i < resultTab.Count(); i++)
                {
                    if (resultTab[i] == 110 || resultTab[i] == 112 || resultTab[i] == 108)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Funkcja {0}. Wyznaczona wartość nieliniowości: {1}", i, resultTab[i]);
                }
                Console.ResetColor();
                Console.WriteLine("\n\nWynik sbox");
                Console.WriteLine("Wyznaczona wartość nieliniowości sboxa to: {0}", resultSboxTab.Min());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
