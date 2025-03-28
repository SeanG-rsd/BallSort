using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;
using System.Text;
using System.Linq;

public class DatabaseTest : MonoBehaviour
{

    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader dbreader;  // not used in this example
    string DATABASE_NAME = "/database.s3db";

    static List<int> tube = new List<int>{ 1, 2, 3, 4 };
    List<List<int>> level = new List<List<int>>{ tube, tube, tube};
    // Start is called before the first frame update
    void Start()
    {
        string filepath = Application.dataPath + DATABASE_NAME;
        Debug.Log($"filepath={filepath}");
        conn = "URI=file:" + filepath;

        CreateATable();

        string info = "";
        for (int i = 0; i < level.Count; i++) 
        {
            info += string.Join(",", level[i]) + ",";
        }

        Debug.Log(info);

        InsertValue(info);

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
            sqlQuery = "CREATE TABLE IF NOT EXISTS [levels] (" +
                       "[level_index] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                       "[level] VARCHAR(255) NOT NULL)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
    }

    private void InsertValue(string value)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            string sqlQuery = "INSERT OR REPLACE INTO [levels] ([level]) VALUES (@level)";
        
		    dbcmd.CommandText = sqlQuery;
             dbcmd.Parameters.Add(new SqliteParameter("@level", value));
		    dbcmd.ExecuteNonQuery();
		    dbconn.Close();
        }
    }

    private void ReadTable()
    {
        string text = "Not Found";
        dbconn = new SqliteConnection(conn);
        dbconn.Open();

        string sqlQuery = "SELECT [level] FROM [levels] WHERE [level_index] = 1";
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;

        //dbcmd.Parameters.Add(new SqliteParameter("@name", "Sean G"));

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

        int sublistSize = 4;

        List<int> flatList = text.Split(',').Select(int.Parse).ToList();

        List<List<int>> nestedList = new List<List<int>>();
        for (int i = 0; i < flatList.Count; i += sublistSize)
        {
            nestedList.Add(flatList.GetRange(i, sublistSize));
        }
    }
}
