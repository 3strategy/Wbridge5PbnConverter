using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace wbridge5pbnconverter
{
    class Program
    {
        enum Series { Spades, Hearts, Diamonds, Clubs };
        public static char[] reference = new char[] { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };

        static void Main(string[] args)
        {
            try
            {



                string filename;
                if (args.Length > 0)
                    filename = args[0];
                else
                    filename = @"C:\Users\3stra\Dropbox\Guy\BridgeHands\hand.pbn";
                Console.WriteLine("Input the card order: type \r\n S for SHCD\r\n H for HSDC\r\n " +
                    "D for DSHC\r\n C for CHSD\r\n  SHDC or 'Enter' for standard input order (no reordring will be perfomed)");
                string order = Console.ReadLine().ToUpper();
                var lines = from line in File.ReadLines(filename)
                                //where line.Contains("[Deal ")
                            select new
                            {
                                Line = line
                            };
                string newFile = filename.Substring(0, filename.Length - 4) + ".PRC.pbn";
                using (StreamWriter sw = File.CreateText(newFile))
                {
                    foreach (var l in lines)
                    {
                        if (l.Line.Contains("[Deal "))
                        {

                            string s = FixDeal(l.Line, order);
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

                Console.WriteLine("press w to open in wBridge5");
                string ss = Console.ReadLine();
                if (ss.ToUpper() == "W")
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"C:\wbridge5\Wbridge5.exe";
                    startInfo.Arguments = "\"" + newFile + "\"";
                    Process.Start(startInfo);
                }

            }
            catch (Exception ex)
            {
                //var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = new StackTrace(ex, true).GetFrame(0);
                // Get the line number from the stack frame
                //var line = frame.GetFileLineNumber();
                //var col = frame.GetFileColumnNumber();
                Alert(ConsoleColor.Red, ex.Message + "\t line: " + frame.GetFileLineNumber() + "\t col: " + frame.GetFileColumnNumber());
                Console.ReadLine();
            }
        }


        public static string FixDeal(string deal, string order)
        {
            if (order == "")
                order = "SHDC";//standard pbn with no reordering.

            string spades, hearts, clubs, diamonds;
            HashSet<char> spadeHash = new HashSet<char>();
            HashSet<char> heartHash = new HashSet<char>();
            HashSet<char> diamondHash = new HashSet<char>();
            HashSet<char> clubHash = new HashSet<char>();

            string s = deal.Substring(9).ToUpper();
            s = s.Substring(0, s.Length - 2);
            //RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            s = regex.Replace(s, " ");
            s = s.TrimEnd();
            var splithands = s.Split(' ');
            string newdeal = "";
            HashSet<char>[] h = new HashSet<char>[4];
            for (int i = 0; i < 4; i++)
            {
                h[i] = new HashSet<char>();
            }

            foreach (var hand in splithands)
            {
                //string cardsets = new string[4];
                //count cards - report shortness or excess
                if (hand.Length > 16)
                    Alert(ConsoleColor.Yellow, "hand :" + hand + " is too long");
                else if (hand.Length < 16)
                    Alert(ConsoleColor.Yellow, "hand :" + hand + " is too short");
                var cardsets = hand.Split('.');
                if (cardsets.Length < 4)
                { //fix data set for short hands (to avoid index out of range in voids)
                    Alert(ConsoleColor.Blue, " *** hand " + hand + " *** has unspecified VOID. Add a dot (.) where appropriate to specify the void. \r\n file will not be analysed further");
                    return deal;
                }
                //reorder hands
                switch (order)
                {
                    case "S":
                        spades = cardsets[0];
                        hearts = cardsets[1];
                        clubs = cardsets[2];
                        diamonds = cardsets[3];
                        break;
                    case "H":
                        hearts = cardsets[0];
                        spades = cardsets[1];
                        diamonds = cardsets[2];
                        clubs = cardsets[3];
                        break;
                    case "D":
                        diamonds = cardsets[0];
                        spades = cardsets[1];
                        hearts = cardsets[2];
                        clubs = cardsets[3];
                        break;
                    case "C":
                        clubs = cardsets[0];
                        hearts = cardsets[1];
                        spades = cardsets[2];
                        diamonds = cardsets[3];
                        break;
                    case "SHDC":
                        spades = cardsets[0];
                        hearts = cardsets[1];
                        diamonds = cardsets[2];
                        clubs = cardsets[3];
                        break;
                    default:
                        Console.WriteLine("You requested a weird deck order");
                        return deal;
                }

                //append hand cards to newdeal in correct pbn order
                newdeal += spades + "." + hearts + "." + diamonds + "." + clubs + " ";

                //push into hashes and report duplicates
                foreach (char c in spades)
                    if (!spadeHash.Add(c))
                        Alert(ConsoleColor.Red, ">>>> ++ " + c.ToString() + "  is duplicate in Spades");
                foreach (char c in hearts)
                    if (!heartHash.Add(c))
                        Alert(ConsoleColor.Red, ">>>> ++ " + c.ToString() + "  is duplicate in Hearts");
                foreach (char c in diamonds)
                    if (!diamondHash.Add(c))
                        Alert(ConsoleColor.Red, ">>>> ++ " + c.ToString() + "  is duplicate in Diamonds");
                foreach (char c in clubs)
                    if (!clubHash.Add(c))
                        Alert(ConsoleColor.Red, ">>>> ++ " + c.ToString() + "  is duplicate in Clubs");
            }
            //cross hashes against reference to identify the specific missing card(s).
            foreach (var c in reference)
                if (!spadeHash.Contains(c)) //report missing cards.
                    Alert(ConsoleColor.Green, ">>>> -- " + c + " is missing in Spades");
            foreach (var c in reference)
                if (!heartHash.Contains(c)) //report missing cards.
                    Alert(ConsoleColor.Green, ">>>> -- " + c + " is missing in Hearts");
            foreach (var c in reference)
                if (!diamondHash.Contains(c)) //report missing cards.
                    Alert(ConsoleColor.Green, ">>>> -- " + c + " is missing in Diamonds");
            foreach (var c in reference)
                if (!clubHash.Contains(c)) //report missing cards.
                    Alert(ConsoleColor.Green, ">>>> -- " + c + " is missing in Clubs");

            newdeal = "[Deal \"N:" + newdeal.TrimEnd() + "\"]";
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
