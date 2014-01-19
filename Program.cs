using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriSubtle.Utilities;

namespace TokenStringConsole {
  public class Program {
    public static void Main(string[] args) {
      double total = 0.0;
      for (int i = 0; i <= 1; i++) {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var items = TokenString.NewTokenString(113);
        var b = TokenString.GetIsValid(items.Value);
        Console.WriteLine(b);
        sw.Stop();
        //Console.WriteLine("Cycle {0} completed at {1:0.00} ms with {2} items", (i + 1), sw.Elapsed.TotalMilliseconds, items.Count);
        if (i > 0) { total += sw.Elapsed.TotalMilliseconds; }

        //foreach (var q in items) {
        //  Console.WriteLine(q.Value);
        //}
      }

      Console.WriteLine("Total = {0:#,##0.00}", total / 5);


      //foreach (var x in TokenString.AlphaNumericAny) {
      //  Console.WriteLine(x);
      //}

      //var lt = DateTime.Now.Ticks;

      //var ts = TokenString.NewTokenString();

      //Console.WriteLine(ts.HexValue);

      //var tb = Encoding.UTF8.GetBytes(ts);
      //var xb = BitConverter.GetBytes(lt);
      //foreach (var x in xb) {
      //  Console.WriteLine(x);
      //}
      //sw.Reset();
      //sw.Start();
      //Parallel.For(0, items.Count, i => {
      //  if (items[i] != "~###~-#####-~####") {
      //    Console.WriteLine("Failed Validation.");
      //  }
      //});
      //sw.Stop();

      //Console.WriteLine("{0} items", items.Count());
      //Console.WriteLine("Validation {0} ms", sw.Elapsed.TotalMilliseconds);

      Console.Write("Press any key to continue.... ");
      Console.ReadKey(true);
    }
  }
}