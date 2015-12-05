using NonFactors.Mvc.Lookup.Tests.Objects.Data;
using System.Linq;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestLookupProxy : GenericLookupProxy<TestModel>
    {
        private IQueryable<TestModel> models;

        public TestLookupProxy()
        {
            models = new Context().TestModels.OrderByDescending(model => model.Id);
        }

        protected override IQueryable<TestModel> GetModels()
        {
            return models;
        }
    }
}
