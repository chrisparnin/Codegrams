using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Reader
{
    class ReadCommands
    {
        public static int WordCount(SQLiteConnection sqlite)
        {
            string sql = @"
                SELECT SUM(WordFreq)
                FROM [WordId] 
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public static string Escape(string value)
        {
            return value.Replace("'", "''");
        }

        public static int IdentifierCount(SQLiteConnection sqlite)
        {
            string sql = @"
                SELECT SUM(IdentFreq) 
                FROM [WordId]
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }


        public static int WordSequenceCount(SQLiteConnection sqlite)
        {
            string sql = @"
                SELECT SUM(WordSeqValue) 
                FROM [SeqFrequency]
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public static int IdentifierSequenceCount(SQLiteConnection sqlite)
        {
            string sql = @"
                SELECT SUM(IdentSeqValue) 
                FROM [SeqFrequency]
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }



        public static int LookupWordId(SQLiteConnection sqlite, string word)
        {
            string sql = @"
                SELECT Value 
                FROM [WordId] 
                WHERE [Word] = '{0}'
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = string.Format(sql, Escape(word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return -1;
        }

        public static int LookupWordFrequency(SQLiteConnection sqlite, string word)
        {
            string sql = @"
                SELECT WordFreq 
                FROM [WordId] 
                WHERE [Word] = '{0}'
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = string.Format(sql, Escape(word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                return reader.GetInt32(0);
            }
            return 0;

        }

        public static int LookupIdentifierFrequency(SQLiteConnection sqlite, string word)
        {
            string sql = @"
                SELECT IdentFreq 
                FROM [WordId] 
                WHERE [Word] = '{0}'
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = string.Format(sql, Escape(word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public static int LookupWordSequenceFrequency(SQLiteConnection sqlite, string phrase)
        {
            string sql = @"
                SELECT WordSeqValue 
                FROM [SeqFrequency] 
                WHERE [Key] = '{0}'
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = string.Format(sql, Escape(phrase));
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public static int LookupIdentifierSequenceFrequency(SQLiteConnection sqlite, string phrase)
        {
            string sql = @"
                SELECT IdentSeqValue 
                FROM [SeqFrequency] 
                WHERE [Key] = '{0}'
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = string.Format(sql, Escape(phrase));
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
    }
}
