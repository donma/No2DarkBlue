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

Update Data:
--

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

