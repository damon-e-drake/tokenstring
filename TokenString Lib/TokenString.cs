using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace VeriSubtle.Utilities {
  public struct TokenString {
    // # = AlphaNumeric (any case)
    // _ = AlphaNumeric (lowercase)
    // + = AlphaNumeric (uppercase)
    // @ = AlphaOnly (any case)
    // * = Number
    // ~ = Repeater
    private static char[] AlphaNumericAny = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPARSTUVWXYZ0123456789".ToCharArray();
    private static char[] AlphaOnly = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPARSTUVWXYZ".ToCharArray();
    private static char[] AlphaNumericLower = "abcdefghjkmnpqrstuvwxyz0123456789".ToCharArray();
    private static char[] AlphaNumericUpper = "ABCDEFGHJKMNPARSTUVWXYZ0123456789".ToCharArray();
    private static char[] Numbers = "0123456789".ToCharArray();
    private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

    public static string NewToken(string TokenMask) {
      char r = ' ';
      var sb = new char[TokenMask.Length];
      var mb = new byte[TokenMask.Length];
      rng.GetBytes(mb);

      Parallel.For(0, TokenMask.Length, i => {
        var c = TokenMask[i];
        switch (c) {
          case '#':
            sb[i] = AlphaNumericAny[(int)mb[i] % AlphaNumericAny.Length];
            break;
          case '_':
            sb[i] = AlphaNumericLower[(int)mb[i] % AlphaNumericLower.Length];
            break;
          case '+':
            sb[i] = AlphaNumericUpper[(int)mb[i] % AlphaNumericUpper.Length];
            break;
          case '*':
            sb[i] = Numbers[(int)mb[i] % Numbers.Length];
            break;
          case '@':
            sb[i] = AlphaOnly[(int)mb[i] % AlphaOnly.Length];
            break;
          case '~':
            r = r == ' ' ? AlphaNumericAny[(int)mb[i] % AlphaNumericAny.Length] : r;
            sb[i] = r;
            break;
          default:
            sb[i] = c;
            break;
        }
      });

      return new string(sb);
    }
    public static List<string> NewTokens(string TokenMask, int Capacity, int MaxCycles = 5) {
      var list = new BlockingCollection<string>();
      var cycles = 0;

      while (cycles < MaxCycles && list.Count < Capacity) {
        Parallel.For(1, Capacity + 1, (i, state) => {
          if (cycles >= MaxCycles || list.Count == Capacity) { state.Stop(); }

          var f = NewToken(TokenMask);
          if (!list.Contains(f)) { list.Add(f); }
        });

        cycles++;
      }

      Console.WriteLine(cycles);
      return list.Take(Capacity).ToList();
    }
    public static bool IsValid(string TokenString, string TokenMask) {
      var vals = new BlockingCollection<bool>();
      if (TokenString.Length != TokenMask.Length) { return false; }

      var r = ' ';
      var f = TokenString.ToCharArray();
      var m = TokenMask.ToCharArray();

      Parallel.For(0, TokenString.Length, (i, state) => {
        var isvalid = false;

        var c = m[i];
        switch (c) {
          case '#':
            isvalid = AlphaNumericAny.Contains(f[i]);
            break;
          case '_':
            isvalid = AlphaNumericLower.Contains(f[i]);
            break;
          case '+':
            isvalid = AlphaNumericUpper.Contains(f[i]);
            break;
          case '*':
            isvalid = Numbers.Contains(f[i]);
            break;
          case '@':
            isvalid = AlphaOnly.Contains(f[i]);
            break;
          case '~':
            r = r == ' ' ? f[i] : r;
            isvalid = (r == f[i]);
            break;
          default:
            isvalid = (f[i] == m[i]);
            break;
        }
        vals.Add(isvalid);
        if (!isvalid) { state.Stop(); }
      });

      return vals.Contains(false) ? false : true;
    }
  }
}
