package com.chunta.rog.fetchitunes;

import android.app.Activity;
import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.widget.ImageView;

import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class imgDownload {

    FileStore imgfileAccess;
    ExecutorService executorService;

    public imgDownload(Context context){
        imgfileAccess=new FileStore(context);
        executorService=Executors.newFixedThreadPool(5);
    }

    public void DisplayImage(String url, ImageView imageView)
    {
        queueIMG(url, imageView);
    }

    private void queueIMG(String url, ImageView imageView)
    {
        IMGLoad p=new IMGLoad(url, imageView);
        executorService.submit(new IMGLoader(p));
    }

    private Bitmap getBitmap(String url)
    {
        Bitmap urlBitmap = null;
        //File imgfile=imgfileAccess.getIMG(url);
        try {
            urlBitmap = BitmapFactory.decodeFile(imgfileAccess.getIMG(url).getPath());
            if(urlBitmap!=null)
                return urlBitmap;
        } catch (Exception e) {

        }

        try {
            URL urlRead = new URL(url);
            HttpURLConnection connection = (HttpURLConnection) urlRead.openConnection();
            connection.setDoInput(true);
            connection.connect();
            InputStream input = connection.getInputStream();
            urlBitmap = BitmapFactory.decodeStream(input);
            imgfileAccess.storeImg(urlBitmap,url);
            return urlBitmap;
        } catch (Exception ex){
            ex.printStackTrace();
            return null;
        }
    }

    private class IMGLoad
    {
        public String url;
        public ImageView imageView;
        public IMGLoad(String u, ImageView i){
            url=u;
            imageView=i;
        }
    }

    class IMGLoader implements Runnable {
        IMGLoad imgLoad;
        IMGLoader(IMGLoad imgLoad){
            this.imgLoad=imgLoad;
        }

        @Override
        public void run() {
            Bitmap bmp=getBitmap(imgLoad.url);
            BitmapDisplayer bd=new BitmapDisplayer(bmp, imgLoad);
            Activity a=(Activity)imgLoad.imageView.getContext();
            a.runOnUiThread(bd);
        }
    }
    class BitmapDisplayer implements Runnable
    {
        Bitmap bitmap;
        IMGLoad imgLoad;
        public BitmapDisplayer(Bitmap b, IMGLoad p) {
            bitmap=b;
            imgLoad=p;
        }
        public void run()
        {
            imgLoad.imageView.setImageBitmap(bitmap);
        }
    }
}