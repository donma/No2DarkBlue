# No2DarkBlue
----
一個讓您更簡單使用 [WindowsAzure.Storage](https://www.nuget.org/packages/WindowsAzure.Storage/) 的工具，其中主要幫助了幾個問題

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
Delete DAta With Confirm Etag -

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

AllDataKeys - Get All  DTableEntity Datas

```csharp
var q = new No2DarkBlue.Query<User>(conn, "SAMPLETABLE", false);
var res = q.AllDataKeys();

if (res != null)
{
    Response.Write("COUNT:"+res.Count() + "<br>");
    foreach(var c in res)
    {
        Response.Write(c.PartitionKey+","+c.RowKey)
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

AllDataKeysByPK - Get All Datas by PartitionKey

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

AllPKs - Get All Partition Keys

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




