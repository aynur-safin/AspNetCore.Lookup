using System;

namespace NonFactors.Mvc.Lookup
{
    [Serializable]
    public class LookupException : Exception
    {
        public LookupException(String message)
            : base(message)
        {
        }
    }
}
