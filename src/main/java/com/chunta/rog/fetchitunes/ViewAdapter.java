package com.chunta.rog.fetchitunes;

import android.app.Activity;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.Map;

public class ViewAdapter extends BaseAdapter {

    private Activity activity;
    private ArrayList<Map<String, String>> arrlistview;
    private static LayoutInflater mainLayout=null;
    public imgDownload imgLoad;

    public ViewAdapter(Activity a, ArrayList<Map<String, String>> d)
    {
        activity = a;
        arrlistview = d;
        this.mainLayout = (LayoutInflater)activity.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        imgLoad = new imgDownload(activity.getApplicationContext());

    }

    @Override
    public int getCount() {
        return arrlistview.size();
    }

    @Override
    public Object getItem(int position) {
        return position;
    }

    @Override
    public long getItemId(int position) {
        return position;
    }

    // return listview
    @Override
    public View getView(int position, View convertView, ViewGroup parent) {

        TopMusic.ViewHolder holder = null;
        if (convertView == null)
        {
            holder = new TopMusic.ViewHolder();
            convertView = mainLayout.inflate(R.layout.listview_content, null);
            holder.img = (ImageView) convertView.findViewById(R.id.imageView);
            holder.songName = (TextView) convertView.findViewById(R.id.textView1);
            holder.artistName = (TextView) convertView.findViewById(R.id.textView2);
            convertView.setTag(holder);
        } else {
            holder = (TopMusic.ViewHolder) convertView.getTag();
        }
        holder.songName.setText((String)arrlistview.get(position).get("textView1"));
        holder.artistName.setText((String)arrlistview.get(position).get("textView2"));
        imgLoad.DisplayImage(arrlistview.get(position).get("imageView"), holder.img);

        return convertView;
    }

}


