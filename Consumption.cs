using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsumerApp;

using DataEntity.Config;
using DataEntity.Model.Input;
using DataEntity.Model.Output;
using ConsumerApp.Utils;
using System.Collections;

namespace ConsumerApp {
    public class Consumption {

        public Thread _thread;
        public int _timeToSleep;

        public int[] _arrayStatus = new int[] { 1, 2 };
        public Random _random = new Random();

        public int ID = 1;

        public int flag = 0;

        /// <summary>
        /// Change status cmd 
        /// random - 1 ELB
        /// random - 2 PRLS
        /// random - 3 PRLI
        /// random - 4 PRLT
        /// </summary>
        public void ConsumptionSteps() {
            flag = 1;
            while (flag == 1) {

                flag = 0;
                using (var session = SqliteDB.Instance.GetSession()) {

                    using (var transaction = session.BeginTransaction()) {

                        var productionOrder = session.Query<ProductionOrder>().ToList();

                        productionOrder.ForEach(prodOrder => {

                            var poPailStatus = session.Query<ProductionOrderPailStatus>().Where(p => p.POID == prodOrder.POID).ToList();
                            var bom = session.Query<ProductionOrderBom>().Where(a => a.POID == prodOrder.POID).ToList();
                            var pailCount_PRTL = session.Query<ProductionOrderPailStatus>().Where(a => a.POID == prodOrder.POID && a.PailStatus == "PRLT").ToList();
                            var pailCount_PRTI = session.Query<ProductionOrderPailStatus>().Where(a => a.POID == prodOrder.POID && a.PailStatus == "PRLI").ToList();

                            SetTimeToSleep(prodOrder.MESStatus);

                            var status = prodOrder.Status;

                            switch (prodOrder.Status) {
                                case "ELB":
                                    this.ELBCase(session, prodOrder);
                                    
                                    break;
                                case "PRLS":
                                    poPailStatus.ForEach(pailStatus => {

                                        /*
                                         * thread sleep = timeout
                                         * wait until the pail is done
                                         * var timeToSleep = pailStatus.Timeout; //TODO: To be removed
                                         */

                                        bom.ForEach(bomItem => {
                                            var checkConsumption = session.Query<ProductionOrderConsumption>().Where(p => p.POID == prodOrder.POID &&
                                                                                                                        p.PailNumber == pailStatus.PailNumber&&
                                                                                                                        p.Item == bomItem.Item).Count();
                                            if (checkConsumption == 0) {
                                                var item = CreateConsupmtion(prodOrder);
                                                item.SetBomDetails(bomItem);
                                                session.Save(ConsumptionData(bomItem, prodOrder, pailStatus, poPailStatus.Count));
                                            }
                                            
                                        });

                                    });

                                    prodOrder.Status = "PRLT";
                                    flag = 1;
                                    session.Update(prodOrder);

                                    Thread.Sleep(_timeToSleep);

                                    break;
                                case "PRLI":
                                    //prodOrder.Status = "PRLS";
                                    prodOrder.Status = "PRLI";

                                    flag = 1;
                                    session.Update(prodOrder);

                                    Thread.Sleep(_timeToSleep);

                                    break;
                                case "PRLT":
                                    if (pailCount_PRTL.Count == prodOrder.PlannedQtyBUC)
                                        prodOrder.Status = "PRLT";

                                    session.Update(prodOrder);
                                    Thread.Sleep(_timeToSleep);

                                    break;
                            }

                        });
                        transaction.Commit();
                    }
                }
            }
        }

        private void ELBCase(NHibernate.ISession session, ProductionOrder prodOrder) {
            var x = _random.Next(_arrayStatus.Length);
            prodOrder.Status = (_arrayStatus[x] == 1) ? "ELB" : "PRLS";

            flag = 1;
            session.Update(prodOrder);

            Thread.Sleep(_timeToSleep);
        }

        public void SetTimeToSleep(int status) {
            _timeToSleep = status *1000 * 75 / 100;
        }


        public void StartThread() {
            _thread = new Thread(new ThreadStart(this.ConsumptionSteps)) {
                IsBackground = true
            };
            _thread.Priority = ThreadPriority.Normal;

            _thread.Start();
        }

