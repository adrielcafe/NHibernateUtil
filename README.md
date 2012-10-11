NHibernateUtil
==============
This utility class makes the NHibernate + Fluent NHibernate more easier and fun!

This simple class comes pre configured for MySQL, PostgreSQL, SQL Server 2008, SQLite and Oracle 10g.

You can connect/disconnect and execute the basic CRUD (with batch support).


Requirements
------------
* Nhiberante 3 and later
* Fluent NHibernate 1.2 and later
* Database drivers for .NET platform that you will use
  * MySQL
  * PostgreSQL
  * SQL Server 2008
  * SQLite
  * Oracle 10g

How to use
----------
Let's use this POCO and this Fluent Map for the examples:
```csharp
public class Person
{
    public virtual int id { get; set; }
    public virtual string name { get; set; }
    public virtual int age{ get; set; }
}

public class MapPerson : ClassMap<Person>
{
    public MapPerson()
    {
        Table("people");
        Id(x => x.id);
        Map(x => x.name);
        Map(x => x.age);
    }
}
```

First of all we need to connect to the database. Below are possible ways to connect with the DBMS.
```csharp
NHUtil.Connect(DBMS.MySQL, "localhost", 3306, "test_nh", "root", "root");

NHUtil.Connect(DBMS.PostgreSQL, "localhost", 5432, "test_nh", "postgres", "root");

NHUtil.Connect(DBMS.SQLServer2008, @".\SQLEXPRESS", 0, "test_nh", null, null);

NHUtil.Connect(DBMS.SQLite, null, 0, @"c:\test_nh.db", null, null);

// Not yet been tested in Oracle
```

To disconnect:
```csharp
NHUtil.Disconnect();
```

**ATTENTION**: 


#### CRUD