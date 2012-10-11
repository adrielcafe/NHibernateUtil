/*
    Copyright (c) 2012 Adriel Café <ac@adrielcafe.com>
    GitHub Repository: http://github.com/adrielcafe/NHibernateUtil
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace NHibernate.Util
{
    /// <summary>
    /// DBMSs supported
    /// </summary>
	public enum DBMS
	{
		MySQL,
		PostgreSQL,
		SQLite,
		SQLServer2008,
		Oracle10
	}

	public sealed class NHUtil
	{
		private static ISessionFactory sessionFactory;
		private static ITransaction transaction;
		private static Configuration config;
		private static IQuery query;

        /// <summary>
        /// Connects to database
        /// </summary>
        /// <param name="sgbd">The DBMS that will be connected</param>
        /// <param name="server">The server</param>
        /// <param name="port">The port</param>
        /// <param name="database">The database</param>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>True if succeed</returns>
		public static bool Connect(DBMS sgbd, string server, int port, string database, string username, string password)
		{
			try
			{
                string conStr = null;
                switch (sgbd)
                {
                    case DBMS.SQLServer2008:
                        conStr = string.Format("Data Source={0}; User Id={2}; Password={3}; Integrated Security=SSPI;",
                            server, username, password);
                        break;
                    case DBMS.Oracle10:
                        conStr = string.Format("Data Source={0}; Database={1}; User Id={2}; Password={3}; Integrated Security=SSPI;",
                            server, database, username, password);
                        break;
                    default:
                        string.Format("Server={0}; Port={1}; Database={2}; User ID={3}; Password={4};",
                            server, port, database, username, password);
                        break;
                }

				switch (sgbd)
				{
					case DBMS.MySQL:
						config = Fluently.Configure()
							.Database(MySQLConfiguration.Standard.ConnectionString(conStr)
								.IsolationLevel(IsolationLevel.ReadCommitted))
							.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
							.BuildConfiguration();
						break;

					case DBMS.PostgreSQL:
						config = Fluently.Configure()
							.Database(PostgreSQLConfiguration.Standard.ConnectionString(conStr)
								.IsolationLevel(IsolationLevel.ReadCommitted))
							.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
							.BuildConfiguration();
						break;

					case DBMS.SQLite:
						config = Fluently.Configure()
							.Database(SQLiteConfiguration.Standard.UsingFile(database)
								.IsolationLevel(IsolationLevel.ReadCommitted))
							.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
							.BuildConfiguration();
						break;

					case DBMS.SQLServer2008:
						config = Fluently.Configure()
							.Database(MsSqlConfiguration.MsSql2008.ConnectionString(conStr)
								.IsolationLevel(IsolationLevel.ReadCommitted))
							.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
							.BuildConfiguration();
						break;

					case DBMS.Oracle10:
						config = Fluently.Configure()
							.Database(OracleDataClientConfiguration.Oracle10.ConnectionString(conStr)
								.IsolationLevel(IsolationLevel.ReadCommitted))
							.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
							.BuildConfiguration();
                        break;
				}

                sessionFactory = Fluently.Configure(config).BuildSessionFactory();
                NHUtil.UpdateSchema();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error while connecting to database: " + e.Message);
				return false;
			}
		}

        /// <summary>
        /// Disconnects from the database
        /// </summary>
        /// <returns>True if succeed</returns>
		public static bool Disconnect()
		{
			if (sessionFactory == null)
				return true;
			else
				try
				{
					sessionFactory.Close();
					sessionFactory.Dispose();
					sessionFactory = null;
					return true;
				}
				catch { return false; }
		}

        /// <summary>
        /// Verify if is connected
        /// </summary>
        /// <returns>True if is connected</returns>
		public static bool IsConnected()
		{
			try
			{
				ISession session = OpenSession();
				return session.IsOpen && session.IsConnected;
			}
			catch { return false; }
		}
		
        /// <summary>
        /// Open a new session
        /// </summary>
        /// <returns>The session object</returns>
		private static ISession OpenSession()
		{
			try
			{
				return sessionFactory.OpenSession();
			}
			catch (Exception e)
			{
				Console.WriteLine("Can't open session: " + e.Message);
				return null;
			}
		}

        /// <summary>
        /// Close the informed session
        /// </summary>
        /// <param name="session"></param>
		private static void CloseSession(ISession session)
		{
			try
			{
				session.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Can't close session: " + e.Message);
			}
		}

        /// <summary>
        /// Update the database schema
        /// </summary>
        /// <returns>True if succeed</returns>
		public static bool UpdateSchema()
		{
			try
			{
				new SchemaUpdate(config).Execute(true, true);
				return true;
			}
			catch { return false; }
		}

        /// <summary>
        /// Reset the database schema. All the data will be erased!
        /// </summary>
        /// <returns>True if succeed</returns>
		public static bool ResetSchema()
		{
			try
			{
				new SchemaExport(config).Execute(true, true, false);
				return true;
			}
			catch { return false; }
		}
		
        /// <summary>
        /// Executes the SELECT operation
        /// </summary>
        /// <typeparam name="T">The Type that will be selected</typeparam>
        /// <returns>An array of objects</returns>
		public static T[] Select<T>()
		{
			List<T> lstObj = null;
			ISession session = null;
			query = null;

			try
			{
				session = OpenSession();
				session.CacheMode = CacheMode.Get;
				query = session.CreateQuery("From " + typeof(T));
				lstObj = (List<T>)query.List<T>();
			}
			catch (Exception e)
			{
				lstObj = null;
				Console.WriteLine("Error while selecting: " + e.Message);
			}
			finally
			{
				CloseSession(session);
			}

			return lstObj.ConvertAll(x => (T)x).ToArray();
		}

        /// <summary>
        /// Executes the SELECT ... WHERE id = ? operation
        /// </summary>
        /// <typeparam name="T">The Type that will be selected</typeparam>
        /// <param name="id">The ID of the register</param>
        /// <returns>An object</returns>
		public static T Select<T>(int id)
		{
			ISession session = null;
			object obj = null;

			try
			{
				session = OpenSession();
				session.CacheMode = CacheMode.Get;
				obj = session.Get(typeof(T), id);
			}
			catch (Exception e)
			{
				obj = null;
				Console.WriteLine("Error while selecting: " + e.Message);
			}
			finally
			{
				CloseSession(session);
			}
			return (T)obj;
		}

        /// <summary>
        /// Executes the INSERT or UPDATE operation
        /// </summary>
        /// <param name="obj">The object that will be persisted</param>
        /// <returns>True if succeed</returns>
		public static bool SaveOrUpdate(object obj)
		{
			ISession session = null;

			try
			{
				session = OpenSession();
				transaction = session.BeginTransaction();
				if (obj is object[])
					foreach (object x in obj as object[])
						session.SaveOrUpdate(x);
				else
					session.SaveOrUpdate(obj);
				transaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error while inserting/updating: " + e.Message);
				transaction.Rollback();
				CloseSession(session);
				return false;
			}
			finally
			{
				CloseSession(session);
			}

			return true;
		}

        /// <summary>
        /// Executes the DELETE operation
        /// </summary>
        /// <param name="obj">The object that will be deleted</param>
        /// <returns>True if succeed</returns>
		public static bool Delete(object obj)
		{
			ISession session = null;

			try
			{
				session = OpenSession();
				transaction = session.BeginTransaction();
				if (obj is object[])
					foreach (object x in obj as object[])
						session.Delete(x);
				else
					session.Delete(obj);
				transaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error while deleting: " + e.Message);
				transaction.Rollback();
				CloseSession(session);
				return false;
			}
			finally
			{
				CloseSession(session);
			}

			return true;
		}
	}
}