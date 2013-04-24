using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS
{
  public class StatusValue : IComparable<StatusValue>
  {
    private static Dictionary<string, string> _statusMap;
    public static string POSITIVE = "Pos";
    public static string NEGATIVE = "Neg";
    public static string NA = "NA";

    private static Dictionary<string, string> StatusMap
    {
      get
      {
        if (_statusMap == null)
        {
          _statusMap = new Dictionary<string, string>();
          _statusMap["y"] = POSITIVE;
          _statusMap["yes"] = POSITIVE;
          _statusMap["positive"] = POSITIVE;
          _statusMap["true"] = POSITIVE;
          _statusMap["pos"] = POSITIVE;
          _statusMap["er+"] = POSITIVE;
          _statusMap["p"] = POSITIVE;
          _statusMap["1"] = POSITIVE;

          _statusMap["n"] = NEGATIVE;
          _statusMap["no"] = NEGATIVE;
          _statusMap["negative"] = NEGATIVE;
          _statusMap["false"] = NEGATIVE;
          _statusMap["neg"] = NEGATIVE;
          _statusMap["er-"] = NEGATIVE;
          _statusMap["0"] = NEGATIVE;

          _statusMap["nd"] = NA;
          _statusMap["not performed"] = NA;
          _statusMap["na"] = NA;
          _statusMap["er?"] = NA;
          _statusMap["n/a"] = NA;
          _statusMap["?"] = NA;
          _statusMap["unk"] = NA;
          _statusMap["u"] = NA;
          _statusMap["--"] = NA;
        }
        return _statusMap;
      }
    }

    public static string TransferStatus(string oldValue)
    {
      var lValue = oldValue.Trim().ToLower();
      if (StatusMap.ContainsKey(lValue))
      {
        return StatusMap[lValue];
      }
      else
      {
        return lValue;
      }
    }

    private string _value;
    public string Value
    {
      get
      {
        return _value;
      }
      set
      {
        _value = TransferStatus(value);
      }
    }

    public StatusValue(string value = "NA")
    {
      this.Value = value;
    }

    public override string ToString()
    {
      return this.Value;
    }

    public override int GetHashCode()
    {
      return this.Value.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is StatusValue)
      {
        return this.Value.Equals((obj as StatusValue).Value);
      }

      return false;
    }

    public int CompareTo(StatusValue other)
    {
      return this.Value.CompareTo(other.Value);
    }
  }
}
