package com.chunta.rog.fetchitunes;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import android.os.AsyncTask;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

public class TopMusic extends AppCompatActivity {
    private Button reFresh;
    private ListView showTopLV;
    private NetworkInfo netInfo;
    private ConnectivityManager CM;
    private File filesDir, PicturesDir;
    private Handler mUI_Handler = new Handler();
    private Handler mThreadHandler;
    private HandlerThread mThread;
    private SiteDB dbHlp;
    private URL url;
    private JSONArray jsonArr;
    private BufferedInputStream in;
    private BufferedOutputStream out;
    private String[] artistNameArr,songName,pictureURL;
    private ViewAdapter viewAdapter;
    private ArrayList<Map<String, String>> arrlistview = new ArrayList<Map<String, String>>();
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_top_music);

        CM = (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);      //get system service
        connectDB();        //connect database
        reFresh = (Button) findViewById(R.id.button);
        showTopLV = (ListView) findViewById(R.id.listview);

        // set refresh button as Enable/Disable
        netInfo = CM.getActiveNetworkInfo();
        if (netInfo == null)
            reFresh.setEnabled(false);
        else
            reFresh.setEnabled(true);

        filesDir = getFilesDir();
        PicturesDir = new File(getFilesDir() + "/Pictures/");
        if (!PicturesDir.exists()) {
            PicturesDir.mkdirs();
            Log.d("dir", PicturesDir.toString());
        }
        viewAdapter = new ViewAdapter(this,arrlistview);
        mThread = new HandlerThread("name");
        mThread.start();
        mThreadHandler = new Handler(mThread.getLooper());
        initialView();

    }

    @Override
    protected void onStart() {
        super.onStart();
    }

    @Override
    protected void onResume() {
        super.onResume();
        reFresh = (Button) findViewById(R.id.button);
        if(dbHlp == null)
            dbHlp = new SiteDB(this);

        // set refresh button as Enable/Disable
        netInfo = CM.getActiveNetworkInfo();
        if (netInfo == null)
            reFresh.setEnabled(false);
        else
            reFresh.setEnabled(true);

        reFresh.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                mThreadHandler.post(downloadRSSJson);
            }
        });
    }

    private Runnable downloadRSSJson=new Runnable () {
        public void run() {
            // TODO Auto-generated method stub
            byte[] data = new byte[1];
            try {
                url = new URL("https://rss.itunes.apple.com/api/v1/tw/itunes-music/top-songs/all/100/explicit.json");
                in = new BufferedInputStream( url.openConnection().getInputStream() );
                out = new BufferedOutputStream( new FileOutputStream(filesDir.toString() + "/topMusic.json") );
                while( in.read(data) != -1) {
                    out.write(data);
                }
                out.flush();
                out.close();
                in.close();
            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }
            mUI_Handler.post(parserJson);
        }
    };

    private void initialView () {
        netInfo = CM.getActiveNetworkInfo();    //get device network status
        Log.d("test", "check");
        if ( dbHlp.getRank(1).equals("NoExist") && netInfo != null) {
            Log.d("test", "auto download");
            mThreadHandler.post(downloadRSSJson);
        } else {
            arrlistview = dbHlp.getTopMusicInfo();
            ViewAdapter viewAdapter = new ViewAdapter(this,arrlistview);
            Log.d("setAdapter", "update from offline");
            showTopLV.setAdapter(viewAdapter);
        }
    }
    private void displayListView () {
        for (int i=1; i <= jsonArr.length(); i++)
        {
            Map<String, String> item = new HashMap<String, String>();
            item.put("imageView", pictureURL[i]);
            item.put("textView1", songName[i]);
            item.put("textView2", artistNameArr[i]);
            arrlistview.add(item);
        }
        ViewAdapter viewAdapter = new ViewAdapter(this,arrlistview);
        //Log.d("setAdapter", "update from online");
        showTopLV.setAdapter(viewAdapter);
    }

    private Runnable parserJson=new Runnable () {

        public void run() {

            File file = new File(filesDir.toString() + "/topMusic.json");
            try {
                java.io.FileReader reader = new java.io.FileReader(file);
                StringBuffer buff = new StringBuffer();
                char[] c = new char[256];
                int length;
                while ((length = reader.read(c)) > 0) {
                    buff.append(c);
                }
                reader.close();
                artistNameArr = new String[101];
                songName = new String[101];
                pictureURL = new String[101];
                JSONObject jsonObject = new JSONObject(buff.toString());
                String jsonFeed = jsonObject.getString("feed");
                JSONObject jsonObject2 = new JSONObject(jsonFeed);
                jsonArr = jsonObject2.getJSONArray("results");
                for (int i=0; i<jsonArr.length();i++) {
                    int rank = i + 1;
                    JSONObject jsonSubObject = jsonArr.getJSONObject(i);
                    artistNameArr[rank] = jsonSubObject.getString("artistName");
                    songName[rank] = jsonSubObject.getString("name");
                    pictureURL[rank] = jsonSubObject.getString("artworkUrl100");
                    /*
                    Log.d("topMusic", "Title: " + songName[rank] + " ; artistName: "
                            + artistNameArr[rank] + " ; Album:  " + String.valueOf(pictureURL[rank].hashCode()));
                    */
                    new DownloadImgTask().execute(pictureURL[rank]); // download img
                    Site Site = new Site(pictureURL[rank], artistNameArr[rank], songName[rank], Integer.toString(rank));
                    if ( dbHlp.getRank(rank).equals("NoExist")) {
                        long count = dbHlp.insertDB(Site);
                        if(count == -1)
                            Log.e("SQLite", "Fail to Insert DB. Rank: " + Integer.toString(rank));
                    } else {
                        int result = dbHlp.updateDB(Site);
                        if ( result == -1 )
                            Log.e("SQLite", "Fail to update DB. Rank: " + Integer.toString(rank));
                    }

                }
            } catch (Exception e) {
                e.printStackTrace();
            }
            displayListView();
        }

    };

    private class DownloadImgTask extends AsyncTask<String, String, String> {
        @Override
        protected String doInBackground(String... param) {
            Bitmap urlBitmap;
            try {
                File imgFile = new File (PicturesDir.toString()+ "/" + param[0].hashCode()+".JPEG");
                if (!imgFile.exists()) {
                    URL urlRead = new URL(param[0]);
                    HttpURLConnection connection = (HttpURLConnection) urlRead.openConnection();
                    connection.setDoInput(true);
                    connection.setConnectTimeout(30000);
                    connection.setReadTimeout(30000);
                    connection.connect();
                    InputStream input = connection.getInputStream();
                    urlBitmap = BitmapFactory.decodeStream(input);
                    BufferedOutputStream outBitmap;
                    outBitmap = new BufferedOutputStream(new FileOutputStream(imgFile));
                    urlBitmap.compress(Bitmap.CompressFormat.JPEG, 100, outBitmap);
                    outBitmap.flush();
                    outBitmap.close();
                    return "Success";
                }
                return "NoDownload";
            } catch (Exception e){
                e.printStackTrace();
                return "fail";
            }
        }

        protected void onPostExecute(String result) {
            /*
            if (result.equals("fail"))
                Log.d("AsyncTask", "Download Img Fail.");
            else if (result.equals("NoDownload"))
                Log.d("AsyncTask", "No Download Img.");
                */
        }
    }

    static class ViewHolder
    {
        public ImageView img;
        public TextView songName;
        public TextView artistName;
    }

    /** connect database **/
    private void connectDB() {
        if(dbHlp == null)
            dbHlp = new SiteDB(this);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_top_music, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (dbHlp != null) {
            dbHlp.close();
            dbHlp = null;
        }
    }

    @Override
    protected void onStop() {
        super.onStop();
    }
     @Override
     protected void onDestroy() {
            super.onDestroy();
    }
}
