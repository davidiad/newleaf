using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ES3Internal;

public class ES3Spreadsheet
{
	private int cols = 0;
	private int rows = 0;
	private Dictionary<Index, string> cells = new Dictionary<Index, string>();

	private const string QUOTE = "\"";
	private const string ESCAPED_QUOTE = "\"\"";
	private static char[] CHARS_TO_ESCAPE = { ',', '"', '\n' };

	public void SetCell<T>(int col, int row, object value)
	{
		var settings = new ES3Settings ();
		using(var ms = new MemoryStream())
		{
			using (var jsonWriter = new ES3JSONWriter (ms, settings, false, false))
				jsonWriter.Write(value, ES3.ReferenceMode.ByValue);
			cells [new Index (col, row)] = settings.encoding.GetString(ms.ToArray());
		}

		// Expand the spreadsheet if necessary.
		if((col+1) > cols)
			cols = (col+1);
		if((row+1) > rows)
			rows = (row+1);
	}

	public T GetCell<T>(int col, int row)
	{
		string value;
		if(!cells.TryGetValue(new Index (col, row), out value))
			throw new KeyNotFoundException("Cell with index ("+ col + " ,"+ row +") was not found.");

		var settings = new ES3Settings ();
		using(var ms = new MemoryStream(settings.encoding.GetBytes(value)))
			using (var jsonReader = new ES3JSONReader(ms, settings, false))
				return jsonReader.Read<T>();
	}

	// TODO
	/*public void Load()
	{
		// Don't forget to unescape strings using Unescape() method when loading!
	}*/

	public void Save(string filePath)
	{
		Save(new ES3Settings (filePath), false);
	}

	public void Save(string filePath, ES3Settings settings)
	{
		Save(new ES3Settings (filePath, settings), false);
	}

	public void Save(ES3Settings settings)
	{
		Save(settings, false);
	}

	public void Save(string filePath, bool append)
	{
		Save(new ES3Settings (filePath), append);
	}

	public void Save(string filePath, ES3Settings settings, bool append)
	{
		Save(new ES3Settings (filePath, settings), append);
	}

	public void Save(ES3Settings settings, bool append)
	{
		using (var writer = new StreamWriter(ES3Stream.CreateStream(settings, append ? ES3FileMode.Append : ES3FileMode.Write)))
		{
			// If data already exists and we're appending, we need to prepend a newline.
			if(append && ES3.FileExists(settings))
				writer.Write('\n');

			var array = ToArray();
			for(int row = 0; row < rows; row++)
			{
				if(row != 0)
					writer.Write('\n');

				for(int col = 0; col < cols; col++)
				{
					if(col != 0)
						writer.Write(',');
					writer.Write( Escape(array [col, row]) );
				}
			}
		}
		if(!append)
			ES3IO.CommitBackup(settings);
	}

	private static string Escape(string str)
	{
		if(str == null)
			return null;
		if(str.Contains(QUOTE))
			str = str.Replace(QUOTE, ESCAPED_QUOTE);
		if(str.IndexOfAny(CHARS_TO_ESCAPE) > -1)
			str = QUOTE + str + QUOTE;
		return str;
	}

	private static string Unescape(string str)
	{
		if(str.StartsWith(QUOTE) && str.EndsWith(QUOTE))
		{
			str = str.Substring(1, str.Length-2);
			if(str.Contains(ESCAPED_QUOTE))
				str = str.Replace(ESCAPED_QUOTE, QUOTE);
		}
		return str;
	}

	private string[,] ToArray()
	{
		var array = new string[cols, rows];
		foreach (var cell in cells)
			array [cell.Key.col, cell.Key.row] = cell.Value;
		return array;
	}

	protected struct Index
	{
		public int col;
		public int row;

		public Index(int col, int row)
		{
			this.col = col;
			this.row = row;
		}
	}
}
