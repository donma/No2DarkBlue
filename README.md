# No2DarkBlue
----
一個讓您更簡單使用 [WindowsAzure.Storage](https://www.nuget.org/packages/WindowsAzure.Storage/) 的工具，主要讓你可以更簡單去使用 Azure Storage Table.

Happy Coding :)

DTableEntity
---

Example :

```csharp

    public class Foo : DTableEntity
    {

        public string Name { get; set; }
        public DateTime Birth { get; set; } // Auto Convert To Local Time
        public decimal Salary { get; set; } // Convert decimal to string on Table
        public List<Foo> Friend { get; set; } // Auto Serialize to string on Table , Auto Deserialize from Table
        
    }
    
``` 
## Operator

Update Data - 

```csharp

            //conn is your connection string
            var op = new No2DarkBlue.Operator(conn, "SAMPLETABLE");

            var obj = new Foo();

            obj.PartitionKey = "PK1";
            obj.RowKey = "RK1";
            obj.Name = "許當麻";
            obj.Salary = 123456789.987654321m;
            obj.Birth = new DateTime(1983, 05, 11);

            var friend = new Foo();
            friend.Name = "許當麻的朋友1";
            friend.Salary = 123456789.987654321m;
            friend.Birth = new DateTime(1983, 05, 11);

            obj.Friend = new System.Collections.Generic.List<Foo>();
            obj.Friend.Add(friend);

            op.Update(obj);
    
```

Update Data With Confirm ETag - 

```csharp

op.UpdateWithConfirmETag(obj);
    
```

Delete Data -

```csharp

op.Delete("RK1", "PK1");
    
```
Delete Data With Confirm Etag -

```csharp

op.DeleteWithEtag("RK", "PK", obj.ETag);
    
```
## Query

IsDataExisted - Check PK and RK Data is Existed.

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
Response.Write(q.IsDataExisted("USER999", "GROUP10") + "<br>");
Response.Write(q.IsDataExisted("USER111111", "GROUP1") + "<br>");
    
```

AllDataCount -  Get All Table Count 

```csharp
var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
Response.Write(q.AllDataCount()+" ! ");

```

AllDataKeys - Get All  DTableEntity Data Keys ,  return is DTableEntity[]

```csharp
var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllDataKeys();

if (res != null)
{
    Response.Write("COUNT:"+res.Count() + "<br>");
    foreach(var c in res)
    {
        Response.Write(c.PartitionKey+","+c.RowKey);
    }
}
```

AllDataKeysByRowKey - 

```csharp
var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllDataKeysByRK("USER11");
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}
```

AllDataKeysByPK - Get All Data Keys by PartitionKey , return is DTableEntity[]

```csharp
var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllDataKeysByPK("GROUP2");
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

AllPKs - Get All Partition Keys , return is string[]

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllPKs();
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

AllDatas - Get All Table Datas

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllDatas();
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

DatasByPKRKs - Get Datas by PartitionKey and RowKey Pair

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);

var res = q.DatasByPKRKs(new No2DarkBlue.TableKeyPair("USER99", "GROUP1"), new No2DarkBlue.TableKeyPair("USER995", "GROUP10"), new No2DarkBlue.TableKeyPair("DATA_NOT_EXISTE", "GROUP1"));

if (res.Count()>0)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

DataByPKRK - Get One Data by PartitionKey and RowKey

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);

var res1 = q.DataByPKRK("USER99", "GROUP1");

if (res1 != null)
{
    Response.Write(JsonConvert.SerializeObject(res1) + "<br>");
}
else {
    Response.Write("RES1 NO DATA" + "<br>");
}

var res2 = q.DataByPKRK("DATANOTEXISTED", "GROUP1");
if (res2 != null)
{
    Response.Write(JsonConvert.SerializeObject(res2) + "<br>");
}
else
{
    Response.Write("RES2 NO DATA" + "<br>");
}

```

DatasByPK - Get All Data By PartitionKey

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.DatasByPK("GROUP4");
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

DatasByRK - Get All Data By RowKey

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.DatasByRK("USER99");
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

## Advanced Query

DatasByExpression

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE");
var res = q.DatasByExpression(x => x.Age > 0);
if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

DatasByFilterCondition

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE");
var res = q.DatasByFilterCondition(
        TableQuery.GenerateFilterConditionForInt("Age", QueryComparisons.GreaterThanOrEqual, 950),
        new string[] { TableOperators.And, TableOperators.And },
        new string[] { TableQuery.GenerateFilterCondition("Name", QueryComparisons.GreaterThanOrEqual, "USER9"),
                        TableQuery.GenerateFilterConditionForBool("IsSingle", QueryComparisons.Equal, true)
                        }

    );

if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

DatasByFilterString

```csharp

var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE");
var res = q.DatasByFilterString("Age ge 950 and Age lt 1000");

if (res != null)
{
    Response.Write("COUNT:" + res.Count() + "<br>");
    foreach (var c in res)
    {
        Response.Write(JsonConvert.SerializeObject(c) + "<br>");

    }
}

```

Happy Coding :)











