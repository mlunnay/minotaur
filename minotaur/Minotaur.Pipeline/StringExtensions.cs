using System.Linq;

namespace Minotaur.Pipeline
{
  public static class StringExtensions
  {
    public static bool IsInt(this string s)
    {
      if (s.StartsWith("-"))
        s = s.Remove(0, 1);
      return s.All(c => char.IsDigit(c));
    }

    public static bool IsUInt(this string s)
    {
      return s.All(c => char.IsDigit(c));
    }

    public static bool IsFloat(this string s)
    {
      if (s.StartsWith("-"))
        s = s.Remove(0, 1);
      bool firstPeriod = true;
      foreach (char c in s)
      {
        if (c == '.' && firstPeriod)
        {
          firstPeriod = false;
          continue;
        }
        if (char.IsDigit(c))
          continue;
        return false;
      }
      return true;
    }
  }
}