        private ProductionOrderConsumption CreateConsupmtion(ProductionOrder productionOrder) {
            ProductionOrderConsumption item = new();

            item.ID = ID++;
            item.CreationDate = DateTime.Today;
            item.POID = productionOrder.POID;
            item.MaterialID = productionOrder.MaterialID;
            //item.PailNumber = pailStatus.PailNumber;
            //item.Item = bom.Item;
            //item.ItemUom = bom.ItemQtyUOM;
            //item.SetBomDetails(bom);

            item.ItemStorageLoc = "1";
            //item.MPGStatus = pailStatus.MPGStatus;
           // item.MESStatus = pailStatus.MESStatus;
            //item.ErrorMessage = pailStatus.ErrorMessage;
            item.MPGRowUpdated = DateTime.Now;
            //item.ItemQty = ConsumptionItemQty(pailCount, pailNumber, bom);

            return item;
        }





        public ProductionOrderConsumption ConsumptionData(ProductionOrderBom bom,
                           ProductionOrder productionOrder,
                           ProductionOrderPailStatus pailStatus,
                           int pailCount) {

            ProductionOrderConsumption consumption = new ProductionOrderConsumption();

            consumption.ID = ID++;
            consumption.CreationDate = DateTime.Today;
            consumption.POID = productionOrder.POID;
            consumption.MaterialID = productionOrder.MaterialID;
            //consumption.PailNumber = pailStatus.PailNumber;
            consumption.PailNumber = 
            consumption.Item = bom.Item;
            consumption.ItemStorageLoc = "1";
            consumption.MPGStatus = pailStatus.MPGStatus;
            consumption.MESStatus = pailStatus.MESStatus;
            consumption.ErrorMessage = pailStatus.ErrorMessage;
            consumption.MPGRowUpdated = DateTime.Now;

            int pailNumber = int.Parse(pailStatus.PailNumber);

            consumption.ItemQty = ConsumptionItemQty(pailCount, pailNumber,bom);
            consumption.ItemUom = bom.ItemQtyUOM;
            pailStatus.Timeout = "0";
            pailStatus.EndDate = DateTime.Now;
            pailStatus.NetWeight = GetNetWeight(pailStatus);
            pailStatus.PailStatus = "PRLT";
            pailStatus.MPGRowUpdated = DateTime.Now;

            return consumption;
        }
        
        public decimal GetNetWeight(ProductionOrderPailStatus pailStatus) {
            return pailStatus.GrossWeight + (decimal)((double)pailStatus.GrossWeight * 0.01);
        }

        /// <summary>
        /// Return item quantity for consumption data
        /// </summary>
        /// <param name="pailCount"></param>
        /// <param name="pailNumber"></param>
        /// <param name="bom"></param>
        /// <returns></returns>
        public decimal ConsumptionItemQty(int pailCount, int pailNumber, ProductionOrderBom bom) {

            if ((bom.ItemQtyUOM == "BUC") && (bom.ItemQty != pailCount)) {
                if (pailNumber == pailCount) {
                    return bom.ItemQty;
                }

            } else {
                double qty = (double)bom.ItemQty / pailCount;

                if (bom.ItemQty == pailCount) {
                    return (decimal)qty;
                } else {
                    /* add or subtract offset to item quantity */
                    var offset = (qty >= 5) ? ((double)qty) * 0.01 : ((double)qty) * 0.1;
                    var sign = _random.Next(0, 1);

                    return (decimal)((sign == 1) ? (qty + offset) : (qty - offset));
                }
            }

            return bom.ItemQty;
        }

        /// <summary>
        /// Get all CMD and show in ListView 
        /// </summary>
        /// <returns></returns>
        public List<CMDListView> ShowCMD() {
            List<CMDListView> items = new List<CMDListView>();
            using (var session = SqliteDB.Instance.GetSession()) {

                using (var transaction = session.BeginTransaction()) {
                    var productionOrder = session.Query<ProductionOrder>().ToList();

                    productionOrder.ForEach(prodOrder => {
                        items.Add(new CMDListView() { POID = prodOrder.POID, Status = prodOrder.Status });
                    });
                }
            }
            return items;
        }

        public List<PailListView> ShowPails(string poid) {
            List<PailListView> items = new List<PailListView>();
            using (var session = SqliteDB.Instance.GetSession()) {

                using (var transaction = session.BeginTransaction()) {
                    var pailStatus = session.Query<ProductionOrderPailStatus>().Where(p => p.POID == poid).ToList();
                    pailStatus.ForEach(pail => {
                        items.Add(new PailListView() { PailNumber = pail.PailNumber, Status = pail.PailStatus });
                    });
                }
            }
            return items;
        }
        
    }
}
