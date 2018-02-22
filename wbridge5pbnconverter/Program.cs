using System;
using System.IO;
using System.Linq;
namespace wbridge5pbnconverter
{
    class Program
    {
        static void Main(string[] args)
        {

            //FileStream fs = new FileStream(@"C:\Users\3stra\Dropbox\Guy\BridgeHands\hand.pbn", FileMode.OpenOrCreate,
            //FileAccess.ReadWrite, FileShare.None);
            //var f = File.ReadLines(@"C:\Users\3stra\Dropbox\Guy\BridgeHands\hand.pbn");

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
                        Console.WriteLine("Here is the deal:" + FixDeal(l.Line));
                        sw.WriteLine(FixDeal(l.Line));
                        //Console.WriteLine();
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
            foreach (var hand in splithands)
            {
                var cardsets = hand.Split('.');
                if (fix)
                {
                    string clubs = cardsets[2];
                    cardsets[2] = cardsets[3];
                    cardsets[3] = clubs;
                }
                newdeal += cardsets[0] + "." + cardsets[1] + "." + cardsets[2] + "." + cardsets[3] + " ";
            }
            newdeal = "[Deal N:" + newdeal + "]";
            //[Deal "N:KJ3.JT74.QJ32.76 62.A95.K96.JT832 Q74.KQ2.AT85.KQ4 AT985.863.74.A95"]
            return newdeal;
        }

    }

}
