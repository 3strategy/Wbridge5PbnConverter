using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
namespace wbridge5pbnconverter
{
    class Program
    {
        enum Series { spades, hearts, diamonds, Clubs };
        public static char[] refe = new char[] { '2', '3', '4','5','6','7','8','9','T','J','Q','K' };

        static void Main(string[] args)

        {
            var lines = from line in File.ReadLines(@"C:\Users\3stra\Dropbox\Guy\BridgeHands\hand.pbn")
                            //where line.Contains("[Deal ")
                        select new
                        {
                            Line = line
                        };
            using (StreamWriter sw = File.CreateText(@"C:\Users\3stra\Dropbox\Guy\BridgeHands\handfixed.pbn"))
            {
                foreach (var l in lines)

                {

                    if (l.Line.Contains("[Deal "))
                    {
                        
                        string s = FixDeal(l.Line);
                        Console.WriteLine("Pre Procesed Deal: " + l.Line);
                        Console.WriteLine("     Post Process: " + s);
                        sw.WriteLine(s);
                    }
                    else
                    {
                        Console.WriteLine(l.Line);
                        sw.WriteLine(l.Line);
                    }
                }
            }
            //string code = myString.Substring(myString.IndexOf(toBeSearched) + toBeSearched.Length);
            Console.ReadLine();
        }


        public static string FixDeal(string deal, bool fix = true)
        {
            string s = deal.Substring(9).ToUpper();
            s = s.Substring(0, s.Length - 2);
            var splithands = s.Split(' ');
            string newdeal = "";
            HashSet<char>[] h = new HashSet<char>[4];
            for (int i = 0; i < 4; i++)
            {
                h[i] = new HashSet<char>();
            }

            foreach (var hand in splithands)
            {
                if (hand.Length > 16)
                    Alert(ConsoleColor.Yellow , "hand :" + hand + " is too long");
                else if (hand.Length < 16)
                    Alert(ConsoleColor.Yellow, "hand :" + hand + " is too short");
                var cardsets = hand.Split('.');
                if (fix)
                {
                    string clubs = cardsets[2];
                    cardsets[2] = cardsets[3];
                    cardsets[3] = clubs;
                }
                for (int i = 0; i < 4; i++)
                    foreach (char c in cardsets[i])
                        if (!h[i].Add(c)) //report duplicate cards.
                            Alert(ConsoleColor.Red , ">>>> ++ " + c.ToString() + "  is duplicate in " + (Series)(i + 1 - Convert.ToInt32(fix)));


                newdeal += cardsets[0] + "." + cardsets[1] + "." + cardsets[2] + "." + cardsets[3] + " ";
            }
            for (int i = 0; i < 4; i++)
                foreach (var c in refe)
                    if (!h[i].Contains(c)) //report missing cards.
                        Alert(ConsoleColor.Green, ">>>> -- " + c + " is missing in " + (Series)(i + 1 - Convert.ToInt32(fix)));

            newdeal = "[Deal \"N:" + newdeal + "\"]";
            //[Deal "N:KJ3.JT74.QJ32.76 62.A95.K96.JT832 Q74.KQ2.AT85.KQ4 AT985.863.74.A95"]
            return newdeal;
        }
        public static void Alert(ConsoleColor color, string s)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

}
