using DataEntity.Model.Input;
using DataEntity.Model.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerApp.Utils {
    public static class Details {

        public static void SetBomDetails(this ProductionOrderConsumption item, ProductionOrderBom bom) {
            item.Item = bom.Item;
            item.ItemUom = bom.ItemQtyUOM;
        }
    }
}
