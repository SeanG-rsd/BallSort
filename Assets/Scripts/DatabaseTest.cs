using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;
using System.Text;

public class DatabaseTest : MonoBehaviour
{

    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader dbreader;  // not used in this example
    string DATABASE_NAME = "/database.s3db";
    // Start is called before the first frame update
    void Start()
    {
        string filepath = Application.dataPath + DATABASE_NAME;
        Debug.Log($"filepath={filepath}");
        conn = "URI=file:" + filepath;

        //CreateATable();

        //InsertValue();

        ReadTable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateATable()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            sqlQuery = "CREATE TABLE IF NOT EXISTS [my_table] (" +
                       "[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                       "[name] VARCHAR(255)  NOT NULL," +
                       "[age] INTEGER DEFAULT '18' NOT NULL)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
    }

    private void InsertValue()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            string sqlQuery = "INSERT OR REPLACE INTO [my_table] ([name]) VALUES (@name)";
        
		    dbcmd.CommandText = sqlQuery;
             dbcmd.Parameters.Add(new SqliteParameter("@name", "Sean G"));
		    dbcmd.ExecuteNonQuery();
		    dbconn.Close();
        }
    }

    private void ReadTable()
    {
        string text = "Not Found";
        dbconn = new SqliteConnection(conn);
        dbconn.Open();

        string sqlQuery = "SELECT [name] FROM [my_table] WHERE [name] = @name";
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;

        dbcmd.Parameters.Add(new SqliteParameter("@name", "Sean G"));

        dbreader = dbcmd.ExecuteReader();
		if (dbreader.Read())
            {
                text = dbreader.GetString(0); // Assuming you're retrieving the first column (id)
            }
            else
            {
                Debug.Log("QueryString - nothing to read...");
            }
		dbreader.Close();
		dbconn.Close();

        Debug.Log("Read Table: " + text);
    }
}
