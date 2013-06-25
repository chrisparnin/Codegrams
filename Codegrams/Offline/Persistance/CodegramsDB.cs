using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Persistance.Contracts;

namespace Codegrams.Persistance
{
    public class CodegramsDB
    {
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);
        }

        static CodegramsDB()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (Environment.Is64BitProcess) // .NET 4.0
            {
                path = Path.Combine(path, "Library", "x64", "SQLite.Interop.dll");
            }
            else
            {
                // X32
                path = Path.Combine(path, "Library", "x86", "SQLite.Interop.dll");
            }
            NativeMethods.LoadLibrary(path);
        }

        public static void Init(CodegramsContract contract, string path)
        {
            SQLiteConnection.CreateFile(path);
            var sqlite = new SQLiteConnection(string.Format("Data Source={0}",path));
            sqlite.Open();  //Initiate connection to the db

            CreateWordIdTable(sqlite);
            CreateFrequencyTable(sqlite);

            using (SQLiteTransaction transaction = sqlite.BeginTransaction())
            {
                foreach (var pair in contract.WordId)
                {
                    AddWordIdRow(sqlite, pair.Key, contract.KeyFrequency[pair.Key], pair.Value);
                }
                foreach (var pair in contract.SequenceFrequency)
                {
                    AddFrequencyRow(sqlite, pair.Key, pair.Value);
                }

                transaction.Commit();
            }
            sqlite.Close();
            Console.WriteLine();
        }

        private static void CreateWordIdTable(SQLiteConnection sqlite)
        {
            //                [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            string createWordIdTableSQL = @"
                CREATE TABLE IF NOT EXISTS [WordId](
                [Word] NVARCHAR NULL,
                [WordFreq] INT NULL,
                [IdentFreq] INT NULL,
                [Value] INT NULL
            );
            CREATE INDEX WordLookup ON WordId(Word);
            ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = createWordIdTableSQL;
            cmd.ExecuteNonQuery();
        }

        private static void CreateFrequencyTable(SQLiteConnection sqlite)
        {
            string createCodegramsTableSQL = @"
                CREATE TABLE IF NOT EXISTS [SeqFrequency](
                [Key] NVARCHAR NULL,
                [WordSeqValue] INT NULL,
                [IdentSeqValue] INT NULL
            );
            CREATE INDEX KeyLookup ON SeqFrequency(Key);
            ";

            var cmd = sqlite.CreateCommand();
            cmd.CommandText = createCodegramsTableSQL;  //set the passed query
            cmd.ExecuteNonQuery();
        }

        private static void AddFrequencyRow(SQLiteConnection sqlite, string key, SequenceFrequency value)
        {
            using (SQLiteCommand cmd = sqlite.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO SeqFrequency(Key,WordSeqValue,IdentSeqValue) VALUES(?,?,?)";

                SQLiteParameter Key = cmd.CreateParameter();
                SQLiteParameter WordSeqValue = cmd.CreateParameter();
                SQLiteParameter IdentSeqValue = cmd.CreateParameter();

                cmd.Parameters.Add(Key);
                cmd.Parameters.Add(WordSeqValue);
                cmd.Parameters.Add(IdentSeqValue);

                Key.Value = key;
                WordSeqValue.Value = value.WordSequenceFrequency;
                IdentSeqValue.Value = value.IdentifierSequenceFrequency;
                cmd.ExecuteNonQuery();
            }
        }

        private static void AddWordIdRow(SQLiteConnection sqlite, string word, KeyFrequency freq, int value)
        {
            using (SQLiteCommand cmd = sqlite.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO WordId(Word,WordFreq,IdentFreq,Value) VALUES(?,?,?,?)";

                SQLiteParameter Word = cmd.CreateParameter();
                SQLiteParameter WordFreq = cmd.CreateParameter();
                SQLiteParameter IdentFreq = cmd.CreateParameter();
                SQLiteParameter Value = cmd.CreateParameter();

                cmd.Parameters.Add(Word);
                cmd.Parameters.Add(WordFreq);
                cmd.Parameters.Add(IdentFreq);
                cmd.Parameters.Add(Value);

                Word.Value = word;
                WordFreq.Value = freq.WordFrequency;
                IdentFreq.Value = freq.IdentifierFrequency;
                Value.Value = value;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
