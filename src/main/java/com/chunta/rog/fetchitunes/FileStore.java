package com.chunta.rog.fetchitunes;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.Log;

import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class FileStore {

    private File PicturesDir;

    public FileStore(Context context){
        PicturesDir = new File(context.getFilesDir() + "/Pictures/");
    }

    public File getIMG(String url) {

        String nameJPEG = String.valueOf(url.hashCode()) + ".JPEG";
        File imgFile = new File(PicturesDir, nameJPEG);
        //Log.d("file", imgFile.toString());
        return imgFile;
    }

    public void storeImg (Bitmap b, String url) {
        int imgName = url.hashCode();
        BufferedOutputStream outBitmap;
        File storeIMG = new File(PicturesDir.toString() + "/" + imgName +".JPEG");
        try {
            outBitmap = new BufferedOutputStream(new FileOutputStream(storeIMG));
            b.compress(Bitmap.CompressFormat.JPEG, 100, outBitmap);
            outBitmap.flush();
            outBitmap.close();
        } catch (FileNotFoundException e) {
            //e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
