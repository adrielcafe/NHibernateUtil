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
// MySQL
NHUtil.Connect(DBMS.MySQL, "localhost", 3306, "test_nh", "root", "root");

// PostgreSQL
NHUtil.Connect(DBMS.PostgreSQL, "localhost", 5432, "test_nh", "postgres", "root");

// SQLServer 2008
NHUtil.Connect(DBMS.SQLServer2008, @".\SQLEXPRESS", 0, "test_nh", null, null);

// SQLite
NHUtil.Connect(DBMS.SQLite, null, 0, @"c:\test_nh.db", null, null);

// Oracle
// Not yet been tested
```

To disconnect:
```csharp
NHUtil.Disconnect();
```

**ATTENTION**: In most of cases you only need to connect and disconnect from database ONE TIME while your application is running.


#### CRUD
For **SELECT** all records:
```csharp
Person[] people = NHUtil.Select<Person>();
// or
List<Person> people = new List<Person>(NHUtil.Select<Person>());
```

For **SELECT** just one record by ID:
```csharp
Person person = NHUtil.Select<Person>(1);
```

The **INSERT**:
```csharp
Person person = new Person();
person.name = "Richard";
person.age = 23;
NHUtil.SaveOrUpdate(person);
```

The **UPDATE**:
```csharp
Person person = NHUtil.Select<Person>(1);
person.name = "Tom";
person.age = 34;
NHUtil.SaveOrUpdate(person);
```

The **DELETE**:
```csharp
Person person = NHUtil.Select<Person>(1);
NHUtil.Delete(person);
```

#### Batch INSERT, UPDATE and DELETE
