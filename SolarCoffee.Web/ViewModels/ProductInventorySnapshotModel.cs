using System;
using System.Collections.Generic;

namespace SolarCoffee.Web.ViewModels
{
    public class ProductInventorySnapshotModel
    {
        public List<int> QuantityOnHand { get; set; }
        public int ProductId { get; set; }
    }


    public class SnapshotResponse
    {
        public List<ProductInventorySnapshotModel> ProductInventorySnapshots { get; set; }
        public List<DateTime> Timeline { get; set; }
    }
}