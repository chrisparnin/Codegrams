using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codegrams.Offline.Reader;

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
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public static string Escape(string value)
        {
            return value.Replace("'", "''").Replace("\\","\\\\");
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
            if (reader.Read())
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
            if (reader.Read())
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
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }


        public static int LookupWordId(SQLiteConnection sqlite, Cache cache, string word)
        {
            var cachedInt = cache.GetItemInt("id:"+word);
            if (cachedInt != null)
                return cachedInt.Value;
            
            string sql = @"
                SELECT Value 
                FROM [WordId] 
                WHERE [Word] = @Phrase
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@Phrase", word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var val = reader.GetInt32(0);
                cache.AddItem("id:"+word, val);
                return val;
            }
            cache.AddItem("id:"+word, -1);
            return -1;
        }

        public static int LookupWordFrequency(SQLiteConnection sqlite, Cache cache, string word)
        {
            var cachedInt = cache.GetItemInt("freq:" + word);
            if (cachedInt != null)
                return cachedInt.Value;

            string sql = @"
                SELECT WordFreq 
                FROM [WordId] 
                WHERE [Word] = @Phrase
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@Phrase", word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() && reader.HasRows)
            {
                var val= reader.GetInt32(0);
                cache.AddItem("freq:" + word, val);
                return val;
            }
            cache.AddItem("freq:" + word, 0);
            return 0;

        }

        public static int LookupIdentifierFrequency(SQLiteConnection sqlite, Cache cache, string word)
        {
            var cachedInt = cache.GetItemInt("idfreq:" + word);
            if (cachedInt != null)
                return cachedInt.Value;

            string sql = @"
                SELECT IdentFreq 
                FROM [WordId] 
                WHERE [Word] = @Phrase
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@Phrase", word));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() && reader.HasRows)
            {
                var val = reader.GetInt32(0);
                cache.AddItem("idfreq:" + word, val);
                return val;
            }
            cache.AddItem("idfreq:" + word, 0);
            return 0;
        }

        public static int LookupWordSequenceFrequency(SQLiteConnection sqlite, Cache cache, string phrase)
        {
            var cachedInt = cache.GetItemInt("wordseqfreq:" + phrase);
            if (cachedInt != null)
                return cachedInt.Value;

            string sql = @"
                SELECT WordSeqValue 
                FROM [SeqFrequency] 
                WHERE [Key] = @Phrase
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@Phrase", phrase));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() && reader.HasRows)
            {
                var val = reader.GetInt32(0);
                cache.AddItem("wordseqfreq:" + phrase, val);
                return val;
            }
            cache.AddItem("wordseqfreq:" + phrase, 0);
            return 0;
        }

        public static int LookupIdentifierSequenceFrequency(SQLiteConnection sqlite, Cache cache, string phrase)
        {
            var cachedInt = cache.GetItemInt("idseqfreq:" + phrase);
            if (cachedInt != null)
                return cachedInt.Value;

            string sql = @"
                SELECT IdentSeqValue 
                FROM [SeqFrequency] 
                WHERE [Key] = @Phrase
                ";
            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@Phrase", phrase));
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() && reader.HasRows)
            {
                var val = reader.GetInt32(0);
                cache.AddItem("idseqfreq:" + phrase, val);
                return val;
            }

            cache.AddItem("idseqfreq:" + phrase, 0);
            return 0;
        }
    }
}
