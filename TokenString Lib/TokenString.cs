using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VeriSubtle.Utilities {
  public static class TokenStringUtils {
    public static string TokenFormat(this string token, string format, bool uppercase = false) {
      token = token.Replace("-", "");

      if (!Regex.IsMatch(token, @"^[a-zA-Z0-9]+$")) { throw new FormatException("Only Hexadecimal string values are accepted."); }
      if (!Regex.IsMatch(format, @"^[-0-9]+$")) { throw new FormatException("Formatting string was not in the correct format."); }

      var limits = format.Split('-').Select(int.Parse).ToArray();
      if (limits.Sum() != token.Length) { throw new ArgumentOutOfRangeException("Provided format does not account for all token characters."); }

      StringBuilder sb = new StringBuilder(token.Length + limits.Length);

      var offset = 0;
      foreach (var i in limits) {
        sb.AppendFormat("{0}-", token.Substring(offset, i));
        offset += i;
      }
      sb.Remove(sb.Length - 1, 1);

      return uppercase ? sb.ToString() : sb.ToString().ToLower();
    }

    public static bool IsValid(this string Token, int MaskID) {
      var vals = new BlockingCollection<bool>();
      string s = null;

      if (!TokenString.AvailableMasks.TryGetValue(MaskID, out s)) { return false; }
      if (Token.Length != s.Length) { return false; }

      var maskbit = s[s.IndexOf('%')] >= 0 ? s[s.IndexOf('%')] : ' ';
      if (maskbit == ' ') { return false; }

      var r = ' ';
      var f = Token.ToCharArray();
      Parallel.For(0, f.Length - 1, (i, state) => {
        var isvalid = false;

        char[] x = null;
        if (TokenString.Masking.TryGetValue(s[i], out x)) {
          isvalid = x.Contains(f[i]);
        }

        if (s[i] == '%') {
          isvalid = f[i] == maskbit;
          return;
        }

        if (s[i] == '~') {
          r = r == ' ' ? f[i] : r;
          isvalid = (r == f[i]);
        }

        isvalid = isvalid ? true : (f[i] == s[i]);

        vals.Add(isvalid);
        if (!isvalid) { state.Stop(); }
      });

      return vals.Contains(false) ? false : true;
    }
  }
  public struct TokenString {
    private static char[] AlphaLower = "abcdefghjkmnpqrstuvwxyz".ToCharArray();
    private static char[] AlphaUpper = "ABCDEFGHJKMNPARSTUVWXYZ".ToCharArray();
    private static char[] Numbers = "0123456789".ToCharArray();
    private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

    public static readonly Dictionary<char, char[]> Masking = new Dictionary<char, char[]> {
      { '#', AlphaLower.Concat(AlphaUpper).Concat(Numbers).ToArray() },
      { '@', AlphaLower.Concat(AlphaUpper).ToArray() },
      { '*', Numbers}
    };

    public static Dictionary<int, string> AvailableMasks = new Dictionary<int, string> {
      { 121, "@@~#~#@@*~#%#~#" },
      { 113, "####~#####~#####%#~#" }
    };

    public string Value { get; private set; }
    public string HexValue {
      get {
        return BitConverter.ToString(Encoding.UTF8.GetBytes(Value)).Replace("-", "");
      }
    }
    public string Base64 {
      get {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(Value));
      }
    }
    public override bool Equals(object o) {
      if (o is TokenString) { return (this.Value == ((TokenString)o).Value); }
      return false;
    }
    public override int GetHashCode() {
      return this.Value.GetHashCode();
    }
    public override string ToString() {
      return this.Value;
    }


    public static TokenString NewTokenString(int MaskID) {
      var mask = AvailableMasks[MaskID];
      byte r = 0;
      var sb = new byte[mask.Length];
      var mk = ASCIIEncoding.ASCII.GetBytes(mask);
      var mb = new byte[mask.Length];
      rng.GetBytes(mb);

      Parallel.For(0, mk.Length, i => {
        var c = (char)mk[i];
        char[] x = null;

        if (c == '%') {
          sb[i] = (byte)((char)MaskID);
          return;
        }

        if (c == '~') {
          r = r == 0 ? (byte)Masking['#'][(int)mb[i] % Masking['#'].Length] : r;
          sb[i] = r;
          return;
        }

        if (Masking.TryGetValue(c, out x)) {
          sb[i] = (byte)x[(int)mb[i] % x.Length];
          return;
        }

        sb[i] = (byte)c;
      });

      return new TokenString { Value = Encoding.UTF8.GetString(sb) };
    }
    public static List<TokenString> NewTokenStrings(int tokenMask, int Capacity) {
      var tokens = new BlockingCollection<TokenString>();

      Parallel.For(1, (int)(Capacity * 1.08), (i, state) => { tokens.Add(NewTokenString(tokenMask)); });

      return tokens.Distinct().Take(Capacity).ToList();
    }
    

    public static bool operator ==(TokenString x, TokenString y) {
      return x.Equals(y);
    }
    public static bool operator !=(TokenString x, TokenString y) {
      return !x.Equals(y);
    }
    public static implicit operator string(TokenString ts) {
      return ts.Value;
    }
  }
}