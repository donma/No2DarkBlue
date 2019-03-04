using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace No2DarkBlue
{
    public class Query<T> where T : DTableEntity, new()
    {
        public CloudStorageAccount CSAgent = null;
        public CloudTableClient CTClient = null;

        public string TableName { get; set; }
        public CloudTable CTable = null;


        public Query(string connectionString, string tableName, bool autoCreateTable = false)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("connectionString cannot be empty.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new Exception("tableName cannot be empty.");
            }

            try
            {

                CSAgent = CloudStorageAccount.Parse(connectionString);
                CTClient = CSAgent.CreateCloudTableClient();

                if (autoCreateTable)
                {

                    if (!CTable.ExistsAsync().Result)
                    {
                        var res = CTable.CreateIfNotExistsAsync().Result;
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception("No2DarkBlue can't create table : " + ex.Message);
            }
        }

        public void CreateTable()
        {

            CTable = CTClient.GetTableReference(TableName);
            try
            {
                if (!CTable.ExistsAsync().Result)
                {
                    var res = CTable.CreateIfNotExistsAsync().Result;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Dtor
        ~Query()
        {
            CSAgent = null;
            CTClient = null;
            CTable = null;
            GC.Collect();
        }
        #endregion

        /// <summary>
        /// Is DataExisted
        /// FAST
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        /// 2018/12/07
        /// <returns></returns>
        public bool IsDataExisted(string rowKey, string partitionKey = "")
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(rowKey)) throw new ArgumentNullException(nameof(rowKey));

            TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>();

            tableQuery.FilterString = TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                                        TableOperators.And,
                                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

            EntityResolver<string> resolver = (pk, rk, ts, props, etag) =>
            (
                                rk + "," + pk
            );

            List<string> res = new List<string>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<string> tableQueryResult =
                     CTable.ExecuteQuerySegmentedAsync(tableQuery, resolver, continuationToken).Result;

                continuationToken = tableQueryResult.ContinuationToken;

                res.AddRange(tableQueryResult.Results.Where(x => !string.IsNullOrEmpty(x)).ToList());
            } while (continuationToken != null);

            return res.Count > 0;

        }

        /// <summary>
        /// 取得所有Keys
        /// SLOW
        /// 5W dats return 5W keys for 10s
        /// 2018/12/07
        /// </summary>
        public DTableEntity[] AllDataKeys()
        {
            //var query = new TableQuery<DTableEntity>();
            var res = new ConcurrentList<DTableEntity>();

            var query = new TableQuery<DTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                     "RowKey", "PartitionKey"
                }
            };

            TableContinuationToken continuationToken = null;

            do
            {
                var page = CTable.ExecuteQuerySegmentedAsync(query, continuationToken).Result;
                continuationToken = page.ContinuationToken;
                if (page.Results != null)
                {
                    foreach (var c in page.Results)
                    {
                        res.Add(c);
                    }
                }
            }
            while (continuationToken != null);

            return res.ToArray();
        }
    
        /// <summary>
        /// 透過 RowKey 取得所有Keys 
        /// SLOW
        /// 5W datas search 5W rowkeys for 10s
        /// 2018/12/07
        /// </summary>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public DTableEntity[] AllDataKeysByRowKey(string rowKey = "")
        {

            var res = new ConcurrentList<DTableEntity>();

            var query = new TableQuery<DTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                     "RowKey", "PartitionKey"
                }
            };

            query.FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

            TableContinuationToken continuationToken = null;

            do
            {
                var page = CTable.ExecuteQuerySegmentedAsync(query, continuationToken).Result;
                continuationToken = page.ContinuationToken;
                if (page.Results != null)
                {
                    foreach (var c in page.Results)
                    {
                        res.Add(c);
                    }
                }
            }
            while (continuationToken != null);

            return res.ToArray();
        }
        
        /// <summary>
        /// 透過 PartitionKey 取得所有Keys 
        /// SLOW
        /// 5W datas search 5W rowkeys for 10s
        /// 2018/12/07
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public DTableEntity[] AllDataKeysByPK(string partitionKey = "")
        {
            
            var res = new ConcurrentList<DTableEntity>();

            var query = new TableQuery<DTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                     "RowKey", "PartitionKey"
                }
            };

            query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

            TableContinuationToken continuationToken = null;

            do
            {
                var page = CTable.ExecuteQuerySegmentedAsync(query, continuationToken).Result;
                continuationToken = page.ContinuationToken;
                if (page.Results != null)
                {
                    foreach (var c in page.Results)
                    {
                        res.Add(c);
                    }
                }
            }
            while (continuationToken != null);

            return res.ToArray();
        }
        
        /// <summary>
        /// 取得所有PartitionKeys
        /// FAST
        /// 5W datas for Disdinct 100 PK waste 0.3s
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public string[] AllPKs()
        {
            //var query = new TableQuery<DTableEntity>();
            var res = new ConcurrentList<string>();

            var query = new TableQuery<DTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                     "RowKey", "PartitionKey"
                }
            };
            query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, "");

            TableContinuationToken continuationToken = null;

            do
            {
                var page = CTable.ExecuteQuerySegmentedAsync(query, continuationToken).Result;
                continuationToken = page.ContinuationToken;
                if (page.Results != null)
                {
                    foreach (var c in page.Results)
                    {
                        if (!string.IsNullOrEmpty(c.PartitionKey))
                        {
                            res.Add(c.PartitionKey);
                        }
                    }
                }
            }
            while (continuationToken != null);

            return res.Distinct().ToArray();
        }

        /// <summary>
        /// 取得所有資料
        /// SLOW
        /// 5W for 26s
        /// 2018/12/07
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> AllDatas(params string[] limitProps)
        {
            var q = new TableQuery<T>();

            if (limitProps.Length > 0)
            {
                q.SelectColumns = new List<string>();
                q.SelectColumns.Add("RowKey");
                q.SelectColumns.Add("PartitionKey");
                foreach (var p in limitProps)
                {
                    q.SelectColumns.Add(p);
                }
            }
            
            return DatasByTableQuery(new TableQuery<T>());
        }

        /// <summary>
        /// 取得所有資料
        /// SLOW
        /// 5W for 2s
        /// 2018/12/07
        /// </summary>
        /// <returns></returns>
        public decimal AllDataCount()
        {

            TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "")).Select(new string[] { "RowKey" });

            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey("RowKey") ? props["RowKey"].StringValue : null;


            var count = 0;
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<string> tableQueryResult =
                     CTable.ExecuteQuerySegmentedAsync(tableQuery, resolver, continuationToken).Result;

                continuationToken = tableQueryResult.ContinuationToken;
                count += tableQueryResult.Results.Count;


            } while (continuationToken != null);




            return count;
        }


        /// <summary>
        /// Get Datas By Keys
        /// FAST
        /// 5W get 5 data and some duplicate get result 4  for 0.26s
        /// </summary>
        /// <param name="tableKeyPairs"></param>
        /// <returns></returns>
        public IEnumerable<T> DatasByPKRKs(params TableKeyPair[] tableKeyPairs)
        {
            //檢查參數是否傳入正確
            if (tableKeyPairs == null) throw new ArgumentNullException(nameof(tableKeyPairs));

            foreach (var tkps in tableKeyPairs)
            {
                if (string.IsNullOrEmpty(tkps.RowKey))
                {
                    throw new Exception("RowKey cant be null.");
                }
            }


            tableKeyPairs = tableKeyPairs.GroupBy(x => x.RowKey + "," + x.PartitionKey).Select(g => g.First()).ToArray();

            var res = new ConcurrentList<T>();


            Parallel.ForEach(tableKeyPairs, (tkp) =>
            {
                var obj = DataByPKRK(tkp.RowKey, tkp.PartitionKey);
                if (obj != null)
                {
                    res.Add(obj);
                }
            });

            return res;
        }

        /// <summary>
        /// 取得單一資料
        /// FAST
        /// 5W datas get one data for 0.2
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public T DataByPKRK(string rowKey, string partitionKey = "")
        {
            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var res = CTable.ExecuteAsync(operation).Result.Result;
            return (T)res;
        }

        /// <summary>
        /// 取得資料 By PartictionKey
        /// NORMAL
        /// 取回五萬筆花了23 秒，取回只有一筆花了0.39
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="props">如果不是null ，就只撈你給的Properties 回來</param>
        /// <returns></returns>
        public IEnumerable<T> DatasByPK(string partitionKey = "", params string[] props)
        {

            var query = new TableQuery<T>();
            if (props.Length > 0)
            {
                query.SelectColumns = new List<string>();
                foreach (var p in props)
                {
                    query.SelectColumns.Add(p);
                }
            }
            query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

            return DatasByTableQuery(query);

        }

        /// <summary>
        /// 取得資料 By RowKey
        /// NORMAL
        /// 取回五萬筆花了23 秒，取回只有一筆花了0.39
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="props">如果不是null ，就只撈你給的Properties 回來</param>
        /// <returns></returns>
        public IEnumerable<T> DatasByRK(string rowKey = "", params string[] props)
        {

            var query = new TableQuery<T>();
            if (props.Length > 0)
            {
                query.SelectColumns = new List<string>();
                foreach (var p in props)
                {
                    query.SelectColumns.Add(p);
                }
            }
            return DatasByTableQuery(query);
        }

        public IEnumerable<T> DatasByExpression(Expression<Func<T, bool>> expression)
        {
            var query = new TableQuery<T>();
            var q = query.Where(expression).AsTableQuery();
            return DatasByTableQuery(q);

        }

        /// <summary>
        /// 搜尋資料 by 複雜條件
        /// sample:
        /// TableQuery.GenerateFilterCondition("Name", QueryComparisons.LessThanOrEqual, "USERT")
        /// </summary>
        /// <param name="firstFilterCondition"></param>
        /// <param name="tableOperators"></param>
        /// <param name="othersConditions"></param>
        /// <returns></returns>
        public IEnumerable<T> DatasByFilterCondition(string firstFilterCondition, string[] tableOperators, string[] othersConditions)
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(firstFilterCondition)) throw new ArgumentNullException(nameof(firstFilterCondition));
            if (tableOperators != null && othersConditions == null) throw new ArgumentNullException(nameof(othersConditions));
            if (othersConditions != null && tableOperators == null) throw new ArgumentNullException(nameof(tableOperators));
            if (othersConditions != null && tableOperators != null)
            {
                if (othersConditions.Length != tableOperators.Length)
                {
                    throw new Exception("tableOperators and othersConditions length should be the same.");
                }
            }

            var query = new TableQuery<T>();

            var res = new ConcurrentList<T>();

            if (tableOperators == null)
            {

                query.FilterString = firstFilterCondition;
            }
            else
            {

                query.FilterString = firstFilterCondition;
                for (var i = 0; i < tableOperators.Length; i++)
                {
                    string finalFilter = TableQuery.CombineFilters(query.FilterString
                 , tableOperators[i],
                othersConditions[i]);

                    query.FilterString = finalFilter;
                }


            }

            return DatasByTableQuery(query);

        }

        /// <summary>
        /// 搜尋資料 by Free Style Filter String
        /// sample:
        /// TableQuery.GenerateFilterCondition("Name", QueryComparisons.LessThanOrEqual, "USERT")
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public IEnumerable<T> DatasByFilterString(string filterString, params string[] props)
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(filterString)) throw new ArgumentNullException(nameof(filterString));

            var query = new TableQuery<T>();

            if (props.Length > 0)
            {
                query.SelectColumns = new List<string>();
                query.SelectColumns.Add("RowKey");
                query.SelectColumns.Add("PartitionKey");
                foreach (var p in props)
                {
                    query.SelectColumns.Add(p);
                }
            }
            return DatasByTableQuery(query);

        }


        /// <summary>
        /// DatasByTableQuery
        /// </summary>
        /// <param name="tableQuery"></param>
        /// <returns></returns>
        public IEnumerable<T> DatasByTableQuery(TableQuery<T> tableQuery)
        {

            var res = new ConcurrentList<T>();

            TableContinuationToken continuationToken = null;
            do
            {
                var page = CTable.ExecuteQuerySegmentedAsync(tableQuery, continuationToken).Result;
                continuationToken = page.ContinuationToken;
                if (page.Results != null)
                {
                    foreach (var obj in page.Results)
                    {
                        res.Add(obj);
                    }
                }
            }
            while (continuationToken != null);

            return res;

        }

    }
}
