using System;

namespace SubnetCalculator
{
    public class Params
    {
        private Dictionary<string, string> _params = new Dictionary<string, string>();



        public void AddItem(string v, string value)
        {
            if (_params == null) _params = new Dictionary<string, string>();
            _params.Add(v, value);
        }

  

        internal string Item(string v)
        {
            return _params[v];
        }
    }
}