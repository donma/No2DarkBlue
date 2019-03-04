using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace No2DarkBlue
{
    public class TableKeyPair
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }

        public TableKeyPair(string rowKey, string partitionKey)
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(rowKey)) throw new ArgumentNullException(nameof(rowKey));
            RowKey = rowKey;
            PartitionKey = partitionKey;
        }
    }
}
