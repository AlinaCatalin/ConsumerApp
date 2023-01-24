using DataEntity.Model.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerApp.Utils {
    public class CMDListView {
        public string POID { get; set; }
        public string Status { get; set; }

        public static CMDListView CreateFromPO(ProductionOrder po) {
            return new CMDListView { POID = po.POID, Status = po.Status };
        }
    }
}
