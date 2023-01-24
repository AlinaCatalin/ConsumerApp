using DataEntity.Model.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerApp.Utils {
    public class PailListView {
        public string PailNumber { get; set; }
        public string Status { get; set; }

        public static PailListView CreateFromPail(ProductionOrderPailStatus pail) {
            return new PailListView { PailNumber = pail.PailNumber, Status = pail.PailStatus };
        }
    }
}
