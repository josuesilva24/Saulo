using System;
using System.Data.SqlClient;
using System.IO;

namespace DAL
{
    public class Connection
    {
        public Connection()
        {
            TxtValues = File.ReadAllLines(@"Conexion.txt");
        }

        private string ServerName { get { return TxtValues[0].Split('=')[1]; } }
        private string DbName { get { return TxtValues[1].Split('=')[1]; } }
        private string UserName { get { return TxtValues[2].Split('=')[1]; } }
        private string Password { get { return TxtValues[3].Split('=')[1]; } }
        private string[] TxtValues { get; set; } 

 #region Setting Connection
        private string ConnectionString
        {
            get { return "Server=" + ServerName + ";Database=" + DbName + ";User Id=" + UserName + "; Password=" + Password + "; Connection Timeout=4000"; }
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        public SqlCommand GetReader(string query)
        {
            var cnn = GetConnection();
            cnn.Open();
            return new SqlCommand(query, cnn);
        }
        public void CloseReader()
        {
            var cnn = GetConnection();
            cnn.Close();
        }
#endregion



#region tests
        private void test()
        {
            // Create the connection to the resource!
            // This is the connection, that is established and
            // will be available throughout this block.
            using (var conn = new SqlConnection())
            {
                // Create the connectionString
                // Trusted_Connection is used to denote the connection uses Windows Authentication
                conn.ConnectionString = "Server=[server_name];Database=[database_name];Trusted_Connection=true";
                conn.Open();
                // Create the command
                SqlCommand command = new SqlCommand("SELECT * FROM TableName WHERE FirstColumn = @0", conn);
                // Add the parameters.
                command.Parameters.Add(new SqlParameter("0", 1));

                /* Get the rows and display on the screen! 
                 * This section of the code has the basic code
                 * that will display the content from the Database Table
                 * on the screen using an SqlDataReader. */

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("FirstColumn\tSecond Column\t\tThird Column\t\tForth Column\t");
                    while (reader.Read())
                    {
                        Console.WriteLine(String.Format("{0} \t | {1} \t | {2} \t | {3}",
                            reader[0], reader[1], reader[2], reader[3]));
                    }
                }
                Console.WriteLine("Data displayed! Now press enter to move to the next section!");
                Console.ReadLine();
                Console.Clear();

                /* Above code was used to display the data from the Database table!
                 * This following section explains the key features to use 
                 * to add the data to the table. This is an example of another
                 * SQL Command (INSERT INTO), this will teach the usage of parameters and connection.*/

                Console.WriteLine("INSERT INTO command");

                // Create the command, to insert the data into the Table!
                // this is a simple INSERT INTO command!

                SqlCommand insertCommand = new SqlCommand("INSERT INTO TableName (FirstColumn, SecondColumn, ThirdColumn, ForthColumn) VALUES (@0, @1, @2, @3)", conn);

                // In the command, there are some parameters denoted by @, you can 
                // change their value on a condition, in my code they're hardcoded.

                insertCommand.Parameters.Add(new SqlParameter("0", 10));
                insertCommand.Parameters.Add(new SqlParameter("1", "Test Column"));
                insertCommand.Parameters.Add(new SqlParameter("2", DateTime.Now));
                insertCommand.Parameters.Add(new SqlParameter("3", false));

                // Execute the command, and print the values of the columns affected through
                // the command executed.

                Console.WriteLine("Commands executed! Total rows affected are " + insertCommand.ExecuteNonQuery());
                Console.WriteLine("Done! Press enter to move to the next step");
                Console.ReadLine();
                Console.Clear();

                /* In this section there is an example of the Exception case
                 * Thrown by the SQL Server, that is provided by SqlException 
                 * Using that class object, we can get the error thrown by SQL Server.
                 * In my code, I am simply displaying the error! */
                Console.WriteLine("Now the error trial!");
                
                // try block
                try
                {
                    // Create the command to execute! With the wrong name of the table (Depends on your Database tables)
                    SqlCommand errorCommand = new SqlCommand("SELECT * FROM someErrorColumn", conn);
                    // Execute the command, here the error will pop up!
                    // But since we're catching the code block's errors, it will be displayed inside the console.
                    errorCommand.ExecuteNonQuery();
                }
                // catch block
                catch (SqlException er)
                {
                    // Since there is no such column as someErrorColumn (Depends on your Database tables)
                    // SQL Server will throw an error.
                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }
            // Final step, close the resources flush dispose them. ReadLine to prevent the console from closing.
            Console.ReadLine();
        }
#endregion
        
    }
}