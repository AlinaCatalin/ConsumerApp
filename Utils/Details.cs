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

        public static void SetPailDetails(this ProductionOrderConsumption item, ProductionOrderPailStatus pail) {
            item.PailNumber = pail.PailNumber;
            item.MPGStatus = pail.MPGStatus;
            item.MESStatus = pail.MESStatus;
            item.ErrorMessage = pail.ErrorMessage;
        }

        /// <summary>
        /// Set item quantity for consumption data
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pailCount"></param>
        /// <param name="pailNumber"></param>
        /// <param name="bom"></param>
        public static void ConsumptionItemQty(this ProductionOrderConsumption item, int pailCount, int pailNumber, ProductionOrderBom bom) {
            Random _random = new Random();

            if ((bom.ItemQtyUOM == "BUC") && (bom.ItemQty != pailCount)) {
                if (pailNumber == pailCount) {
                    item.ItemQty = bom.ItemQty;
                }

            } else {
                double qty = (double)bom.ItemQty / pailCount;

                if (bom.ItemQty == pailCount) {
                    item.ItemQty = (decimal)qty;
                } else {
                    /* add or subtract offset to item quantity */
                    var offset = (qty >= 5) ? ((double)qty) * 0.01 : ((double)qty) * 0.1;
                    var sign = _random.Next(0, 1);

                    item.ItemQty = (decimal)((sign == 1) ? (qty + offset) : (qty - offset));
                }
            }
        }

        public static void SavePailData(this ProductionOrderPailStatus pailStatus) {
            pailStatus.Timeout = "0";
            pailStatus.EndDate = DateTime.Now;
            pailStatus.NetWeight = pailStatus.GrossWeight + (decimal)((double)pailStatus.GrossWeight * 0.01);
            pailStatus.PailStatus = "PRLT";
            pailStatus.MPGRowUpdated = DateTime.Now;

        }

    }
}
