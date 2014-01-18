using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VeriSubtle.Utilities;

namespace TokenStringConsole {
  public class Program {
    public static void Main(string[] args) {
      Stopwatch sw = new Stopwatch();
      sw.Start();
      var items = TokenString.NewTokens("#~###-#####-~####", 10000);
      sw.Stop();

      Console.WriteLine("{0} items", items.Count());
      Console.WriteLine("Creation {0} ms", sw.Elapsed.TotalMilliseconds);

      sw.Reset();
      sw.Start();
      Parallel.For(0, items.Count, i => {
        if (!TokenString.IsValid(items[i], "#~###-#####-~####")) {
          Console.WriteLine("Failed Validation.");
        }
      });
      sw.Stop();

      Console.WriteLine("{0} items", items.Count());
      Console.WriteLine("Validation {0} ms", sw.Elapsed.TotalMilliseconds);

      Console.Write("Press any key to continue.... ");
      Console.ReadKey(true);
    }
  }
}