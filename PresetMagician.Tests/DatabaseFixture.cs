using System;

namespace PresetMagician.Tests
{
    public class DatabaseFixture: IDisposable
    {
        public DatabaseFixture()
        {
           // Db = new SqlConnection("MyConnectionString");

            // ... initialize data in the test database ...
        }

        public void Dispose()
        {
            // ... clean up test data from the database ...
        }

    }
}