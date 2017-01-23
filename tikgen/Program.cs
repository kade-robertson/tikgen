using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace tikgen 
{
    class Program 
    {
        static readonly int[] offsets = { 0x240, 0x140, 0x80, 0x240, 0x140, 0x80 };

        static void Main(string[] args) {
            if (args.Length == 2) {
                Console.WriteLine(string.Format("[i] Obtaining TMD for Title ID {0}...", args[0]));
                byte[] tmddata = null;
                using (var wc = new WebClient()) {
                    tmddata = wc.DownloadData(string.Format("http://ccs.cdn.c.shop.nintendowifi.net/ccs/download/{0}/tmd", args[0]));
                }
                if (tmddata == null) {
                    Console.WriteLine("[!] Unable to obtain TMD. Exiting.");
                    return;
                }
                Console.WriteLine("[i] Getting version data...");
                var type = tmddata[0x3];
                var vers = new byte[2];
                Array.Copy(tmddata, offsets[type] + 0x9C, vers, 0, 2);


                Console.WriteLine("[i] Building ticket data...");
                var outfile = build_ticket(vers, args[0], args[1]);
                Console.WriteLine(string.Format("[i] Writing to {0}.tik...", args[0]));
                File.WriteAllBytes(string.Format("{0}.tik", args[0]), outfile);
                Console.WriteLine("[i] Ticket generated successfully!");
            } else {
                Console.WriteLine("[!] Usage: tikgen.exe <Title ID> <Encrypted Title Key>");
                return;
            }
        }

        static byte[] build_ticket(byte[] vers, string titleid, string enckey) {
            var temp = Base.tikbase;
            var size = 0x140;
            var b_titleid = new byte[8];
            var b_enckey = new byte[16];

            for (int i = 0; i < 16; i += 2) {
                b_titleid[i / 2] = Convert.ToByte(titleid.Substring(i, 2), 16);
            }
            for (int i = 0; i < 32; i += 2) {
                b_enckey[i / 2] = Convert.ToByte(enckey.Substring(i, 2), 16);
            }

            Array.Copy(b_titleid, 0, temp, size + 0x9C, 8);
            Array.Copy(vers, 0, temp, size + 0xA6, 2);
            Array.Copy(b_enckey, 0, temp, size + 0x7F, 16);

            return temp;
        }
    }
}
