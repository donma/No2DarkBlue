using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace No2DarkBlue
{

    public class Operator
    {

        public CloudStorageAccount CSAgent = null;
        public CloudTableClient CTClient = null;

        public string TableName { get; set; }
        public CloudTable CTable = null;
        public Operator(string connectionString, string tableName, bool autoCreateTable = false)
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
                TableName = tableName;
                CSAgent = CloudStorageAccount.Parse(connectionString);
                CTClient = CSAgent.CreateCloudTableClient();
                CTable = CTClient.GetTableReference(TableName);

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

        /// <summary>
        /// Update data with confirm Etag
        /// 修改資料，並且確保資料的一致性的更新
        /// 請注意:如果資料庫沒有該資料會 throw Exception
        /// 所以建議你的做法就是 取出來 該資料 確保該 Etag 是最新的，如果出錯就是代表
        /// 你的 ETag 不是最新或是根本沒有那筆資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void UpdateWithConfirmETag(DTableEntity data)
        {
            //檢查參數是否傳入正確
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.RowKey)) throw new ArgumentNullException(nameof(data.RowKey));

            try
            {

                var operation = TableOperation.Replace(data);


                var res = CTable.ExecuteAsync(operation).Result;

            }
            catch (Exception ex)
            {

                throw ex;

            }
            finally
            {
                // DataWriter.DelLock(Role.RoleKey, Collection, id);
            }


        }



        /// <summary>
        /// Update data.
        /// 修改資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Operator Update(ITableEntity data)
        {
            //檢查參數是否傳入正確
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.RowKey)) throw new ArgumentNullException(nameof(data.RowKey));
            if (string.IsNullOrEmpty(data.ETag))
            {

                data.ETag = "*";
            }
            try
            {

                var operation = TableOperation.InsertOrReplace(data);

                var tableRequestOptions = new TableRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(500), 4),

                };

                var res = CTable.ExecuteAsync(operation, tableRequestOptions, null).Result;

            }
            catch (Exception ex)
            {

                throw ex;

            }
            finally
            {
                // DataWriter.DelLock(Role.RoleKey, Collection, id);
            }


            return this;

        }


        /// <summary>
        /// Delete data.
        /// 刪除資料
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public bool Delete(string rowKey, string partitionKey)
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(rowKey)) throw new ArgumentNullException(nameof(rowKey));

            try
            {
                TableOperation operation = TableOperation.Delete(new DTableEntity { RowKey = rowKey, PartitionKey = partitionKey, ETag = "*" });

                var res = CTable.ExecuteAsync(operation).Result;

                return true;
            }
            catch (Exception ex)
            {

                return !this.IsDataExisted(rowKey, partitionKey);

            }
            finally
            {
                // DataWriter.DelLock(Role.RoleKey, Collection, id);
            }


        }

        /// <summary>
        /// Delete data.
        /// 刪除資料
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        /// <param name="eTag"></param>
        /// <returns></returns>
        public bool DeleteWithEtag(string rowKey, string partitionKey, string eTag = "*")
        {

            //檢查參數是否傳入正確
            if (string.IsNullOrEmpty(rowKey)) throw new ArgumentNullException(nameof(rowKey));

            try
            {
                TableOperation operation = TableOperation.Delete(new DTableEntity { RowKey = rowKey, PartitionKey = partitionKey, ETag = eTag });

                var res = CTable.ExecuteAsync(operation).Result;

                return true;
            }
            catch (Exception ex)
            {

                return false;

            }
            finally
            {
                // DataWriter.DelLock(Role.RoleKey, Collection, id);
            }


        }


        /// <summary>
        /// Is DataExisted
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="partitionKey"></param>
        /// <param name="eTag"></param>
        /// <returns></returns>
        public bool IsDataExisted(string rowKey, string partitionKey)
        {
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
        /// 取得所有資料總數
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


        #region Dtor
        ~Operator()
        {
            CSAgent = null;
            CTClient = null;
            CTable = null;
            GC.Collect();
        }
        #endregion
    }


}

