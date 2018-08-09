package com.chunta.rog.fetchitunes;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

public class SiteDB extends SQLiteOpenHelper {
	private static final String DATABASE_NAME = "sites";
	private static final int DATABASE_VERSION = 1;
	private static final String TABLE_NAME = "TopMusicTW";
	private static final String TABLE_CREATE =
			"CREATE TABLE " + TABLE_NAME + " ( " +" picture VARCHAR(500) NOT NULL, " +
					" nameArtist VARCHAR(100) NOT NULL, " +" nameSong VARCHAR(100) NOT NULL, " +
					"rank VARCHAR(5) NOT NULL, PRIMARY KEY (rank)); ";
	private static final String COL_picture = "picture";
	private static final String COL_nameArtist = "nameArtist";
	private static final String COL_nameSong = "nameSong";
	private static final String COL_rank = "rank";
	//private static final String COL_nameImg = "nameImg";
	public SiteDB(Context context) {
		super(context, DATABASE_NAME, null, DATABASE_VERSION);
		// TODO Auto-generated constructor stub
	}

	@Override
	public void onCreate(SQLiteDatabase db) {
		// TODO Auto-generated method stub
		db.execSQL(TABLE_CREATE);
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
		// TODO Auto-generated method stub
		db.execSQL("DROP TABLE IF EXISTS " + TABLE_NAME);
		onCreate(db);
	}
	/** insert new data to database **/
	public long insertDB(Site Site){
		SQLiteDatabase db = getWritableDatabase();
		ContentValues values = new ContentValues();
		values.put(COL_picture, Site.getPicture());
		values.put(COL_nameArtist, Site.getNameArtist());
		values.put(COL_nameSong, Site.getNameSong());
		values.put(COL_rank, Site.getRank());
		long count = db.insert(TABLE_NAME, null, values);
		db.close();
		return count;
		
	}
	/** modify data **/
	public int updateDB(Site Site){
		SQLiteDatabase db = getWritableDatabase();
		ContentValues values = new ContentValues();
		values.put(COL_picture, Site.getPicture());
		values.put(COL_nameArtist, Site.getNameArtist());
		values.put(COL_nameSong, Site.getNameSong());
		String whereClause = COL_rank + "='"+ Site.getRank()+"'";
		int count =db.update(TABLE_NAME, values, whereClause, null);
		db.close();
		return count;
	}
	/** delete data according to rank(primary key) **/
	public int deleteDB(String rank){
		SQLiteDatabase db = getWritableDatabase();
		String whereClause = COL_rank + "='" + rank + "'";
		int count = db.delete(TABLE_NAME, whereClause, null);
		db.close();
		return count;
	}

	/** check this rank row has data**/
	public String getRank(int Rank) {
	    String result="NoExist";
        SQLiteDatabase db = getReadableDatabase();
        String sql = "SELECT rank FROM "+ TABLE_NAME + " WHERE rank LIKE ?";
        String [] args = { Integer.toString(Rank)};
        Cursor cursor = db.rawQuery(sql, args);
        //Log.d("SiteDB", "getRank, Rank: " + Rank + " cursor.getCount: " + cursor.getCount());
        if (cursor.getCount() == 1)
            result = "Exist";
        db.close();
        cursor.close();
        return result;
    }

	/** query all data according to choosing picture, nameArtist, and nameSong show in Show activity  **/
	public ArrayList<Map<String, String>>  getTopMusicInfo() {

		SQLiteDatabase db = getReadableDatabase();
		String sql = "SELECT picture, nameArtist, nameSong FROM "+ TABLE_NAME;
		Cursor cursor = db.rawQuery(sql, null);

		ArrayList<Map<String, String>> arrayListItem = new ArrayList<Map<String, String>>();
		while(cursor.moveToNext()){
			Map<String, String> itemInfo = new HashMap<String, String>();
			itemInfo.put("imageView", cursor.getString(0));
			itemInfo.put("textView2", cursor.getString(1));
			itemInfo.put("textView1", cursor.getString(2));
			//Log.d("DB", itemInfo.get("imageView").toString() + "  " + itemInfo.get("textView2").toString() + "  " + itemInfo.get("textView1").toString());
			arrayListItem.add(itemInfo);
			}
		cursor.close();
		db.close();
		return arrayListItem;
	}
}
